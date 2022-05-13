using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ImmersiveCrafting
{
  public class CollectibleBehaviorSealCrock : CollectibleBehavior
  {
    WorldInteraction[] interactions;

    public CollectibleBehaviorSealCrock(CollectibleObject collObj) : base(collObj)
    {
    }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);

      api.Event.EnqueueMainThreadTask(() =>
      {
        interactions = ObjectCacheUtil.GetOrCreate(api, "crockInteractions", () =>
        {
          List<ItemStack> lstacks = new List<ItemStack>();

          foreach (CollectibleObject obj in api.World.Collectibles)
          {
            if (obj is BlockLiquidContainerBase blc && blc.IsTopOpened && blc.AllowHeldLiquidTransfer)
            {
              lstacks.Add(new ItemStack(obj));
            }
          }

          return new WorldInteraction[]
          {
            new WorldInteraction()
            {
              ActionLangCode = "immersivecrafting:heldhelp-sealcrock",
              MouseButton = EnumMouseButton.Right,
              Itemstacks = lstacks.ToArray()
            }
          };
        });
      }, "initCrockInteractions");
    }

    // public override void Initialize(JsonObject properties)
    // {
    //   base.Initialize(properties);
    // }

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
      var block = byEntity.World.BlockAccessor.GetBlock(blockPos);
      var blockEntity = world.BlockAccessor.GetBlockEntity(blockPos);

      IPlayer byPlayer = null;
      if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
      if (byPlayer == null) return;

      if (firstEvent && handHandling != EnumHandHandling.PreventDefault)
      {
        if (block is BlockGroundStorage)
        {
          var begs = blockEntity as BlockEntityGroundStorage;
          ItemSlot gsslot = begs.GetSlotAt(blockSel);
          if (gsslot == null || gsslot.Empty) return;
          ItemStack crock = gsslot.Itemstack;
          
          if (crock?.Collectible is BlockCrock && crock.Attributes.GetBool("sealed") == false)
          {
            
            crock.Attributes.SetBool("sealed", true);
            slot.TakeOut(1);
            slot.MarkDirty();
            gsslot.MarkDirty();
            begs.updateMeshes();
            begs.MarkDirty(true);
            handHandling = EnumHandHandling.PreventDefault;
          }
        }
      }
    }
  }
}