using System;

public static partial class EventCenter
{
	public static event Action<GameSaveData> EnteringGame;

	public static void EmitEnteringGame(GameSaveData p_saveData)
	{
		EnteringGame?.Invoke(p_saveData);
	}
}
