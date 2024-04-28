using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageMinecarts.ModBlock
{
    public class BlockMinecartRails : BlockRails
    {
        protected bool PlaceIfSuitable(IWorldAccessor world, IPlayer byPlayer, Block block, BlockPos pos)
		{
			string failureCode = "";
			BlockSelection blockSel = new BlockSelection
			{
				Position = pos,
				Face = BlockFacing.UP
			};

			if (block.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
			{
				block.DoPlaceBlock(world, byPlayer, blockSel, null);
				return true;
			}

			return false;
		}

		protected Block GetRailBlock(IWorldAccessor world, string prefix, BlockFacing dir0, BlockFacing dir1)
		{
			Block block = world.GetBlock(base.CodeWithParts(prefix + dir0.Code[0].ToString() + dir1.Code[0].ToString()));

			if (block != null)
			{
				return block;
			}

			return world.GetBlock(base.CodeWithParts(prefix + dir1.Code[0].ToString() + dir0.Code[0].ToString()));
		}

		protected BlockFacing GetOpenedEndedFace(BlockFacing[] dirFacings, IWorldAccessor world, BlockPos blockPos)
		{
			if (!(world.BlockAccessor.GetBlock(blockPos.AddCopy(dirFacings[0])) is BlockRails))
			{
				return dirFacings[0];
			}

			if (!(world.BlockAccessor.GetBlock(blockPos.AddCopy(dirFacings[1])) is BlockRails))
			{
				return dirFacings[1];
			}

			return null;
		}

		protected BlockFacing[] GetFacingsFromType(string type)
		{
			string codes = type.Split(new char[] { '_' })[1];

			return new BlockFacing[]
			{
				BlockFacing.FromFirstLetter(codes[0]),
				BlockFacing.FromFirstLetter(codes[1])
			};
		}

        protected bool TryAttachPlaceToHorizontal(IWorldAccessor world, IPlayer byPlayer, BlockPos position, BlockFacing toFacing, BlockFacing targetFacing)
		{
			BlockPos facingOffsetPos = position.AddCopy(toFacing);
			Block facingOffsetBlock = world.BlockAccessor.GetBlock(facingOffsetPos);

			if (!(facingOffsetBlock is BlockRails))
			{
                // Stop placement if rail in front of target place location is rails
				return false;
			}

			BlockFacing fromFacing = toFacing.Opposite;
			BlockFacing[] forwardDirFacings = this.GetFacingsFromType(facingOffsetBlock.Variant["type"]);
			if (world.BlockAccessor.GetBlock(facingOffsetPos.AddCopy(forwardDirFacings[0])) is BlockRails && world.BlockAccessor.GetBlock(facingOffsetPos.AddCopy(forwardDirFacings[1])) is BlockRails)
			{
                // Stop placement if space in front of target place location is surronded by similar facing rails
				return false;
			}

			BlockFacing neibFreeFace = this.GetOpenedEndedFace(forwardDirFacings, world, position.AddCopy(toFacing));
			if (neibFreeFace == null)
			{
                // Stop placement if space in front of target place location is not adjacent to similar facing rails
				return false;
			}

			Block blockToPlace = this.GetRailBlock(world, "curved_", toFacing, targetFacing);
			if (blockToPlace != null)
			{
				return this.PlaceIfSuitable(world, byPlayer, blockToPlace, position);
			}

			string dirs = facingOffsetBlock.Variant["type"].Split(new char[] { '_' })[1];

			BlockFacing neibKeepFace = (dirs[0] == neibFreeFace.Code[0]) ? BlockFacing.FromFirstLetter(dirs[1]) : BlockFacing.FromFirstLetter(dirs[0]);
			Block block = this.GetRailBlock(world, "curved_", neibKeepFace, fromFacing);
			if (block == null)
			{
				return false;
			}

			block.DoPlaceBlock(world, byPlayer, new BlockSelection
			{
				Position = position.AddCopy(toFacing),
				Face = BlockFacing.UP
			}, null);

			return false;
		}

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
		{
			if (!this.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
			{
				return false;
			}

			BlockFacing targetFacing = Block.SuggestedHVOrientation(byPlayer, blockSel)[0];
			Block blockToPlace = null;
			for (int i = 0; i < BlockFacing.HORIZONTALS.Length; i++)
			{
				BlockFacing facing = BlockFacing.HORIZONTALS[i];
				if (this.TryAttachPlaceToHorizontal(world, byPlayer, blockSel.Position, facing, targetFacing))
				{
					return true;
				}
			}

			if (blockToPlace == null)
			{
                BlockPos facingOffsetAbovePos = blockSel.Position.AddCopy(targetFacing).Offset(BlockFacing.UP);
                Block facingOffsetAboveBlock = world.BlockAccessor.GetBlock(facingOffsetAbovePos);

				if (targetFacing.Axis == EnumAxis.Z)
				{
                    // Place ramp piece if rail is found above
                    if (facingOffsetAboveBlock is BlockRails)
                    {
						if (targetFacing == BlockFacing.NORTH)
						{
							blockToPlace = world.GetBlock(base.CodeWithParts("raised_sn"));
						}
						else
						{
							blockToPlace = world.GetBlock(base.CodeWithParts("raised_ns"));
						}
                    }
                    else
                    {
                        blockToPlace = world.GetBlock(base.CodeWithParts("flat_ns"));
                    }
				}
				else
				{
                    // Place ramp piece if rail is found above
                    if (facingOffsetAboveBlock is BlockRails)
                    {
						if (targetFacing == BlockFacing.EAST)
						{
							blockToPlace = world.GetBlock(base.CodeWithParts("raised_we"));
						}
						else
						{
							blockToPlace = world.GetBlock(base.CodeWithParts("raised_ew"));
						}
                    }
                    else
                    {
                        blockToPlace = world.GetBlock(base.CodeWithParts("flat_we"));
                    }
				}
			}
			blockToPlace.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
			return true;
		}

		public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
		{
			return new ItemStack(world.GetBlock(base.CodeWithParts("flat_ns")));
		}
    }
}