using System;
using System.Collections;
using System.Collections.Generic;
using VintageMinecarts.ModUtil;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageMinecarts.ModEntity
{
    public class EntityMinecart : Entity, IRenderer, IMountableSupplier
    {
        public EntityMinecart()
        {
            this.Seat = new EntityMinecartSeat(this, 0, this.MountOffsets[0]);
        }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            if (api is ICoreClientAPI capi)
            {
                capi.Event.RegisterRenderer(this, EnumRenderStage.Before, "minecartsim");
                //modsysSounds = api.ModLoader.GetModSystem<ModSystemBoatingSound>();
            }

            // The mounted entity will try to mount as well, but at that time, the boat might not have been loaded, so we'll try mounting on both ends. 
            if (this.Seat.PassengerEntityIdForInit != 0 && this.Seat.Passenger == null)
            {
                var entity = api.World.GetEntityById(this.Seat.PassengerEntityIdForInit) as EntityAgent;
                if (entity != null)
                {
                    entity.TryMount(this.Seat);
                }
            }
        }

        public virtual float SeatsToMotion(float dt)
        {
            return 0.0f;
        }

        protected virtual bool IsRailBlock(Block block)
        {
            return (block is BlockRails);
        }

        protected virtual EnumRailDirection GetRailDirection(BlockRails blockRails)
        {
            if (blockRails.LastCodePart().Contains("curved_es")) return EnumRailDirection.EAST_TO_SOUTH;
            if (blockRails.LastCodePart().Contains("curved_sw")) return EnumRailDirection.SOUTH_TO_WEST;
            if (blockRails.LastCodePart().Contains("curved_wn")) return EnumRailDirection.WEST_TO_NORTH;
            if (blockRails.LastCodePart().Contains("curved_ne")) return EnumRailDirection.NORTH_TO_EAST;
            if (blockRails.LastCodePart().Contains("flat_ns")) return EnumRailDirection.NORTH_TO_SOUTH;
            if (blockRails.LastCodePart().Contains("flat_we")) return EnumRailDirection.WEST_TO_EAST;
            if (blockRails.LastCodePart().Contains("raised_sn")) return EnumRailDirection.UPWARDS_NORTH;
            if (blockRails.LastCodePart().Contains("raised_ns")) return EnumRailDirection.UPWARDS_SOUTH;
            if (blockRails.LastCodePart().Contains("raised_we")) return EnumRailDirection.UPWARDS_EAST;
            if (blockRails.LastCodePart().Contains("raised_ew")) return EnumRailDirection.UPWARDS_WEST;

            // Uh oh
            throw new ArgumentException("BlockRails variant does not have a valid rail direction.");
        }

        protected virtual void MoveAlongRail(BlockPos railBlockPos, BlockRails blockRails, float deltaTime)
        {            
            //this.PositionBeforeFalling = this.SidedPos.XYZ;

            // Get direction from motion
            BlockFacing minecartHorizontalDirection = BlockFacing.FromVector(
                this.SidedPos.GetViewVector().X,
                0,
                this.SidedPos.GetViewVector().Z
            );

            // Apply effects of gravity on speed if on raised track
            EnumRailDirection currentDirection = GetRailDirection(blockRails);
            switch (currentDirection)
            {
                case EnumRailDirection.UPWARDS_NORTH:
                    this.SidedPos.Motion.Add(0.0, 0.0, 0.008);
                    break;
                case EnumRailDirection.UPWARDS_SOUTH:
                    this.SidedPos.Motion.Add(0.0, 0.0, -0.008);
                    break;
                case EnumRailDirection.UPWARDS_EAST:
                    this.SidedPos.Motion.Add(-0.008, 0.0, 0.0);
                    break;
                case EnumRailDirection.UPWARDS_WEST:
                    this.SidedPos.Motion.Add(0.008, 0.0, 0.0);
                    break;
            }

            // Determine movement from current rail position and movement direction, but only if cart is actively moving
            if (this.SidedPos.Motion.Length() <= 0.00001f)
            {
                return;
            }

            KeyValuePair<Vec3i, Vec3i> exitFromTo;
            if (_exitMap.TryGetValue(currentDirection, out exitFromTo)) 
            {
                Vec3i fromPos = exitFromTo.Key;
                Vec3i toPos = exitFromTo.Value;

                // Yaw depending on from and to locations
                BlockFacing newDirection = BlockFacing.FromNormal(toPos);
                switch (newDirection.Index)
                {
                    case BlockFacing.indexNORTH:
                        this.SidedPos.Yaw = 0f;
                        break;
                    case BlockFacing.indexSOUTH:
                        this.SidedPos.Yaw = GameMath.DEG2RAD * 180f;
                        break;
                    case BlockFacing.indexEAST:
                        this.SidedPos.Yaw = GameMath.DEG2RAD * 270f;
                        break;
                    case BlockFacing.indexWEST:
                        this.SidedPos.Yaw = GameMath.DEG2RAD * 90f;
                        break;
                    default:
                        break;
                }

                // Roll depending on from and to locations
                bool movingUpward = fromPos.Y < toPos.Y;
                bool movingDownward = fromPos.Y > toPos.Y;
                if (movingUpward)
                {
                    this.SidedPos.Roll = GameMath.DEG2RAD * 45f;
                }
                else if (movingDownward)
                {
                    this.SidedPos.Roll = GameMath.DEG2RAD * -45f;
                }
            }
        }

        protected virtual void MoveFreely(float deltaTime)
        {

        }

        protected virtual void UpdateMinecartAngleAndMotion(float baseDeltaTime)
        {
            // Ignore lag spikes
            float deltaTime = Math.Min(0.5f, baseDeltaTime);

            float step = GlobalConstants.PhysicsFrameTime;
            float pilotMotion = SeatsToMotion(step);

            // Try get block below entity
            BlockPos currentBlockPos = this.SidedPos.AsBlockPos;
            Block currentBlock = this.World.BlockAccessor.GetBlock(currentBlockPos);

            if (IsRailBlock(currentBlock))
            {
                // On rails, so movement will be locked to rails
                this.MoveAlongRail(currentBlockPos, (BlockRails)currentBlock, deltaTime);
            }              
            else
            {
                // Off rails, so movement will be unconstrained
                this.MoveFreely(deltaTime);
            }       
        }

        public virtual void OnRenderFrame(float deltaTime, EnumRenderStage renderStage)
        {
            if (VintageMinecartsMod.Instance.CApi.IsGamePaused) return;

            //UpdateMinecartAngleAndMotion(deltaTime);
        }

        public override void OnGameTick(float deltaTime)
        {
            if (World.Side == EnumAppSide.Server)
            {
                UpdateMinecartAngleAndMotion(deltaTime);
            }
            
            base.OnGameTick(deltaTime);
        }

        public void Dispose()
        {

        }

        public Vec3f GetMountOffset(Entity entity)
        {
            if (this.Seat.Passenger == entity)
            {
                return this.Seat.MountOffset;
            }
            return null;
        }

        public bool IsMountedBy(Entity entity)
        {
            if (Seat.Passenger == entity) return true;

            return false;
        }

        public double RenderOrder => 0;

        public int RenderRange => 999;
        
        public EntityMinecartSeat Seat;        

        public Vec3f Rotation { get; set; } = new Vec3f();       

        public Vec3f[] MountOffsets { get; set; } = new Vec3f[] { new Vec3f(0.0f, 0.2f, 0) };

        public IMountable[] MountPoints => new IMountable[] { Seat };

        private Dictionary<EnumRailDirection, KeyValuePair<Vec3i, Vec3i>> _exitMap { get; } = new Dictionary<EnumRailDirection, KeyValuePair<Vec3i, Vec3i>>() {
            { EnumRailDirection.EAST_TO_SOUTH, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.EAST.Normali, BlockFacing.SOUTH.Normali) },
            { EnumRailDirection.SOUTH_TO_WEST, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.SOUTH.Normali, BlockFacing.WEST.Normali) },
            { EnumRailDirection.WEST_TO_NORTH, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.WEST.Normali, BlockFacing.NORTH.Normali) },
            { EnumRailDirection.NORTH_TO_EAST, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.NORTH.Normali, BlockFacing.EAST.Normali) },
            { EnumRailDirection.NORTH_TO_SOUTH, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.NORTH.Normali, BlockFacing.SOUTH.Normali) },
            { EnumRailDirection.WEST_TO_EAST, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.WEST.Normali, BlockFacing.EAST.Normali) },
            { EnumRailDirection.UPWARDS_NORTH, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.NORTH.Normali, BlockFacing.SOUTH.Normali.Add(0, -1, 0)) },
            { EnumRailDirection.UPWARDS_SOUTH, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.NORTH.Normali.Add(0, -1, 0), BlockFacing.SOUTH.Normali) },
            { EnumRailDirection.UPWARDS_WEST, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.WEST.Normali, BlockFacing.EAST.Normali.Add(0, -1, 0)) },
            { EnumRailDirection.UPWARDS_EAST, new KeyValuePair<Vec3i, Vec3i>(BlockFacing.WEST.Normali.Add(0, -1, 0), BlockFacing.EAST.Normali) },
        };

        public override bool ApplyGravity => true;      

        public override bool IsInteractable => true;

        public override float MaterialDensity => 7600;

        public override double SwimmingOffsetY => 0.45;    

        public virtual float SpeedMultiplier => 40f;

        public virtual double ForwardSpeed { get; set; } = 0.0d;
    }
}