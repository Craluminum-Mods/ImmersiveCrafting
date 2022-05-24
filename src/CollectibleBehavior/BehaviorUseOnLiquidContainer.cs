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
          List<ItemStack> lbstacks = new List<ItemStack>();

          foreach (CollectibleObject obj in api.World.Collectibles)
          {
            if (obj is BlockLiquidContainerBase blc && blc.IsTopOpened && blc.AllowHeldLiquidTransfer)
            {
              lstacks.Add(new ItemStack(obj));
            }
            if (obj is BlockBarrel)
            {
              lbstacks.Add(new ItemStack(obj));
            }
          }

          return new WorldInteraction[]
          {
            new WorldInteraction()
            {
              ActionLangCode = actionlangcode,
              MouseButton = EnumMouseButton.Right,
              Itemstacks = lstacks.ToArray()
            },
            new WorldInteraction()
            {
              ActionLangCode = actionlangcode,
              MouseButton = EnumMouseButton.Right,
              HotKeyCode = "sneak",
              Itemstacks = lbstacks.ToArray()
            },
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

      ItemStack outputstack = null;
      if (outputStack.Resolve(world, "output stacks"))
      {
        outputstack = outputStack.ResolvedItemstack;
      }

      var blockCnt = block as BlockLiquidContainerBase;

      if (firstEvent && handHandling != EnumHandHandling.PreventDefault)
      {
        if (blockCnt != null && blockCnt.IsTopOpened)
        {
          var liquid = blockCnt.GetContent(blockPos);
          if (IsLiquidStack(liquid))
          {
            var props = GetProps(liquid);
            if (props != null)
            {
              int takeAmount = GetLiquidAsInt(props);
              if (SatisfiesQuantity(slot, liquid, takeAmount))
              {
                liquid = blockCnt.TryTakeContent(blockPos, takeAmount);
                if (liquid != null)
                {
                  CanSpawnItemStack(byEntity, world, byPlayer, outputstack);
                  CanSpawnParticles(slot, byEntity, world, spawnParticles);
                  GetSound(byEntity, world);
                  TryConsumeIngredient(slot);
                  slot.MarkDirty();
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
                if (SatisfiesQuantity(slot, liquid, takeAmount))
                {
                  liquid = bebarrel.Inventory[1].TakeOut(takeAmount);
                  if (liquid != null)
                  {
                    CanSpawnItemStack(byEntity, world, byPlayer, outputstack);
                    CanSpawnParticles(slot, byEntity, world, spawnParticles);
                    GetSound(byEntity, world);
                    TryConsumeIngredient(slot);
                    bebarrel.MarkDirty(true);
                    slot.MarkDirty();
                    handHandling = EnumHandHandling.PreventDefault;
                  }
                }
              }
            }
          }
        }
        else if (block is BlockGroundStorage)
        {
          var begs = blockEntity as BlockEntityGroundStorage;
          ItemSlot gsslot = begs.GetSlotAt(blockSel);
          if (gsslot == null || gsslot.Empty) return;

          if (gsslot.Itemstack.Collectible is BlockLiquidContainerBase)
          {
            blockCnt = gsslot.Itemstack.Block as BlockLiquidContainerBase;
            var liquid = blockCnt.GetContent(gsslot.Itemstack);
            if (IsLiquidStack(liquid))
            {
              var props = GetProps(liquid);
              if (props != null)
              {
                int takeAmount = GetLiquidAsInt(props);
                if (SatisfiesQuantity(slot, liquid, takeAmount))
                {
                  liquid = blockCnt.TryTakeContent(gsslot.Itemstack, takeAmount);
                  if (liquid != null)
                  {
                    CanSpawnItemStack(byEntity, world, byPlayer, outputstack);
                    CanSpawnParticles(slot, byEntity, world, spawnParticles);
                    GetSound(byEntity, world);
                    TryConsumeIngredient(slot);
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
      }
    }

    private bool SatisfiesQuantity(ItemSlot slot, ItemStack liquid, int takeAmount)
    {
      return takeAmount <= liquid.StackSize && slot.StackSize >= ingredientQuantity;
    }

    private static WaterTightContainableProps GetProps(ItemStack liquid) => BlockLiquidContainerBase.GetContainableProps(liquid);
    private void GetSound(EntityAgent byEntity, IWorldAccessor world) => world.PlaySoundAt(new AssetLocation("sounds/" + sound), byEntity);
    private bool IsLiquidStack(ItemStack liquid) => liquid != null && liquid.Collectible.Code.Equals(liquidStack.Code);
    private int GetLiquidAsInt(WaterTightContainableProps props) => (int)Math.Ceiling((takeQuantity) * props.ItemsPerLitre);
    private ItemStack TryConsumeIngredient(ItemSlot slot) => slot.TakeOut(ingredientQuantity);

    private static void CanSpawnItemStack(EntityAgent byEntity, IWorldAccessor world, IPlayer byPlayer, ItemStack outputstack)
    {
      if (!byPlayer.InventoryManager.TryGiveItemstack(outputstack))
      {
        world.SpawnItemEntity(outputstack, byEntity.Pos.XYZ);
      }
    }

    private static void CanSpawnParticles(ItemSlot slot, EntityAgent byEntity, IWorldAccessor world, bool spawnParticles)
    {
      if (spawnParticles)
      {
        world.SpawnCubeParticles(byEntity.Pos.XYZ, slot.Itemstack.Clone(), 0.1f, 80, 0.3f);
      }
    }
  }
}