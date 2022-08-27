using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ImmersiveCrafting
{
  public class CollectibleBehaviorWaxCheese : CollectibleBehavior
  {
    WorldInteraction[] interactions;

    public CollectibleBehaviorWaxCheese(CollectibleObject collObj) : base(collObj) { }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);

      ItemStack[] cheeseStack = new ItemStack[] { new ItemStack(api.World.GetItem(new AssetLocation("rawcheese-salted"))) };

      interactions = new WorldInteraction[] {
          new WorldInteraction() {
              ActionLangCode = "Wax the cheese",
              MouseButton = EnumMouseButton.Right,
              Itemstacks = cheeseStack,
              HotKeyCode = "shift"
          }
      };
    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      Interact(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
      handling = EnumHandling.PassThrough;
      return interactions.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
    }

    public void Interact(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      if (blockSel == null) return;

      var world = byEntity.World;

      var blockPos = blockSel.Position;
      var blockEntity = world.BlockAccessor.GetBlockEntity(blockPos);

      if (blockEntity is BECheese bec && bec.Inventory[0].Itemstack?.Collectible.Variant["type"] == "salted")
      {
        slot.TakeOut(1);
        slot.MarkDirty();
        bec.Inventory[0].Itemstack = new ItemStack(world.GetItem(bec.Inventory[0].Itemstack?.Collectible.CodeWithVariant("type", "waxed")));
        bec.Inventory[0].MarkDirty();
        bec.MarkDirty(true);
        handHandling = EnumHandHandling.PreventDefault;
      }
    }
  }
}