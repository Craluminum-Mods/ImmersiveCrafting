using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace ImmersiveCrafting
{
  public class BehaviorPatches : ModSystem
  {
    ICoreAPI api;

    public override void Start(ICoreAPI api)
    {
      this.api = api;

      if (api is ICoreServerAPI sapi) sapi.Event.AssetsFinalizers += AppendBehaviors;
    }

    private void AppendBehaviors()
    {
      foreach (CollectibleObject collobj in api.World.Collectibles)
      {
        if (collobj.Code == null || collobj.Id == 0) continue;

        if (collobj.Code == new AssetLocation("beeswax"))
        {
          collobj.CollectibleBehaviors = collobj.CollectibleBehaviors.Append(new CollectibleBehaviorSealCrock(collobj));
          collobj.CollectibleBehaviors = collobj.CollectibleBehaviors.Append(new CollectibleBehaviorWaxCheese(collobj));
        }

        if (collobj.Code == new AssetLocation("fat"))
        {
          collobj.CollectibleBehaviors = collobj.CollectibleBehaviors.Append(new CollectibleBehaviorSealCrock(collobj));
        }
      }
    }
  }
}