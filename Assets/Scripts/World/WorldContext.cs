using System;

[Serializable]
public sealed class WorldContext
{
	private int seed;
	private readonly WorldData worldData = new WorldData();
	private readonly MetaWorldData metaWorldData = new MetaWorldData();
	private readonly WorldLoader worldLoader = new WorldLoader();
	private readonly MetaWorldGenerator metaWorldGenerator = new MetaWorldGenerator();

	public int Seed => seed;
	public WorldData WorldData => worldData;
	public MetaWorldData MetaWorldData => metaWorldData;
	public WorldLoader WorldLoader => worldLoader;
	public MetaWorldGenerator MetaWorldGenerator => metaWorldGenerator;

	public void ApplySeed(int value)
	{
		bool seedChanged = seed != value || worldLoader.Seed != value || metaWorldGenerator.Seed != value;
		seed = value;
		worldLoader.SetSeed(value);
		metaWorldGenerator.SetSeed(value);

		if (seedChanged)
		{
			ClearLoadedData();
		}
	}

	public void ClearLoadedData()
	{
		worldData.Clear();
		metaWorldData.Clear();
	}
}
