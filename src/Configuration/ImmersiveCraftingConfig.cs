namespace ImmersiveCrafting.Configuration
{
  class ImmersiveCraftingConfig
  {
    public bool InteractionSoundsEnabled = true;
    public bool InteractionParticlesEnabled = true;

    public ImmersiveCraftingConfig() { }

    public ImmersiveCraftingConfig(ImmersiveCraftingConfig previousConfig)
    {
      InteractionSoundsEnabled = previousConfig.InteractionSoundsEnabled;
      InteractionParticlesEnabled = previousConfig.InteractionParticlesEnabled;
    }
  }
}