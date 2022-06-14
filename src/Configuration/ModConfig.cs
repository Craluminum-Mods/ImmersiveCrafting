using Vintagestory.API.Common;

namespace ImmersiveCrafting.Configuration
{
  static class ModConfig
  {
    private const string jsonConfig = "ImmersiveCrafting.json";
    private static ImmersiveCraftingConfig config;

    public static void ReadConfig(ICoreAPI api)
    {
      try
      {
        config = LoadConfig(api);

        if (config == null)
        {
          GenerateConfig(api);
          config = LoadConfig(api);
        }
        else
        {
          GenerateConfig(api, config);
        }
      }
      catch
      {
        GenerateConfig(api);
        config = LoadConfig(api);
      }

      api.World.Config.SetBool("InteractionSoundsEnabled", config.InteractionSoundsEnabled);
      api.World.Config.SetBool("InteractionParticlesEnabled", config.InteractionParticlesEnabled);
    }
    private static ImmersiveCraftingConfig LoadConfig(ICoreAPI api) =>
      api.LoadModConfig<ImmersiveCraftingConfig>(jsonConfig);

    private static void GenerateConfig(ICoreAPI api) =>
      api.StoreModConfig(new ImmersiveCraftingConfig(), jsonConfig);

    private static void GenerateConfig(ICoreAPI api, ImmersiveCraftingConfig previousConfig) =>
      api.StoreModConfig(new ImmersiveCraftingConfig(previousConfig), jsonConfig);
  }
}