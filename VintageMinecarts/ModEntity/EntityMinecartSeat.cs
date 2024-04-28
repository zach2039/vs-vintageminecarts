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
        public EntityMinecartSeat(EntityMinecart entityMinecart, int seatNumber, Vec3f mountOffset)
        {
            this.EntityMinecartInstance = entityMinecart;
            this.SeatNumber = seatNumber;
            this._mountOffset = mountOffset;
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

        private void tryTeleportPassengerToGround()
        {
            var world = Passenger.World;
            var ba = Passenger.World.BlockAccessor;
            bool found = false;

            for (int dx = -1; !found && dx <= 1; dx++)
            {
                for (int dz = -1; !found && dz <= 1; dz++)
                {
                    var targetPos = Passenger.ServerPos.XYZ.AsBlockPos.ToVec3d().Add(dx + 0.5, 1.1, dz + 0.5);
                    var block = ba.GetMostSolidBlock((int)targetPos.X, (int)(targetPos.Y - 0.15), (int)targetPos.Z);
                    if (block.SideSolid[BlockFacing.UP.Index] && !world.CollisionTester.IsColliding(ba, Passenger.CollisionBox, targetPos, false))
                    {
                        this.Passenger.TeleportTo(targetPos);
                        found = true;
                        break;
                    }
                }
            }

            for (int dx = -2; !found && dx <= 2; dx++)
            {
                for (int dz = -2; !found && dz <= 2; dz++)
                {
                    if (Math.Abs(dx) != 2 && Math.Abs(dz) != 2) continue;

                    var targetPos = Passenger.ServerPos.XYZ.AsBlockPos.ToVec3d().Add(dx + 0.5, 1.1, dz + 0.5);
                    var block = ba.GetMostSolidBlock((int)targetPos.X, (int)(targetPos.Y - 0.15), (int)targetPos.Z);
                    if (block.SideSolid[BlockFacing.UP.Index] && !world.CollisionTester.IsColliding(ba, Passenger.CollisionBox, targetPos, false))
                    {
                        this.Passenger.TeleportTo(targetPos);
                        found = true;
                        break;
                    }
                }
            }

            for (int dx = -1; !found && dx <= 1; dx++)
            {
                for (int dz = -1; !found && dz <= 1; dz++)
                {
                    var targetPos = Passenger.ServerPos.XYZ.AsBlockPos.ToVec3d().Add(dx + 0.5, 1.1, dz + 0.5);
                    if (!world.CollisionTester.IsColliding(ba, Passenger.CollisionBox, targetPos, false))
                    {
                        this.Passenger.TeleportTo(targetPos);
                        found = true;
                        break;
                    }
                }
            }
        }

        public void DidUnmount(EntityAgent entityAgent)
        {
            if (entityAgent.World.Side == EnumAppSide.Server)
            {
                tryTeleportPassengerToGround();
            }

            var pesr = this.Passenger?.Properties?.Client.Renderer as EntityShapeRenderer;
            if (pesr != null)
            {
                pesr.xangle = 0;
                pesr.yangle = 0;
                pesr.zangle = 0;
            }

            this.Passenger.Pos.Roll = 0;
            this.Passenger = null;
        }

        public void MountableToTreeAttributes(TreeAttribute tree)
        {
            tree.SetString("className", "minecart");
            tree.SetLong("entityIdMinecart", this.EntityMinecartInstance.EntityId);
            tree.SetInt("seatNumber", this.SeatNumber);
        }

        public static IMountable GetMountable(IWorldAccessor world, TreeAttribute tree)
        {
            Entity entity = world.GetEntityById(tree.GetLong("entityIdMinecart"));
            if (entity is EntityMinecart entityMinecart)
            {
                return entityMinecart.Seat;
            }

            return null;
        }

        public EntityMinecart EntityMinecartInstance { get; set; }

        public EntityAgent Passenger { get; set; } = null;

        protected Matrixf ModelMatrix { get; set; } = new Matrixf();

        public string SuggestedAnimation { get { return "sitflooridle"; } }

        public int SeatNumber { get; set; }

        public long PassengerEntityIdForInit { get; set; }

        private Vec3f _mountOffset { get; set; }

        public Vec3f MountOffset 
        { 
            get 
            {
                EntityPos position = EntityMinecartInstance.SidedPos;
                this.ModelMatrix.Identity();

                this.ModelMatrix.Rotate(EntityMinecartInstance.Rotation);

                Vec4f mountOffset4f = new Vec4f(_mountOffset.X, _mountOffset.Y, _mountOffset.Z, 0f);
                Vec4f rotationVec = ModelMatrix.TransformVector(mountOffset4f);

                return new Vec3f(rotationVec.X, rotationVec.Y, rotationVec.Z);
            }
        }

        public EntityPos MountPosition 
        {
            get 
            {
                EntityPos position = EntityMinecartInstance.SidedPos;
                Vec3f mOffset = MountOffset;

                EntityPos mountPosition = new EntityPos();
                mountPosition.SetPos(position.X + mOffset.X, position.Y + mOffset.Y, position.Z + mOffset.Z);
                mountPosition.SetAngles(
                    position.Roll + EntityMinecartInstance.Rotation.X,
                    position.Yaw + EntityMinecartInstance.Rotation.Y,
                    position.Pitch + EntityMinecartInstance.Rotation.Z
                );

                return mountPosition;
            }
        }

        public Vec3f EyePos { get; protected set; }

        public bool CanControl => false;

        public Vintagestory.API.Common.Entities.Entity MountedBy => Passenger;

        public IMountableSupplier MountSupplier => EntityMinecartInstance;

        public EnumMountAngleMode AngleMode => EnumMountAngleMode.Push;

        public Vec3f LocalEyePos => EyePos;

        public EntityControls Controls => null;
    }
}