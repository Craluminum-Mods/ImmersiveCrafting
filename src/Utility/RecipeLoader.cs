// using System.Collections.Generic;
// using Newtonsoft.Json.Linq;
// using Vintagestory.API.Common;

// namespace ImmersiveCrafting;

// public static class RecipeLoader
// {
//     public static void LoadRecipes(this ICoreAPI api, out List<ImmersiveRecipe> loadedRecipes)
//     {
//         List<ImmersiveRecipe> recipes = new();
//         Dictionary<AssetLocation, JToken> files = api.Assets.GetMany<JToken>(api.Logger, "recipes/immersive");

//         int achievementQuantity = 0;
//         foreach (KeyValuePair<AssetLocation, JToken> val in files)
//         {
//             if (val.Value is JObject)
//             {
//                 recipes.Add(val.Value.ToObject<ImmersiveRecipe>());
//                 achievementQuantity++;
//             }
//             if (val.Value is not JArray)
//             {
//                 continue;
//             }
//             using IEnumerator<JToken> enumerator = (val.Value as JArray).GetEnumerator();
//             while (enumerator.MoveNext())
//             {
//                 recipes.Add(enumerator.Current.ToObject<ImmersiveRecipe>());
//                 achievementQuantity++;
//             }
//         }

//         api.Logger.Debug("{0} achievements loaded from {1} files", achievementQuantity, files.Count);

//         loadedRecipes = recipes;
//     }
// }

// public class ImmersiveRecipe : RecipeRegistryBase
// {
//     public override void FromBytes(IWorldAccessor resolver, int quantity, byte[] data)
//     {
//         throw new System.NotImplementedException();
//     }

//     public override void ToBytes(IWorldAccessor resolver, out byte[] data, out int quantity)
//     {
//         throw new System.NotImplementedException();
//     }
// }