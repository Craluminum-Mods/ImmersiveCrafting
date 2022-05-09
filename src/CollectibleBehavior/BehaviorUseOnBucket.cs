using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ImmersiveCrafting
{
  public class CollectibleBehaviorUseOnBucket : CollectibleBehavior
  {
    bool spawnParticles;
    string actionlangcode;
    string sound;
    float takeQuantity;
    int ingredientQuantity;
    JsonItemStack outputStack;
    JsonItemStack liquidStack;
    BlockLiquidContainerBase targetContainer;
    WorldInteraction[] interactions;

    public CollectibleBehaviorUseOnBucket(CollectibleObject collObj) : base(collObj)
    {
    }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);

      api.Event.EnqueueMainThreadTask(() =>
      {
        interactions = ObjectCacheUtil.GetOrCreate(api, "liquidContainerInteractions" + actionlangcode, () =>
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
              ActionLangCode = actionlangcode,
              MouseButton = EnumMouseButton.Right,
              Itemstacks = lstacks.ToArray()
            }
          };
        });
      }, "initLiquidContainerInteractions");
    }

    public override void Initialize(JsonObject properties)
    {
      base.Initialize(properties);

      spawnParticles = properties["spawnParticles"].AsBool();
      actionlangcode = properties["actionLangCode"].AsString();
      sound = properties["sound"].AsString();
      takeQuantity = properties["consumeLiters"].AsFloat();
      ingredientQuantity = properties["ingredientQuantity"].AsInt();
      outputStack = properties["outputStack"].AsObject<JsonItemStack>();
      liquidStack = properties["liquidStack"].AsObject<JsonItemStack>();
    }

    public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      Interact(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
      handling = EnumHandling.PassThrough;
      return interactions.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
    }
  
    public void Interact(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      IWorldAccessor world = byEntity.World;

      IPlayer byPlayer = null;
      if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
      if (byPlayer == null) return;

      targetContainer = byEntity.World.BlockAccessor.GetBlock(blockSel.Position) as BlockLiquidContainerBase;

      if (firstEvent && handHandling != EnumHandHandling.PreventDefault)
      {
        if (blockSel != null && targetContainer != null)
        {
          if (targetContainer.IsTopOpened)
          {
            var liquid = targetContainer.GetContent(blockSel.Position);
            if (liquid != null && liquid.Collectible.Code.Equals(liquidStack.Code))
            {
              var props = BlockLiquidContainerBase.GetContainableProps(liquid);
              if (props != null)
              {
                int takeAmount = (int)Math.Ceiling((takeQuantity) * props.ItemsPerLitre);
                if (takeAmount <= liquid.StackSize)
                {
                  liquid = targetContainer.TryTakeContent(blockSel.Position, takeAmount);
                  if (liquid != null)
                  {
                    ItemStack outputstack = null;
                    
                    if (outputStack.Type == EnumItemClass.Item)
                      outputstack = new ItemStack(world.GetItem(outputStack.Code), outputStack.StackSize);

                    if (outputStack.Type == EnumItemClass.Block)
                      outputstack = new ItemStack(world.GetBlock(outputStack.Code), outputStack.StackSize);

                    if (!byPlayer.InventoryManager.TryGiveItemstack(outputstack))
                    {
                      byEntity.World.SpawnItemEntity(outputstack, byEntity.Pos.XYZ);
                    }
                    if (spawnParticles != false)
                    {
                      byEntity.World.SpawnCubeParticles(byEntity.Pos.XYZ, itemslot.Itemstack.Clone(), 0.1f, 80, 0.3f);
                    }
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/" + sound), byEntity);
                    itemslot.TakeOut(ingredientQuantity);  /// BUG: Ignores ingredientQuantity completely when less items left
                    itemslot.MarkDirty();
                    handHandling = EnumHandHandling.PreventDefault;
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}