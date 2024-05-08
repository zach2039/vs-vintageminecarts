using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using VintageMinecarts.ModUtil;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageMinecarts.ModEntity
{
	public class EntityMinecart : Entity, IRenderer, IDisposable, IMountableSupplier
	{
		public EntityMinecart()
		{
			this.AnimManager = new AnimationManager();
            this.Seat = new EntityMinecartSeat(this, SeatMountOffset)
            {
                CanControl = true
            };
        }

		public override bool CanCollect(Entity byEntity)
		{
			return false;
		}

		public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
		{
			base.Initialize(properties, api, InChunkIndex3d);

			this.cApi = (api as ICoreClientAPI);
			
			if (cApi != null)
			{
				this.cApi.Event.RegisterRenderer(this, EnumRenderStage.Before, "minecartsim");
				this.modsysSounds = api.ModLoader.GetModSystem<ModSystemMinecartSound>(true);
			}

			if (this.Seat.PassengerEntityIdForInit != 0L && this.Seat.Passenger == null)
			{
				EntityAgent entityAgent = api.World.GetEntityById(this.Seat.PassengerEntityIdForInit) as EntityAgent;
				if (entityAgent != null)
				{
					entityAgent.TryMount(this.Seat);
				}
			}

			if (api.Side == EnumAppSide.Server)
			{
				this.AnimManager = AnimationCache.InitManager(api, this.AnimManager, this, properties.Client.LoadedShapeForEntity, null, new string[]
				{
					"head"
				});
				this.AnimManager.OnServerTick(0f);
			}
			else
			{
				this.AnimManager.Init(api, this);
			}
		}

		public override void OnGameTick(float dt)
		{
			if (this.World.Side == EnumAppSide.Server)
			{
				this.updateMinecartAngleAndMotion(dt);
			}

			base.OnGameTick(dt);
		}

		public Vec3f SeatMountOffset = new Vec3f(0f, 0.2f, 0f);

		public virtual Vec3f GetMountOffset(Entity entity)
		{
			if (this.Seat.Passenger == entity)
			{
				return this.Seat.MountOffset;
			}

			return null;
		}

		public bool IsMountedBy(Entity entity)
		{
			if (this.Seat.Passenger == entity)
			{
				return true;
			}

			return false;
		}

		public virtual bool IsEmpty()
		{
			return this.Seat.Passenger == null;
		}

		public override void ToBytes(BinaryWriter writer, bool forClient)
		{
			base.ToBytes(writer, forClient);
			EntityAgent passenger = this.Seat.Passenger;
			writer.Write((passenger != null) ? passenger.EntityId : 0L);
		}

		public override void FromBytes(BinaryReader reader, bool fromServer)
		{
			base.FromBytes(reader, fromServer);
			long entityId = reader.ReadInt64();
			this.Seat.PassengerEntityIdForInit = entityId;
		}

		public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
		{
			if (mode != EnumInteractMode.Interact)
			{
				return;
			}

			if (byEntity.Controls.Sneak && this.IsEmpty())
			{
				EntityAgent passenger = this.Seat.Passenger;
				if (passenger != null)
				{
					passenger.TryUnmount();
				}
				
				ItemStack stack = new ItemStack(this.World.GetItem(this.Code), 1);
				if (!byEntity.TryGiveItemStack(stack))
				{
					this.World.SpawnItemEntity(stack, this.ServerPos.XYZ, null);
				}
				this.Die(EnumDespawnReason.Death, null);
				return;
			}
			if (this.World.Side == EnumAppSide.Server)
			{
				if (byEntity.MountedOn == null && this.Seat.Passenger == null)
				{
					byEntity.TryMount(this.Seat);
				}
			}
		}

		public virtual Vec2d SeatsToMotion(float deltaTime)
		{
			double linearMotion = 0.0;
			double angularMotion = 0.0;
			double offRailStrength = 0.1f;

			if (this.Seat.Passenger != null && this.Seat.CanControl)
			{
				EntityControls controls = this.Seat.controls;
				if (controls.TriesToMove)
				{
					if (controls.Forward || controls.Backward)
					{
						float dir2 = (float)(controls.Forward ? 1 : -1);
						if (Math.Abs(GameMath.AngleRadDistance(base.SidedPos.Yaw, this.Seat.Passenger.SidedPos.Yaw)) > 1.5707964f)
						{
							dir2 *= -1f;
						}
						linearMotion += (double)(offRailStrength * dir2 * deltaTime * 2f);
					}
				}
			}
			return new Vec2d(linearMotion, angularMotion);
		}

        protected virtual void updateMinecartAngleAndMotion(float deltaTime)
		{
			deltaTime = Math.Min(0.5f, deltaTime);
			float step = GlobalConstants.PhysicsFrameTime;

			EntityPos pos = base.SidedPos;
			
			// Handle collision with other entities
			bool bumped = false;
			if (this.Api.World.GetNearestEntity(this.Pos.XYZ, 0.5f, 0.5f, (e) => {return e != null && e.EntityId != this.EntityId && e.EntityId != this.Seat.Passenger?.EntityId;}) is Entity collidingEntity)
			{
				Vec3d posCart = pos.XYZ;
				Vec3d posEnt = collidingEntity.SidedPos.XYZ;
				Vec3d pushVec = posCart.SubCopy(posEnt).Normalize();
				double force = 0.5d / (double)MathF.Min((float)0.1, (float)posCart.Sub(posEnt).Length());
				pos.Motion += pushVec.Mul(force);
				bumped = true;
			}

			// Handle seated control
			Vec2d controlledMotion = this.SeatsToMotion(step);
			this.ForwardSpeed += (controlledMotion.X * (double)this.SpeedMultiplier - this.ForwardSpeed) * (double)deltaTime;
			this.AngularVelocity += (controlledMotion.Y * (double)this.SpeedMultiplier - this.AngularVelocity) * (double)deltaTime;
			
			if (this.ForwardSpeed != 0.0 || bumped)
			{
				Vec3d targetmotion = pos.GetViewVector().Mul((float)(-(float)this.ForwardSpeed)).ToVec3d();
				pos.Motion.X = targetmotion.X;
				pos.Motion.Z = targetmotion.Z;
			}
			if (this.AngularVelocity != 0.0 || bumped)
			{
				pos.Yaw += (float)this.AngularVelocity * deltaTime * 30f;
			}
		}

		public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
		{
			if (this.cApi.IsGamePaused)
			{
				return;
			}

			this.updateMinecartAngleAndMotion(deltaTime);

			if (base.Properties.Client.Renderer is EntityShapeRenderer esr)
			{
				esr.xangle = this.xangle;
				esr.yangle = this.yangle;
				esr.zangle = this.zangle;
			}
			
			bool selfSitting = false;
			selfSitting |= this.Seat.Passenger == this.cApi.World.Player.Entity;

			EntityAgent passenger = this.Seat.Passenger;
            EntityShapeRenderer pesr = null;

			if (passenger != null)
			{
				EntityProperties properties = passenger.Properties;
				pesr = (properties?.Client.Renderer) as EntityShapeRenderer;
			}

			if (pesr != null)
			{
				pesr.xangle = this.xangle;
				pesr.yangle = this.yangle;
				pesr.zangle = this.zangle;
			}

			if (selfSitting)
			{
				this.modsysSounds.NowInMotion((float)this.Pos.Motion.Length());
				return;
			}

			this.modsysSounds.NotMounted();
		}

		public void Dispose()
		{
			
		}

		public override bool ApplyGravity { get; } = true;

		public override bool IsInteractable { get; } = true;

		public override float MaterialDensity { get; } = 1000f;

		public EntityMinecartSeat Seat;

		public double RenderOrder => 0.0f;

		public int RenderRange => 999;

		public IMountable[] MountPoints => new IMountable[] { this.Seat }; 

		public virtual float SpeedMultiplier
		{
			get
			{
				return 1f;
			}
		}

		public double ForwardSpeed;

		public double AngularVelocity;

		public float xangle;

		public float yangle;

		public float zangle;

		private ModSystemMinecartSound modsysSounds;

		private ICoreClientAPI cApi;
	}
}