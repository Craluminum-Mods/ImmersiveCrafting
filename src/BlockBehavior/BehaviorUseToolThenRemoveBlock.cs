using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace ImmersiveCrafting
{
  public class BlockBehaviorUseToolThenRemoveBlock : BlockBehavior
  {
    string actionlangcode;
    int toolDurabilityCost;
    JsonItemStack outputStack;
    WorldInteraction[] interactions;

    public BlockBehaviorUseToolThenRemoveBlock(Block block) : base(block)
    {
    }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);

      interactions = ObjectCacheUtil.GetOrCreate(api, "cutInteractions-", () =>
      {
        List<ItemStack> toolStacks = new List<ItemStack>();

        foreach (CollectibleObject obj in api.World.Items)
        {
          if (obj.Tool == EnumTool.Knife || obj.Tool == EnumTool.Sword)
          {
            toolStacks.Add(new ItemStack(obj));
          }
        }

        return new WorldInteraction[]
        {
          new WorldInteraction()
          {
            ActionLangCode = actionlangcode,
            MouseButton = EnumMouseButton.Right,
            Itemstacks = toolStacks.ToArray()
          }
        };
      });
    }

    public override void Initialize(JsonObject properties)
    {
      base.Initialize(properties);

      actionlangcode = properties["actionLangCode"].AsString();
      toolDurabilityCost = properties["toolDurabilityCost"].AsInt();
      outputStack = properties["outputStack"].AsObject<JsonItemStack>();
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
      ItemStack outputstack = null;
      if (outputStack.Resolve(world, "output stacks"))
        outputstack = outputStack.ResolvedItemstack;

      ItemStack itemslot = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack;
      EnumTool? tool = itemslot?.Collectible.Tool;

      if ((tool == EnumTool.Knife || tool == EnumTool.Sword) && itemslot?.Collectible.GetDurability(itemslot) >= toolDurabilityCost)
      {
        itemslot.Collectible.DamageItem(world, byPlayer.Entity, byPlayer.InventoryManager.ActiveHotbarSlot, toolDurabilityCost);

        if (!byPlayer.InventoryManager.TryGiveItemstack(outputstack))
        {
          world.SpawnItemEntity(outputstack, byPlayer.Entity.Pos.XYZ);
        }

        world.BlockAccessor.SetBlock(0, blockSel.Position);

        handling = EnumHandling.PreventDefault;
      }
      return true;
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
    {
      handling = EnumHandling.PassThrough;
      return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling));
    }
  }
}