public static class GameSession
{
	public static string CurrentSaveId { get; private set; }
	public static GameData CurrentData { get; private set; }

	private static bool hasPendingLoad;

	public static void BeginLoad(string saveId, GameData gameData)
	{
		CurrentSaveId = saveId;
		CurrentData = gameData;
		hasPendingLoad = gameData != null;
	}

	public static bool TryConsumePendingLoad(out string saveId, out GameData gameData)
	{
		saveId = CurrentSaveId;
		gameData = CurrentData;

		if (!hasPendingLoad || gameData == null)
		{
			return false;
		}

		hasPendingLoad = false;
		return true;
	}
}
