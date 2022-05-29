using System;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.API.MathTools;

namespace ImmersiveCrafting
{
  public class BlockBehaviorRemoveByTool : BlockBehavior
  {
    bool spawnParticles;
    string actionlangcode;
    string sound;
    int toolDurabilityCost;
    JsonItemStack outputStack;
    EnumTool[] toolTypes;
    string[] toolTypesStrTmp;
    WorldInteraction[] interactions;
    bool forbidInteraction;

    public BlockBehaviorRemoveByTool(Block block) : base(block)
    {
    }

    public override void OnLoaded(ICoreAPI api)
    {
      base.OnLoaded(api);

      toolTypes = new EnumTool[toolTypesStrTmp.Length];
      for (int i = 0; i < toolTypesStrTmp.Length; i++)
      {
        if (toolTypesStrTmp[i] == null) continue;
        try
        {
          toolTypes[i] = (EnumTool)Enum.Parse(typeof(EnumTool), toolTypesStrTmp[i]);
        }
        catch (Exception)
        {
          api.Logger.Warning("RemoveByTool behavior for block {0}, tool type {1} is not a valid tool type, will default to knife", block.Code, toolTypesStrTmp[i]);
          toolTypes[i] = EnumTool.Knife;
        }
      }
      toolTypesStrTmp = null;

      interactions = ObjectCacheUtil.GetOrCreate(api, "removeByToolInteractions-" + actionlangcode + outputStack.Code, () =>
      {
        List<ItemStack> toolStacks = new List<ItemStack>();

        foreach (CollectibleObject collObj in api.World.Collectibles)
        {
          var tool = collObj.Tool;
          if (tool != null && toolTypes.Contains<EnumTool>((EnumTool)tool))
          {
            toolStacks.Add(new ItemStack(collObj));
          }
        }

        return new WorldInteraction[]
        {
          new WorldInteraction()
          {
            ActionLangCode = actionlangcode,
            MouseButton = EnumMouseButton.Right,
            Itemstacks = toolStacks.ToArray()
          }
        };
      });
    }

    public override void Initialize(JsonObject properties)
    {
      base.Initialize(properties);

      spawnParticles = properties["spawnParticles"].AsBool();
      actionlangcode = properties["actionLangCode"].AsString();
      sound = properties["sound"].AsString();
      toolDurabilityCost = properties["toolDurabilityCost"].AsInt();
      outputStack = properties["outputStack"].AsObject<JsonItemStack>();
      toolTypesStrTmp = properties["toolTypes"].AsArray<string>(new string[0]);
      forbidInteraction = properties["forbidInteraction"].AsBool();
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handling)
    {
      if (forbidInteraction) { return new WorldInteraction[0]; };

      handling = EnumHandling.PassThrough;
      return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handling));
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
      if (forbidInteraction) return true;

      ItemStack outputstack = null;
      if (outputStack.Resolve(world, "output stacks"))
      {
        outputstack = outputStack.ResolvedItemstack;
      }

      ItemSlot activeslot = byPlayer.InventoryManager.ActiveHotbarSlot;
      ItemStack itemslot = activeslot?.Itemstack;

      if (CanUseHeldTool(toolTypes, itemslot))
      {
        itemslot.Collectible.DamageItem(world, byPlayer.Entity, activeslot, toolDurabilityCost);
        CanSpawnItemStack(byPlayer, outputstack);
        CanSpawnParticles(byPlayer, spawnParticles);
        GetSound(byPlayer, sound);
        world.BlockAccessor.SetBlock(0, blockSel.Position);
        handling = EnumHandling.PreventDefault;
      }
      return true;
    }

    private bool CanUseHeldTool(EnumTool[] toolTypes, ItemStack itemslot)
    {
      var tool = itemslot?.Collectible?.Tool;
      if (itemslot?.Collectible.GetDurability(itemslot) >= toolDurabilityCost && tool != null)
      {
        return toolTypes.Contains<EnumTool>((EnumTool)tool);
      }
      else return false;
    }

    private void GetSound(IPlayer byPlayer, string sound)
    {
      bool interactionSoundsEnabled = (bool)byPlayer.Entity.World.Config.TryGetBool("InteractionSoundsEnabled");

      if (interactionSoundsEnabled && sound != null)
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

    private void CanSpawnParticles(IPlayer byPlayer, bool spawnParticles)
    {
      BlockPos pos = byPlayer.Entity.BlockSelection.Position;
      bool interactionParticlesEnabled = (bool)byPlayer.Entity.World.Config.TryGetBool("InteractionParticlesEnabled");

      if (spawnParticles && interactionParticlesEnabled)
      {
        byPlayer.Entity.World.SpawnCubeParticles(pos, pos.ToVec3d().AddCopy(0.5, 0, 0.5), 0.5f, 10, 0.5f);
      }
    }
  }
}
