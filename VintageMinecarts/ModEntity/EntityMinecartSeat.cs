using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageMinecarts.ModEntity
{
    public class EntityMinecartSeat : IMountable
    {
        public EntityMinecartSeat(EntityMinecart entityMinecart, Vec3f mountOffset)
        {
            this.controls.OnAction = new OnEntityAction(this.OnControls);
			this.EntityMinecart = entityMinecart;
			this.mountOffset = mountOffset;
        }

        internal void OnControls(EnumEntityAction enumEntityAction, bool on, ref EnumHandling handled)
		{
			if (enumEntityAction == EnumEntityAction.Sneak && on)
			{
				EntityAgent passenger = this.Passenger;
				if (passenger != null)
				{
					passenger.StopAnimation("holdbothhandslarge");
					passenger.TryUnmount();
				}
				this.controls.StopAllMovement();
			}
		}

        public long PassengerEntityIdForInit;

        public EntityControls controls = new();

        public EntityControls Controls => this.controls;        

        public EntityMinecart EntityMinecart;

        public EntityAgent Passenger;

        public bool CanControl { get; set; }

        public Entity MountedBy => this.Passenger;

        public IMountableSupplier MountSupplier => this.EntityMinecart;

        protected Vec3f mountOffset;

        private Vec4f transformVecTemp = new();

        private Vec3f transformedMountOffset = new();

        private Matrixf modelmat = new();

        public Vec3f MountOffset
		{
			get
			{
				EntityPos pos = this.EntityMinecart.SidedPos;
				this.modelmat.Identity();
				this.modelmat.Rotate(this.EntityMinecart.xangle, this.EntityMinecart.yangle + pos.Yaw, this.EntityMinecart.zangle);
				Vec4f rotvec = this.modelmat.TransformVector(this.transformVecTemp.Set(this.mountOffset.X, this.mountOffset.Y, this.mountOffset.Z, 0f));
				return this.transformedMountOffset.Set(rotvec.X, rotvec.Y, rotvec.Z);
			}
		}

        private EntityPos mountPos = new EntityPos();

        public EntityPos MountPosition 
        {
            get
            {
                EntityPos pos = this.EntityMinecart.SidedPos;
				Vec3f moffset = this.MountOffset;
				this.mountPos.SetPos(pos.X + (double)moffset.X, pos.Y + (double)moffset.Y, pos.Z + (double)moffset.Z);
				this.mountPos.SetAngles(pos.Roll + this.EntityMinecart.xangle, pos.Yaw + this.EntityMinecart.yangle, pos.Pitch + this.EntityMinecart.zangle);
				return this.mountPos;
            }
        }

        public EnumMountAngleMode AngleMode => EnumMountAngleMode.Push;

        public string SuggestedAnimation => "sitflooridle";

        protected Vec3f eyePos = new Vec3f(0f, 1f, 0f);

        public Vec3f LocalEyePos => this.eyePos;

        public static IMountable GetMountable(IWorldAccessor world, TreeAttribute tree)
		{
			EntityMinecart entityMinecart = world.GetEntityById(tree.GetLong("entityIdMinecart", 0L)) as EntityMinecart;

			if (entityMinecart != null)
			{
				return entityMinecart.Seat;
			}

			return null;
		}

        public void DidMount(EntityAgent entityAgent)
        {
            if (this.Passenger != null && this.Passenger != entityAgent)
			{
				this.Passenger.TryUnmount();
				return;
			}
			this.Passenger = entityAgent;
        }

        /// <summary>
        /// Pretty much vanilla's tryTeleportPassengerToShore method
        /// </summary>
        private void tryTeleportPassengerToSafeGround()
		{
			IWorldAccessor world = this.Passenger.World;
			IBlockAccessor ba = this.Passenger.World.BlockAccessor;
			bool found = false;
			int dx = -1;
			while (!found && dx <= 1)
			{
				int dz = -1;
				while (!found && dz <= 1)
				{
					Vec3d targetPos = this.Passenger.ServerPos.XYZ.AsBlockPos.ToVec3d().Add((double)dx + 0.5, 1.1, (double)dz + 0.5);
					if (ba.GetMostSolidBlock(targetPos.AsBlockPos).SideSolid[BlockFacing.UP.Index] && !world.CollisionTester.IsColliding(ba, this.Passenger.CollisionBox, targetPos, false))
					{
						this.Passenger.TeleportTo(targetPos);
						found = true;
						break;
					}
					dz++;
				}
				dx++;
			}
			int dx2 = -2;
			while (!found && dx2 <= 2)
			{
				int dz2 = -2;
				while (!found && dz2 <= 2)
				{
					if (Math.Abs(dx2) == 2 || Math.Abs(dz2) == 2)
					{
						Vec3d targetPos2 = this.Passenger.ServerPos.XYZ.AsBlockPos.ToVec3d().Add((double)dx2 + 0.5, 1.1, (double)dz2 + 0.5);
						if (ba.GetMostSolidBlock(targetPos2.AsBlockPos).SideSolid[BlockFacing.UP.Index] && !world.CollisionTester.IsColliding(ba, this.Passenger.CollisionBox, targetPos2, false))
						{
							this.Passenger.TeleportTo(targetPos2);
							found = true;
							break;
						}
					}
					dz2++;
				}
				dx2++;
			}
			int dx3 = -1;
			while (!found && dx3 <= 1)
			{
				int dz3 = -1;
				while (!found && dz3 <= 1)
				{
					Vec3d targetPos3 = this.Passenger.ServerPos.XYZ.AsBlockPos.ToVec3d().Add((double)dx3 + 0.5, 1.1, (double)dz3 + 0.5);
					if (!world.CollisionTester.IsColliding(ba, this.Passenger.CollisionBox, targetPos3, false))
					{
						this.Passenger.TeleportTo(targetPos3);
						found = true;
						break;
					}
					dz3++;
				}
				dx3++;
			}
		}

        public void DidUnmount(EntityAgent entityAgent)
        {
			if (entityAgent.World.Side == EnumAppSide.Server)
			{
				this.tryTeleportPassengerToSafeGround();
			}

			EntityAgent passenger = this.Passenger;
            EntityShapeRenderer pesr = null;

			if (passenger != null)
			{
				EntityProperties properties = passenger.Properties;
				pesr = (properties?.Client.Renderer) as EntityShapeRenderer;
			}

			if (pesr != null)
			{
				pesr.xangle = 0f;
				pesr.yangle = 0f;
				pesr.zangle = 0f;
			}

			if (passenger != null)
			{
				IAnimationManager animManager = passenger.AnimManager;
				if (animManager != null)
				{
                    entityAgent.StopAnimation("holdbothhandslarge");
				}
			}

			this.Passenger.Pos.Roll = 0f;
			this.Passenger = null;
        }

        public void MountableToTreeAttributes(TreeAttribute tree)
        {
            tree.SetString("className", "EntityMinecartSeat");
			tree.SetLong("entityIdMinecart", this.EntityMinecart.EntityId);
        }
    }
}