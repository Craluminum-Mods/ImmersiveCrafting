using Vintagestory.API.Common;

[assembly: ModInfo("ImmersiveCrafting",
  Authors = new[] { "Craluminum2413" })]


namespace ImmersiveCrafting.Load
{
  class ImmersiveCrafting : ModSystem
  {
    public override void Start(ICoreAPI api)
    {
      base.Start(api);
      api.RegisterCollectibleBehaviorClass("IC_UseOnLiquidContainer", typeof(CollectibleBehaviorUseOnLiquidContainer));
      api.RegisterCollectibleBehaviorClass("IC_SealCrock", typeof(CollectibleBehaviorSealCrock));
      api.RegisterCollectibleBehaviorClass("IC_WaxCheese", typeof(CollectibleBehaviorWaxCheese));
      api.RegisterBlockBehaviorClass("IC_UseToolThenRemoveBlock", typeof(BlockBehaviorUseToolThenRemoveBlock));
      api.World.Logger.Event("started 'Immersive Crafting' mod");
    }
  }
}