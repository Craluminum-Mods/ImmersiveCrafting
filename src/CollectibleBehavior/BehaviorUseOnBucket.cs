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

  public class UseOnBucketProperties
  {
    // public EnumInteractionKeys Hotkey = EnumInteractionKeys.RightClick;
    // public AssetLocation InteractSound = new AssetLocation("sounds/player/build");
    // public JsonItemStack LiquidStack;
    public JsonItemStack OutputStack;
    public ItemStack ResolvedItemStack { get; internal set; }

    public UseOnBucketProperties Clone()
    {
      return new UseOnBucketProperties()
      {
        // Hotkey = Hotkey,
        // InteractSound = InteractSound,
        // LiquidStack = this.LiquidStack.Clone(),
        OutputStack = this.OutputStack.Clone()
      };
    }
  }

  public class CollectibleBehaviorUseOnBucket : CollectibleBehavior
  {
    public UseOnBucketProperties InteractionProps { get; protected set; }
    private static AssetLocation waterCode = new AssetLocation("waterportion");
    public static UseOnBucketProperties OutputStack => OutputStack;
    string actionlangcode;

    public CollectibleBehaviorUseOnBucket(CollectibleObject collObj) : base(collObj)
    {
    }

    public override void Initialize(JsonObject properties)
    {
      base.Initialize(properties);

      actionlangcode = properties["actionLangCode"].AsString();
      InteractionProps = properties.AsObject<UseOnBucketProperties>(null, collObj.Code.Domain);
    }

    public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
        Interact(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
    }

    public static void Interact(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      IWorldAccessor world = byEntity?.World;

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
            if (stack != null && stack.Collectible.Code.Equals(waterCode))
            {
              var props = BlockLiquidContainerBase.GetContainableProps(stack);
              if (props != null)
              {
                int takeAmount = (int)Math.Ceiling((1f) * props.ItemsPerLitre);
                if (takeAmount > 0)
                {
                  stack = container.TryTakeContent(blockSel.Position, takeAmount);
                  if (stack != null)
                  {
                    if (!byPlayer.InventoryManager.TryGiveItemstack(OutputStack.ResolvedItemStack))
                    {
                      byEntity.Api.World.SpawnItemEntity(OutputStack.ResolvedItemStack, blockSel.Position.ToVec3d().Add(0.5, 0.2, 0.5));
                    }
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