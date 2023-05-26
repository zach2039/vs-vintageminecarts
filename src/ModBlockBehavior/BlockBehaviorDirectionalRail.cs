using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VintageMinecarts.ModBlockBehavior
{
    //public class BlockBehaviorDirectionalRail : BlockBehavior
    //{
    //    public BlockBehaviorDirectionalRail(Block block) : base(block)
    //    {
    //    }
//
    //    public override void Initialize(JsonObject properties)
	//	{
	//		base.Initialize(properties);
	//		if (properties["dropBlockFace"].Exists)
	//		{
	//			this.dropBlockFace = properties["dropBlockFace"].AsString(null);
	//		}
	//		if (properties["drop"].Exists)
	//		{
	//			this.drop = properties["drop"].AsObject<JsonItemStack>(null, this.block.Code.Domain);
	//		}
	//	}
//
    //    public override void OnLoaded(ICoreAPI api)
	//	{
	//		base.OnLoaded(api);
	//		JsonItemStack jsonItemStack = this.drop;
	//		if (jsonItemStack == null)
	//		{
	//			return;
	//		}
	//		IWorldAccessor world = api.World;
	//		string str = "Directional rail drop for ";
	//		AssetLocation code = this.block.Code;
	//		jsonItemStack.Resolve(world, str + ((code != null) ? code.ToString() : null), true);
	//	}
//
    //    protected BlockFacing[] GetFacingsFromType(string type)
	//	{
	//		string codes = type.Split(new char[] { '_' })[1];
//
	//		return new BlockFacing[]
	//		{
	//			BlockFacing.FromFirstLetter(codes[0]),
	//			BlockFacing.FromFirstLetter(codes[1])
	//		};
	//	}
//
    //    protected BlockFacing GetOpenedEndedFace(BlockFacing[] dirFacings, IWorldAccessor world, BlockPos blockPos)
	//	{
	//		if (!(world.BlockAccessor.GetBlock(blockPos.AddCopy(dirFacings[0])) is BlockRails))
	//		{
	//			return dirFacings[0];
	//		}
//
	//		if (!(world.BlockAccessor.GetBlock(blockPos.AddCopy(dirFacings[1])) is BlockRails))
	//		{
	//			return dirFacings[1];
	//		}
//
	//		return null;
	//	}
//
    //    protected Block GetRailBlock(IWorldAccessor world, string prefix, BlockFacing dir0, BlockFacing dir1)
	//	{
	//		Block block = world.GetBlock(base.CodeWithParts(prefix + dir0.Code[0].ToString() + dir1.Code[0].ToString()));
//
	//		if (block != null)
	//		{
	//			return block;
	//		}
//
	//		return world.GetBlock(base.CodeWithParts(prefix + dir1.Code[0].ToString() + dir0.Code[0].ToString()));
	//	}
//
    //    protected bool TryAttachPlaceToHorizontal(IWorldAccessor world, IPlayer byPlayer, BlockPos position, BlockFacing toFacing, BlockFacing targetFacing)
	//	{
	//		BlockPos facingOffsetPos = position.AddCopy(toFacing);
	//		Block facingOffsetBlock = world.BlockAccessor.GetBlock(facingOffsetPos);
//
	//		if (!(facingOffsetBlock is BlockRails))
	//		{
    //            // Stop placement if rail in front of target place location is rails
	//			return false;
	//		}
//
	//		BlockFacing fromFacing = toFacing.Opposite;
	//		BlockFacing[] forwardDirFacings = this.GetFacingsFromType(facingOffsetBlock.Variant[this.variantCode]);
	//		if (world.BlockAccessor.GetBlock(facingOffsetPos.AddCopy(forwardDirFacings[0])) is BlockRails && world.BlockAccessor.GetBlock(facingOffsetPos.AddCopy(forwardDirFacings[1])) is BlockRails)
	//		{
    //            // Stop placement if space in front of target place location is surronded by similar facing rails
	//			return false;
	//		}
//
	//		BlockFacing neibFreeFace = this.GetOpenedEndedFace(forwardDirFacings, world, position.AddCopy(toFacing));
	//		if (neibFreeFace == null)
	//		{
    //            // Stop placement if space in front of target place location is not adjacent to similar facing rails
	//			return false;
	//		}
//
	//		Block blockToPlace = this.GetRailBlock(world, "curved_", toFacing, targetFacing);
	//		if (blockToPlace != null)
	//		{
	//			return this.PlaceIfSuitable(world, byPlayer, blockToPlace, position);
	//		}
//
	//		string dirs = facingOffsetBlock.Variant["type"].Split(new char[] { '_' })[1];
//
	//		BlockFacing neibKeepFace = (dirs[0] == neibFreeFace.Code[0]) ? BlockFacing.FromFirstLetter(dirs[1]) : BlockFacing.FromFirstLetter(dirs[0]);
	//		Block block = this.GetRailBlock(world, "curved_", neibKeepFace, fromFacing);
	//		if (block == null)
	//		{
	//			return false;
	//		}
//
	//		block.DoPlaceBlock(world, byPlayer, new BlockSelection
	//		{
	//			Position = position.AddCopy(toFacing),
	//			Face = BlockFacing.UP
	//		}, null);
//
	//		return false;
	//	}
//
    //    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
	//	{
    //        handling = EnumHandling.PreventDefault;
    //        BlockFacing targetHorizontalFacing = Block.SuggestedHVOrientation(byPlayer, blockSel)[0];
//
    //        string dirCode = "ns";
    //        switch (targetHorizontalFacing.Index)
    //        {
    //            case BlockFacing.indexNORTH:
    //                dirCode = "sn";
    //                break;
    //            case BlockFacing.indexSOUTH:
    //                dirCode = "ns";
    //                break;
    //            case BlockFacing.indexEAST:
    //                dirCode = "we";
    //                break;
    //            case BlockFacing.indexWEST:
    //                dirCode = "ew";
    //                break;
    //        }
//
    //        BlockPos facingOffsetAbovePos = blockSel.Position.AddCopy(targetHorizontalFacing).Offset(BlockFacing.UP);
    //        Block facingOffsetAboveBlock = world.BlockAccessor.GetBlock(facingOffsetAbovePos);
    //        string slopeCode = "flat";
    //        if (facingOffsetAboveBlock.HasBehavior(typeof(BlockBehaviorDirectionalRail)))
    //        {
    //            slopeCode = "raised";
    //        }
//
    //        AssetLocation blockCode = this.block.CodeWithVariant(this.variantCode, slopeCode + "_" + dirCode );
	//		Block orientedBlock = world.BlockAccessor.GetBlock(blockCode);
    //        if (orientedBlock == null)
	//		{
	//			string str = "Unable to to find a rotated block with code ";
	//			AssetLocation assetLocation = blockCode;
	//			throw new NullReferenceException(str + ((assetLocation != null) ? assetLocation.ToString() : null) + ", you're maybe missing the side variant group of have a dash in your block code");
	//		}
    //        
	//		if (!orientedBlock.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
	//		{
	//			return false;
	//		}
//
	//		Block blockToPlace = null;
	//		for (int i = 0; i < BlockFacing.HORIZONTALS.Length; i++)
	//		{
	//			BlockFacing facing = BlockFacing.HORIZONTALS[i];
	//			if (this.TryAttachPlaceToHorizontal(world, byPlayer, blockSel.Position, facing, targetHorizontalFacing))
	//			{
	//				return true;
	//			}
	//		}
//
	//		if (blockToPlace == null)
	//		{
    //            
//
	//			if (targetFacing.Axis == EnumAxis.Z)
	//			{
    //                // Place ramp piece if rail is found above
    //                if (facingOffsetAboveBlock is BlockRails)
    //                {
	//					if (targetFacing == BlockFacing.NORTH)
	//					{
	//						blockToPlace = world.GetBlock(base.CodeWithParts("raised_sn"));
	//					}
	//					else
	//					{
	//						blockToPlace = world.GetBlock(base.CodeWithParts("raised_ns"));
	//					}
    //                }
    //                else
    //                {
    //                    blockToPlace = world.GetBlock(base.CodeWithParts("flat_ns"));
    //                }
	//			}
	//			else
	//			{
    //                // Place ramp piece if rail is found above
    //                if (facingOffsetAboveBlock is BlockRails)
    //                {
	//					if (targetFacing == BlockFacing.EAST)
	//					{
	//						blockToPlace = world.GetBlock(base.CodeWithParts("raised_we"));
	//					}
	//					else
	//					{
	//						blockToPlace = world.GetBlock(base.CodeWithParts("raised_ew"));
	//					}
    //                }
    //                else
    //                {
    //                    blockToPlace = world.GetBlock(base.CodeWithParts("flat_we"));
    //                }
	//			}
	//		}
	//		blockToPlace.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
	//		return true;
	//	}
//
    //    public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref EnumHandling handling, ref string failureCode)
	//	{
	//		handling = EnumHandling.PreventDefault;
	//		BlockFacing[] horVer = Block.SuggestedHVOrientation(byPlayer, blockSel);
	//		AssetLocation blockCode = this.block.CodeWithVariant(this.variantCode, horVer[0].Code);
	//		Block orientedBlock = world.BlockAccessor.GetBlock(blockCode);
	//		if (orientedBlock == null)
	//		{
	//			string str = "Unable to to find a rotated block with code ";
	//			AssetLocation assetLocation = blockCode;
	//			throw new NullReferenceException(str + ((assetLocation != null) ? assetLocation.ToString() : null) + ", you're maybe missing the side variant group of have a dash in your block code");
	//		}
	//		if (orientedBlock.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode))
	//		{
	//			orientedBlock.DoPlaceBlock(world, byPlayer, blockSel, itemstack);
	//			return true;
	//		}
	//		return false;
	//	}
//
    //    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropQuantityMultiplier, ref EnumHandling handled)
	//	{
	//		handled = EnumHandling.PreventDefault;
	//		return new ItemStack[]
	//		{
	//			new ItemStack(world.BlockAccessor.GetBlock(this.block.CodeWithVariant(this.variantCode, "flat_ns")), 1)
	//		};
	//	}
//
	//	private string variantCode = "railtype";
//
	//	private JsonItemStack drop;
    //}
}