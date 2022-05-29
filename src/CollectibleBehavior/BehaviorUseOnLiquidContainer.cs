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
    bool forbidInteraction;

    public CollectibleBehaviorUseOnLiquidContainer(CollectibleObject collObj) : base(collObj) { }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);

      api.Event.EnqueueMainThreadTask(() =>
      {
        interactions = ObjectCacheUtil.GetOrCreate(api, "liquidContainerInteractions" + actionlangcode + outputStack.Code, () =>
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
      forbidInteraction = properties["forbidInteraction"].AsBool();
    }

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      Interact(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
      if (forbidInteraction) { return new WorldInteraction[0]; };

      handling = EnumHandling.PassThrough;
      return interactions.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
    }

    public void Interact(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      if (forbidInteraction) return;
      if (blockSel == null) return;

      var block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
      var blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position);

      IPlayer byPlayer = null;
      if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
      if (byPlayer == null) return;

      ItemStack outputstack = null;
      if (outputStack.Resolve(byEntity.World, "output stacks"))
      {
        outputstack = outputStack.ResolvedItemstack;
      }

      ItemStack liquidstack = null;
      if (liquidStack.Resolve(byEntity.World, "output stacks"))
      {
        liquidstack = liquidStack.ResolvedItemstack;
      }

      var blockCnt = block as BlockLiquidContainerBase;

      if (blockCnt != null && blockCnt.IsTopOpened)
      {
        var liquid = blockCnt.GetContent(blockSel.Position);
        if (IsLiquidStack(liquid, liquidstack)
          && GetProps(liquid) != null
          && SatisfiesQuantity(slot, liquid, GetLiquidAsInt(GetProps(liquid))))
        {
          liquid = blockCnt.TryTakeContent(blockSel.Position, GetLiquidAsInt(GetProps(liquid)));
          if (liquid != null)
          {
            CanSpawnItemStack(byPlayer, outputstack);
            CanSpawnParticles(byPlayer, spawnParticles);
            GetSound(byPlayer, sound);
            slot.TakeOut(ingredientQuantity);
            slot.MarkDirty();
            handHandling = EnumHandHandling.PreventDefault;
          }
        }
      }
      else if (block is BlockBarrel)
      {
        var bebarrel = blockEntity as BlockEntityBarrel;
        if (bebarrel != null)
        {
          var liquid = bebarrel.Inventory[1].Itemstack;
          if (IsLiquidStack(liquid, liquidstack)
            && GetProps(liquid) != null
            && SatisfiesQuantity(slot, liquid, GetLiquidAsInt(GetProps(liquid))))
          {
            liquid = bebarrel.Inventory[1].TakeOut(GetLiquidAsInt(GetProps(liquid)));
            if (liquid != null)
            {
              CanSpawnItemStack(byPlayer, outputstack);
              CanSpawnParticles(byPlayer, spawnParticles);
              GetSound(byPlayer, sound);
              slot.TakeOut(ingredientQuantity);
              bebarrel.MarkDirty(true);
              slot.MarkDirty();
              handHandling = EnumHandHandling.PreventDefault;
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
          if (IsLiquidStack(liquid, liquidstack)
            && GetProps(liquid) != null
            && SatisfiesQuantity(slot, liquid, GetLiquidAsInt(GetProps(liquid))))
          {
            liquid = blockCnt.TryTakeContent(gsslot.Itemstack, GetLiquidAsInt(GetProps(liquid)));
            if (liquid != null)
            {
              CanSpawnItemStack(byPlayer, outputstack);
              CanSpawnParticles(byPlayer, spawnParticles);
              GetSound(byPlayer, sound);
              slot.TakeOut(ingredientQuantity);
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

    private static WaterTightContainableProps GetProps(ItemStack liquid) => BlockLiquidContainerBase.GetContainableProps(liquid);
    private static bool IsLiquidStack(ItemStack liquid, ItemStack liquidstack) => liquid != null && liquid.Collectible.Code.Equals(liquidstack.Collectible.Code);
    private int GetLiquidAsInt(WaterTightContainableProps props) => (int)Math.Ceiling((takeQuantity) * props.ItemsPerLitre);

    private bool SatisfiesQuantity(ItemSlot slot, ItemStack liquid, int takeAmount)
    {
      return takeAmount <= liquid.StackSize && slot.StackSize >= ingredientQuantity;
    }

    private void GetSound(IPlayer byPlayer, string sound)
    {
      bool interactionSoundsEnabled = (bool)byPlayer.Entity.World.Config.TryGetBool("InteractionSoundsEnabled");

      if (interactionSoundsEnabled)
      {
        byPlayer.Entity.World.PlaySoundAt(new AssetLocation(sound), byPlayer.Entity);
      }
    }

    private static void CanSpawnItemStack(IPlayer byPlayer, ItemStack outputstack)
    {
      if (!byPlayer.InventoryManager.TryGiveItemstack(outputstack))
      {
        byPlayer.Entity.World.SpawnItemEntity(outputstack, byPlayer.Entity.Pos.XYZ);
      }
    }

    private static void CanSpawnParticles(IPlayer byPlayer, bool spawnParticles)
    {
      bool interactionParticlesEnabled = (bool)byPlayer.Entity.World.Config.TryGetBool("InteractionParticlesEnabled");

      if (spawnParticles && interactionParticlesEnabled)
      {
        byPlayer.Entity.World.SpawnCubeParticles(byPlayer.Entity.Pos.XYZ, byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Clone(), 0.1f, 10, 0.3f);
      }
    }
  }
}