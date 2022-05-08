using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ImmersiveCrafting
{
  // public enum EnumInteractionKeys
  // {
  //   RightClick,
  //   Sneak,
  //   Sprint,
  //   SneakSprint,
  // }

  public class CollectibleBehaviorUseOnBucket : CollectibleBehavior
  {
    public AssetLocation liquidCode = new AssetLocation("waterportion");
    public AssetLocation testOutputStack = new AssetLocation("beeswax");
    string actionlangcode;
    string sound;
    float takeQuantity;
    public JsonItemStack outputStack;

    public CollectibleBehaviorUseOnBucket(CollectibleObject collObj) : base(collObj)
    {
    }

    public override void Initialize(JsonObject properties)
    {
      base.Initialize(properties);

      actionlangcode = properties["actionLangCode"].AsString();
      sound = properties["sound"].AsString();
      takeQuantity = properties["consumeLiters"].AsFloat();
      outputStack = properties["output"].AsObject<JsonItemStack>((null), collObj.Code.Domain);
    }

    public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      Interact(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
    }

    public void Interact(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      IWorldAccessor world = byEntity.World;

      IPlayer byPlayer = null;
      if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
      if (byPlayer == null) return;

      if (firstEvent && handHandling != EnumHandHandling.PreventDefault)
      {
        if (blockSel != null && byEntity.World.BlockAccessor.GetBlock(blockSel.Position) is BlockLiquidContainerBase container)
        {
          if (container.IsTopOpened)
          {
            var stack = container.GetContent(blockSel.Position);
            if (stack != null && stack.Collectible.Code.Equals(liquidCode))
            {
              var props = BlockLiquidContainerBase.GetContainableProps(stack);
              if (props != null)
              {
                int takeAmount = (int)Math.Ceiling((takeQuantity) * props.ItemsPerLitre);
                if (takeAmount > 0)
                {
                  stack = container.TryTakeContent(blockSel.Position, takeAmount);
                  if (stack != null)
                  {
                    ItemStack outputstack = new ItemStack(world.GetItem(testOutputStack));
                    if (!byPlayer.InventoryManager.TryGiveItemstack(outputstack))
                    {
                      byEntity.World.SpawnItemEntity(outputstack, byEntity.Pos.XYZ);
                    }
                    itemslot.TakeOut(1);
                    itemslot.MarkDirty();
                    byEntity.World.SpawnCubeParticles(byEntity.Pos.XYZ, itemslot.Itemstack.Clone(), 0.1f, 80, 0.3f);
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/" + sound), byEntity);
                    handHandling = EnumHandHandling.PreventDefault;
                  }
                }
              }
            }
          }
        }
      }
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
    {
      handling = EnumHandling.PassThrough;
      return new WorldInteraction[]
      {
        new WorldInteraction
        {
            ActionLangCode = actionlangcode,
            MouseButton = EnumMouseButton.Right
        }
      };
    }
  }
}