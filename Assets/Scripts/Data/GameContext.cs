using System;

[Serializable]
public sealed class GameContext
{
	private readonly WorldContext world = new WorldContext();
	private readonly PlayerData player = new PlayerData();

	public WorldContext World => world;
	public PlayerData Player => player;

	public static GameContext CreateFromSave(GameSaveData saveData)
	{
		var context = new GameContext();
		context.LoadFromSave(saveData);
		return context;
	}

	public void LoadFromSave(GameSaveData saveData)
	{
		saveData ??= new GameSaveData();
		world.ApplySeed(saveData.WorldSeed);
		player.WorldCell = saveData.PlayerWorldCell;
	}
}
