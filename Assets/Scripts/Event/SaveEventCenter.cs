using System;

public static partial class EventCenter
{
	public static event Action<GameSaveData> SaveRequested;
	public static event Action<GameSaveData, bool> SaveCompleted;

	public static void EmitSaveRequested(GameSaveData p_targetSaveData = null)
	{
		SaveRequested?.Invoke(p_targetSaveData);
	}

	public static void EmitSaveCompleted(GameSaveData p_saveData, bool p_success)
	{
		SaveCompleted?.Invoke(p_saveData, p_success);
	}
}
