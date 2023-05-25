using System;
using VintageMinecarts.ModUtil;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageMinecarts.ModEntities
{
    public class EntityMinecart : Entity, IRenderer, IMountableSupplier
    {
        public EntityMinecart()
        {
            this.MinecartSeat = new EntityMinecartSeat(this, 0, this.MountOffsets[0]);
        }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            base.Initialize(properties, api, InChunkIndex3d);

            if (VintageMinecartsMod.Instance.CApi != null)
            {
                VintageMinecartsMod.Instance.CApi.Event.RegisterRenderer(this, EnumRenderStage.Before, "minecartsim");
                //modsysSounds = api.ModLoader.GetModSystem<ModSystemBoatingSound>();
            }

            // The mounted entity will try to mount as well, but at that time, the boat might not have been loaded, so we'll try mounting on both ends. 
            if (this.MinecartSeat.PassengerEntityIdForInit != 0 && this.MinecartSeat.Passenger == null)
            {
                var entity = api.World.GetEntityById(this.MinecartSeat.PassengerEntityIdForInit) as EntityAgent;
                if (entity != null)
                {
                    entity.TryMount(this.MinecartSeat);
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

        protected virtual EnumRailDirection GetRailDirection(BlockRails blockRails, BlockFacing minecartMotionDirection)
        {
            if (blockRails.LastCodePart().Contains("curved_es")) return EnumRailDirection.EAST_TO_SOUTH;
            if (blockRails.LastCodePart().Contains("curved_sw")) return EnumRailDirection.SOUTH_TO_WEST;
            if (blockRails.LastCodePart().Contains("curved_wn")) return EnumRailDirection.WEST_TO_NORTH;
            if (blockRails.LastCodePart().Contains("curved_ne")) return EnumRailDirection.NORTH_TO_EAST;
            if (blockRails.LastCodePart().Contains("flat_ns")) return EnumRailDirection.NORTH_TO_SOUTH;
            if (blockRails.LastCodePart().Contains("flat_we")) return EnumRailDirection.WEST_TO_EAST;
            if (blockRails.LastCodePart().Contains("raised_ns")) return (minecartMotionDirection == BlockFacing.NORTH) ? EnumRailDirection.UPWARDS_NORTH : EnumRailDirection.UPWARDS_SOUTH;
            if (blockRails.LastCodePart().Contains("raised_we")) return (minecartMotionDirection == BlockFacing.WEST) ? EnumRailDirection.UPWARDS_WEST : EnumRailDirection.UPWARDS_EAST;

            // Uh oh
            throw new ArgumentException("BlockRails variant does not have a valid rail direction.");
        }

        protected virtual void MoveAlongRail(BlockPos railBlockPos, BlockRails blockRails, float deltaTime)
        {            
            this.PositionBeforeFalling = this.SidedPos.XYZ;

            // Get direction from motion
            BlockFacing minecartHorizontalDirection = BlockFacing.FromVector(
                this.SidedPos.GetViewVector().X,
                0,
                this.SidedPos.GetViewVector().Z
            );

            // Apply effects of gravity on speed if on raised track
            switch (GetRailDirection(blockRails, minecartHorizontalDirection))
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

            // Determin
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
            Block currentBlock = Api.World.BlockAccessor.GetBlock(currentBlockPos);

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

            UpdateMinecartAngleAndMotion(deltaTime);
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
            throw new System.NotImplementedException();
        }

        public bool IsMountedBy(Entity entity)
        {
            throw new System.NotImplementedException();
        }

        public double RenderOrder => 0;

        public int RenderRange => 999;
        
        public EntityMinecartSeat MinecartSeat;        

        public Vec3f Rotation { get; set; } = new Vec3f();       

        public Vec3f[] MountOffsets { get; set; } = new Vec3f[] { new Vec3f(0.0f, 0.2f, 0) };

        public IMountable[] MountPoints => throw new System.NotImplementedException();

        public override bool ApplyGravity => true;      

        public override bool IsInteractable => true;

        public override float MaterialDensity => 7600;

        public override double SwimmingOffsetY => 0.45;    

        public virtual float SpeedMultiplier => 40f;

        public virtual double ForwardSpeed { get; set; } = 0.0d;
    }
}