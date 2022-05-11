using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ImmersiveCrafting
{
  public class CollectibleBehaviorUseOnLiquidContainer : CollectibleBehavior
  {
    bool spawnParticles;
    string actionlangcode;
    string sound;
    float takeQuantity;
    int ingredientQuantity;
    JsonItemStack outputStack;
    JsonItemStack liquidStack;
    WorldInteraction[] interactions;

    public CollectibleBehaviorUseOnLiquidContainer(CollectibleObject collObj) : base(collObj)
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
            if (obj is BlockLiquidContainerBase blc && blc.IsTopOpened && blc.AllowHeldLiquidTransfer || obj is BlockBarrel)
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
      if (blockSel == null) return;

      var world = byEntity.World;

      var blockPos = blockSel.Position;
      var block = byEntity.World.BlockAccessor.GetBlock(blockPos);
      var blockEntity = world.BlockAccessor.GetBlockEntity(blockPos);

      IPlayer byPlayer = null;
      if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
      if (byPlayer == null) return;

      ItemStack outputstack = GetType(world);

      var blockCnt = block as BlockLiquidContainerTopOpened;

      if (firstEvent && handHandling != EnumHandHandling.PreventDefault)
      {
        if (blockCnt != null)
        {
          var liquid = blockCnt.GetContent(blockPos);
          if (IsLiquidStack(liquid))
          {
            var props = GetProps(liquid);
            if (props != null)
            {
              int takeAmount = GetLiquidAsInt(props);
              if (takeAmount <= liquid.StackSize)
              {
                liquid = blockCnt.TryTakeContent(blockPos, takeAmount);
                if (liquid != null)
                {
                  CanSpawnItemStack(byEntity, world, byPlayer, outputstack);
                  CanSpawnParticles(itemslot, byEntity, world, spawnParticles);
                  GetSound(byEntity, world);
                  itemslot.TakeOut(ingredientQuantity);  /// BUG: Ignores ingredientQuantity completely when less items left
                  itemslot.MarkDirty();
                  handHandling = EnumHandHandling.PreventDefault;
                }
              }
            }
          }
        }
        else if (block is BlockBarrel)
        {
          BlockEntityBarrel bebarrel = blockEntity as BlockEntityBarrel;
          if (bebarrel != null)
          {
            var liquid = bebarrel.Inventory[1].Itemstack;
            if (IsLiquidStack(liquid))
            {
              var props = GetProps(liquid);
              if (props != null)
              {
                int takeAmount = GetLiquidAsInt(props);
                if (takeAmount <= liquid.StackSize)
                {
                  liquid = bebarrel.Inventory[1].TakeOut(takeAmount);
                  if (liquid != null)
                  {
                    CanSpawnItemStack(byEntity, world, byPlayer, outputstack);
                    CanSpawnParticles(itemslot, byEntity, world, spawnParticles);
                    GetSound(byEntity, world);
                    itemslot.TakeOut(ingredientQuantity);  /// BUG: Ignores ingredientQuantity completely when less items left
                    bebarrel.MarkDirty(true);
                    itemslot.MarkDirty();
                    handHandling = EnumHandHandling.PreventDefault;
                  }
                }
              }
            }
          }
        }
        else if (block is BlockGroundStorage)
        {
          // if (!byEntity.Controls.Sneak) return;
          var begs = blockEntity as BlockEntityGroundStorage;
          ItemSlot gsslot = begs.GetSlotAt(blockSel);
          if (gsslot == null || gsslot.Empty) return;

          var liquid = blockCnt.GetContent(gsslot.Itemstack); /// Cause crash on this line
          if (IsLiquidStack(liquid))
          {
            var props = GetProps(liquid);
            if (props != null)
            {
              int takeAmount = GetLiquidAsInt(props);
              if (takeAmount <= liquid.StackSize)
              {
                liquid = blockCnt.TryTakeContent(gsslot.Itemstack, takeAmount);  /// Probably will cause crash on this line too
                if (liquid != null)
                {
                  CanSpawnItemStack(byEntity, world, byPlayer, outputstack);
                  CanSpawnParticles(itemslot, byEntity, world, spawnParticles);
                  GetSound(byEntity, world);
                  itemslot.TakeOut(ingredientQuantity);  /// BUG: Ignores ingredientQuantity completely when less items left
                  itemslot.MarkDirty();
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
    }

    private static WaterTightContainableProps GetProps(ItemStack liquid) => BlockLiquidContainerBase.GetContainableProps(liquid);
    private void GetSound(EntityAgent byEntity, IWorldAccessor world) => world.PlaySoundAt(new AssetLocation("sounds/" + sound), byEntity);
    private bool IsLiquidStack(ItemStack liquid) => liquid != null && liquid.Collectible.Code.Equals(liquidStack.Code);
    private int GetLiquidAsInt(WaterTightContainableProps props) => (int)Math.Ceiling((takeQuantity) * props.ItemsPerLitre);

    private ItemStack GetType(IWorldAccessor world)
    {
      ItemStack outputstack = null;

      if (outputStack.Type == EnumItemClass.Item)
        outputstack = new ItemStack(world.GetItem(outputStack.Code), outputStack.StackSize);

      if (outputStack.Type == EnumItemClass.Block)
        outputstack = new ItemStack(world.GetBlock(outputStack.Code), outputStack.StackSize);
      return outputstack;
    }

    private static void CanSpawnItemStack(EntityAgent byEntity, IWorldAccessor world, IPlayer byPlayer, ItemStack outputstack)
    {
      if (!byPlayer.InventoryManager.TryGiveItemstack(outputstack))
      {
        world.SpawnItemEntity(outputstack, byEntity.Pos.XYZ);
      }
    }

    private static void CanSpawnParticles(ItemSlot itemslot, EntityAgent byEntity, IWorldAccessor world, bool spawnParticles)
    {
      if (spawnParticles)
      {
        world.SpawnCubeParticles(byEntity.Pos.XYZ, itemslot.Itemstack.Clone(), 0.1f, 80, 0.3f);
      }
    }
  }
}