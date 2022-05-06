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
      api.RegisterCollectibleBehaviorClass("IR_UseOnBucket", typeof(CollectibleBehaviorUseOnBucket));
    }
  }
}