using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
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
      var beeswaxItem = api.World.GetItem(new AssetLocation("beeswax"));
      beeswaxItem.CollectibleBehaviors = beeswaxItem.CollectibleBehaviors.Append(new CollectibleBehaviorSealCrock(beeswaxItem));
      beeswaxItem.CollectibleBehaviors = beeswaxItem.CollectibleBehaviors.Append(new CollectibleBehaviorWaxCheese(beeswaxItem));

      var fatItem = api.World.GetItem(new AssetLocation("fat"));
      fatItem.CollectibleBehaviors = fatItem.CollectibleBehaviors.Append(new CollectibleBehaviorSealCrock(fatItem));

      var bandageCleanItem = api.World.GetItem(new AssetLocation("bandage-clean"));
      AppendCollectibleBehaviorInSimpleWay(bandageCleanItem, "immersivecrafting:config/useonliquidcontainer/bandage.json", new CollectibleBehaviorUseOnLiquidContainer(bandageCleanItem));

      var pineappleBlock = api.World.GetBlock(new AssetLocation("pineapple"));
      AppendBlockBehaviorInSimpleWay(pineappleBlock, "immersivecrafting:config/removebytool/pineapple.json", new BlockBehaviorRemoveByTool(pineappleBlock));

      var pumpkinBlock = api.World.GetBlock(new AssetLocation("pumpkin-fruit-4"));
      AppendBlockBehaviorInSimpleWay(pumpkinBlock, "immersivecrafting:config/removebytool/pumpkin.json", new BlockBehaviorRemoveByTool(pumpkinBlock));

      var chalkStoneBlock = api.World.GetBlock(new AssetLocation("loosestones-chalk-free"));
      var limeStoneBlock = api.World.GetBlock(new AssetLocation("loosestones-limestone-free"));
      const string loosestonesProperties = "immersivecrafting:config/removebytool/loosestones.json";
      AppendBlockBehaviorInSimpleWay(chalkStoneBlock, loosestonesProperties, new BlockBehaviorRemoveByTool(chalkStoneBlock));
      AppendBlockBehaviorInSimpleWay(limeStoneBlock, loosestonesProperties, new BlockBehaviorRemoveByTool(limeStoneBlock));

      if (api.ModLoader.IsModEnabled("ancienttools"))
      {
        var item = api.World.GetItem(new AssetLocation("ancienttools", "pitch-stick"));
        item.CollectibleBehaviors = item.CollectibleBehaviors.Append(new CollectibleBehaviorSealCrock(item));
      }

      if (api.ModLoader.IsModEnabled("expandedfoods"))
      {
        var item = api.World.GetItem(new AssetLocation("expandedfoods", "soyprep-shelled"));
        AppendCollectibleBehaviorInSimpleWay(item, "immersivecrafting:config/useonliquidcontainer/expandedfoods_soyprep.json", new CollectibleBehaviorUseOnLiquidContainer(item));
      }

      if (api.ModLoader.IsModEnabled("potatoes"))
      {
        var item = api.World.GetItem(new AssetLocation("potatoes", "cornflour"));
        AppendCollectibleBehaviorInSimpleWay(item, "immersivecrafting:config/useonliquidcontainer/morecrops_cornflour.json", new CollectibleBehaviorUseOnLiquidContainer(item));
      }

      foreach (CollectibleObject collobj in api.World.Collectibles)
      {
        if (collobj.Code == null || collobj.Id == 0) continue;

        if (collobj.Code.BeginsWith("game", "sand") && collobj.Code.EndVariant() == collobj.Variant["rock"])
        {
          AppendCollectibleBehaviorInSimpleWay(collobj, "immersivecrafting:config/useonliquidcontainer/sand.json", new CollectibleBehaviorUseOnLiquidContainer(collobj));
        }

        if (collobj.Code.BeginsWith("game", "flour"))
        {
          var behaviorProperties = new JsonObject(api.Assets.Get<JToken>(new AssetLocation(
            "immersivecrafting:config/useonliquidcontainer/flour.json")));

          var serialized = JsonConvert.SerializeObject(behaviorProperties);
          serialized = serialized.Replace("{type}", collobj.Variant["type"]);
          behaviorProperties = JsonConvert.DeserializeObject<JObject>(serialized).ToObject<JsonObject>();

          var instance = new CollectibleBehaviorUseOnLiquidContainer(collobj);
          instance.Initialize(behaviorProperties);
          collobj.CollectibleBehaviors = collobj.CollectibleBehaviors.Append(instance);
        }
      }
    }

    private void AppendCollectibleBehaviorInSimpleWay(CollectibleObject collobj, string propertiesFromJson, CollectibleBehavior instance)
    {
      instance.Initialize(new JsonObject(api.Assets.Get<JToken>(new AssetLocation(propertiesFromJson))));
      collobj.CollectibleBehaviors = collobj.CollectibleBehaviors.Append(instance);
    }

    private void AppendBlockBehaviorInSimpleWay(Block block, string propertiesFromJson, BlockBehavior instance)
    {
      instance.Initialize(new JsonObject(api.Assets.Get<JToken>(new AssetLocation(propertiesFromJson))));
      block.CollectibleBehaviors = block.CollectibleBehaviors.Append(instance);
      block.BlockBehaviors = block.BlockBehaviors.Append(instance);
    }
  }
}