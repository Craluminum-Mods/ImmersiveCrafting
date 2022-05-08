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
    string actionlangcode;
    string sound;
    float takeQuantity;
    int ingredientQuantity;
    JsonItemStack outputStack;

    public CollectibleBehaviorUseOnBucket(CollectibleObject collObj) : base(collObj)
    {
    }

    public override void Initialize(JsonObject properties)
    {
      base.Initialize(properties);

      actionlangcode = properties["actionLangCode"].AsString();
      sound = properties["sound"].AsString();
      takeQuantity = properties["consumeLiters"].AsFloat();
      ingredientQuantity = properties["ingredientQuantity"].AsInt();
      outputStack = properties["outputStack"].AsObject<JsonItemStack>();
    }

    public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
      Interact(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling, ref handling);
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
            var liquid = container.GetContent(blockSel.Position);
            if (liquid != null && liquid.Collectible.Code.Equals(liquidCode))
            {
              var props = BlockLiquidContainerBase.GetContainableProps(liquid);
              if (props != null)
              {
                int takeAmount = (int)Math.Ceiling((takeQuantity) * props.ItemsPerLitre);
                if (takeAmount > 0)
                {
                  liquid = container.TryTakeContent(blockSel.Position, takeAmount);
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
                    byEntity.World.SpawnCubeParticles(byEntity.Pos.XYZ, itemslot.Itemstack.Clone(), 0.1f, 80, 0.3f);
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/" + sound), byEntity);
                    itemslot.TakeOut(ingredientQuantity);  /// Ignores ingredientQuantity completely when less items left
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