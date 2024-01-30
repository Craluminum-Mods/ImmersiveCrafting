using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace ImmersiveCrafting.Utils
{
  public class ImmersiveCraftingUtils
  {
    public bool SatisfiesLiquid(ItemSlot slot, ItemStack liquidstack, ItemStack liquid, float consumeLiters, int ingredientQuantity)
    {
      return liquid?.Collectible.Code.Equals(liquidstack.Collectible.Code) != false
                && BlockLiquidContainerBase.GetContainableProps(liquid) != null
                && (int)Math.Ceiling(consumeLiters * BlockLiquidContainerBase.GetContainableProps(liquid).ItemsPerLitre) <= liquid.StackSize && slot.StackSize >= ingredientQuantity;
    }

    public void GetSound(IPlayer byPlayer, string sound)
    {
      var interactionSoundsEnabled = (bool)byPlayer.Entity.World.Config.TryGetBool("InteractionSoundsEnabled");

      if (interactionSoundsEnabled && sound != null)
      {
        byPlayer.Entity.World.PlaySoundAt(new AssetLocation(sound), byPlayer.Entity);
      }
    }

    public void CanSpawnItemStack(IPlayer byPlayer, ItemStack outputstack)
    {
      if (!byPlayer.InventoryManager.TryGiveItemstack(outputstack))
      {
        byPlayer.Entity.World.SpawnItemEntity(outputstack, byPlayer.Entity.Pos.XYZ);
      }
    }

    /// <summary>
    /// Spawn particles based on held ItemStack
    /// </summary>
    public void CanSpawnParticles(IPlayer byPlayer, bool spawnParticles)
    {
      var interactionParticlesEnabled = (bool)byPlayer.Entity.World.Config.TryGetBool("InteractionParticlesEnabled");

      if (spawnParticles && interactionParticlesEnabled)
      {
        byPlayer.Entity.World.SpawnCubeParticles(byPlayer.Entity.Pos.XYZ, byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Clone(), 0.1f, 10, 0.3f);
      }
    }

    /// <summary>
    /// Spawn particles based on selected block
    /// </summary>
    public void CanSpawnParticles(IPlayer byPlayer, bool spawnParticles, BlockPos pos)
    {
      var interactionParticlesEnabled = (bool)byPlayer.Entity.World.Config.TryGetBool("InteractionParticlesEnabled");

      if (spawnParticles && interactionParticlesEnabled)
      {
        byPlayer.Entity.BlockSelection.Block.SpawnBlockBrokenParticles(pos);
      }
    }

    public StringBuilder GetLiquidDescription(JsonItemStack stack, float litres)
    {
      StringBuilder dsc = new();
      var incontainerrname = Lang.Get($"{stack.Code.Domain}:incontainer-{stack.Type.ToString().ToLower()}-{stack.Code.Path}");
      if (litres == 1)
      {
        dsc.Append(Lang.Get("{0} litre of {1}", litres, incontainerrname));
      }
      else
      {
        dsc.Append(Lang.Get("{0} litres of {1}", litres, incontainerrname));
      }

      return dsc;
    }

    public StringBuilder GetOutputDescription(JsonItemStack stack)
    {
      StringBuilder dsc = new();
      var incontainerrname = Lang.Get($"{stack.Code.Domain}:{stack.Type.ToString().ToLower()}-{stack.Code.Path}");
      dsc.Append(Lang.Get("{0}x {1}", stack.Quantity, incontainerrname));
      return dsc;
    }
  }
}