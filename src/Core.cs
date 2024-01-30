using ImmersiveCrafting.Configuration;
using Vintagestory.API.Common;

[assembly: ModInfo(name: "Immersive Crafting", modID: "immersivecrafting")]

namespace ImmersiveCrafting;

public class Core : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        ModConfig.ReadConfig(api);
        // api.RegisterRecipeRegistry<ImmersiveRecipe>("immersive");
        api.RegisterCollectibleBehaviorClass("IC_UseOnLiquidContainer", typeof(CollectibleBehaviorUseOnLiquidContainer));
        api.RegisterCollectibleBehaviorClass("IC_SealCrock", typeof(CollectibleBehaviorSealCrock));
        api.RegisterCollectibleBehaviorClass("IC_WaxCheese", typeof(CollectibleBehaviorWaxCheese));
        api.RegisterBlockBehaviorClass("IC_RemoveByTool", typeof(BlockBehaviorRemoveByTool));
        api.World.Logger.Event("started '{0}' mod", Mod.Info.Name);
    }
}