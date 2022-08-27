using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ImmersiveCrafting
{
  public class CollectibleBehaviorSealCrock : CollectibleBehavior
  {
    WorldInteraction[] interactions;

    public CollectibleBehaviorSealCrock(CollectibleObject collObj) : base(collObj) { }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);

      api.Event.EnqueueMainThreadTask(() =>
      {
        interactions = ObjectCacheUtil.GetOrCreate(api, "crockInteractions", () =>
        {
          List<ItemStack> crockStacks = new();

          foreach (CollectibleObject obj in api.World.Collectibles)
          {
            if (obj is BlockCrock blc)
            {
              crockStacks.Add(new ItemStack(obj));
            }
          }

          return new WorldInteraction[]
          {
            new WorldInteraction()
            {
              ActionLangCode = "Seal the crock",
              MouseButton = EnumMouseButton.Right,
              Itemstacks = crockStacks.ToArray()
            }
          };
        });
      }, "initCrockInteractions");
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

      var blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position);

      if (blockEntity is BlockEntityGroundStorage begs)
      {
        var gsslot = begs.GetSlotAt(blockSel);
        if (gsslot?.Empty != false) return;
        var crock = gsslot.Itemstack;

        if (crock?.Collectible is BlockCrock && !crock.Attributes.GetBool("sealed"))
        {
          crock.Attributes.SetBool("sealed", true);
          slot.TakeOut(1);
          slot.MarkDirty();
          gsslot.MarkDirty();
          begs.updateMeshes();
          begs.MarkDirty(true);
          handHandling = EnumHandHandling.PreventDefault; /// Quadrants layout in GroundStorable behavior completely overwrites this handling, no idea how to fix
        }
      }
    }
  }
}