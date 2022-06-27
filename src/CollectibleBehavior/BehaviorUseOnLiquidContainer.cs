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
    string sound;
    float consumeLiters;
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
        interactions = ObjectCacheUtil.GetOrCreate(api, "useOnLiquidContainerInteractions" + outputStack.Code, () =>
        {
          List<ItemStack> liquidContainerStacks = new();
          List<ItemStack> barrelStacks = new();

          foreach (CollectibleObject obj in api.World.Collectibles)
          {
            if (obj is BlockLiquidContainerBase blc && blc.IsTopOpened && blc.AllowHeldLiquidTransfer)
            {
              liquidContainerStacks.Add(new ItemStack(obj));
            }
            if (obj is BlockBarrel)
            {
              barrelStacks.Add(new ItemStack(obj));
            }
          }

          var outputStackLang = Lang.Get($"{outputStack.Code.Domain}:{outputStack.Type.ToString().ToLower()}-{outputStack.Code.Path}");
          var outputDsc = Lang.Get("immersivecrafting:Make {0}", outputStackLang);

          return new WorldInteraction[]
          {
            new WorldInteraction()
            {
              ActionLangCode = outputDsc,
              MouseButton = EnumMouseButton.Right,
              Itemstacks = liquidContainerStacks.ToArray()
            },
            new WorldInteraction()
            {
              ActionLangCode = outputDsc,
              MouseButton = EnumMouseButton.Right,
              HotKeyCode = "sneak",
              Itemstacks = barrelStacks.ToArray()
            },
          };
        });
      }, "initUseOnLiquidContainerInteractions");
    }

    public override void Initialize(JsonObject properties)
    {
      base.Initialize(properties);

      spawnParticles = properties["spawnParticles"].AsBool();
      sound = properties["sound"].AsString();
      consumeLiters = properties["consumeLiters"].AsFloat();
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
      if (forbidInteraction) { return new WorldInteraction[0]; }

      handling = EnumHandling.PassThrough;
      return interactions.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
    }

    public void Interact(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      if (forbidInteraction) return;
      if (blockSel == null) return;

      IPlayer byPlayer = null;
      if (byEntity is EntityPlayer player) byPlayer = byEntity.World.PlayerByUid(player.PlayerUID);
      if (byPlayer == null) return;

      ItemStack outputstack = null;
      if (outputStack.Resolve(byEntity.World, "output stacks"))
      {
        outputstack = outputStack.ResolvedItemstack;
      }

      ItemStack liquidstack = null;
      if (liquidStack.Resolve(byEntity.World, "liquid stacks"))
      {
        liquidstack = liquidStack.ResolvedItemstack;
      }

      var pos = blockSel.Position;
      var block = byEntity.World.BlockAccessor.GetBlock(pos);
      var blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(pos);
      var blockCnt = block as BlockLiquidContainerBase;
      var begs = blockEntity as BlockEntityGroundStorage;
      var gsslot = begs?.GetSlotAt(blockSel);

      if (blockCnt?.IsTopOpened == true)
      {
        var liquid = blockCnt.GetContent(pos);
        if (!IsLiquidStack(liquid, liquidstack)
          || GetProps(liquid) == null
          || !SatisfiesQuantity(slot, liquid, GetLiquidAsInt(GetProps(liquid))))
        {
          return;
        }

        liquid = blockCnt.TryTakeContent(pos, GetLiquidAsInt(GetProps(liquid)));
        if (liquid == null) return;

        CanSpawnItemStack(byPlayer, outputstack);
        CanSpawnParticles(byPlayer, spawnParticles);
        GetSound(byPlayer, sound);
        slot.TakeOut(ingredientQuantity);
        slot.MarkDirty();
        handHandling = EnumHandHandling.PreventDefault;
      }

      if (block is BlockBarrel && blockEntity is BlockEntityBarrel bebarrel)
      {
        var liquid = bebarrel.Inventory[1].Itemstack;
        if (!IsLiquidStack(liquid, liquidstack)
          || GetProps(liquid) == null
          || !SatisfiesQuantity(slot, liquid, GetLiquidAsInt(GetProps(liquid))))
        {
          return;
        }

        liquid = bebarrel.Inventory[1].TakeOut(GetLiquidAsInt(GetProps(liquid)));
        if (liquid == null) return;

        CanSpawnItemStack(byPlayer, outputstack);
        CanSpawnParticles(byPlayer, spawnParticles);
        GetSound(byPlayer, sound);
        slot.TakeOut(ingredientQuantity);
        bebarrel.MarkDirty(true);
        slot.MarkDirty();
        handHandling = EnumHandHandling.PreventDefault;
      }

      if (block is BlockGroundStorage)
      {
        if (gsslot?.Empty != false || gsslot.Itemstack.Collectible is not BlockLiquidContainerBase) return;

        blockCnt = gsslot.Itemstack.Block as BlockLiquidContainerBase;
        var liquid = blockCnt.GetContent(gsslot.Itemstack);
        if (!IsLiquidStack(liquid, liquidstack)
          || GetProps(liquid) == null
          || !SatisfiesQuantity(slot, liquid, GetLiquidAsInt(GetProps(liquid))))
        {
          return;
        }

        liquid = blockCnt.TryTakeContent(gsslot.Itemstack, GetLiquidAsInt(GetProps(liquid)));
        if (liquid == null) return;

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

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
      {
      var liquidDsc = Utils.GetLiquidDescription(liquidStack, consumeLiters);
      var outputDsc = Utils.GetOutputDescription(outputStack);
      base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
      dsc.AppendLine(Lang.Get("immersivecrafting:Use on {0} to get {1}", liquidDsc, outputDsc));
    }
  }
}