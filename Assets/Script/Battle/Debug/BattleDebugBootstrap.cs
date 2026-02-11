using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Battle.Debugging
{
#if UNITY_EDITOR
	public static class BattleDebugBootstrap
	{
		private const string BattleSceneName = "BattleScene";
		private static bool hasRun = false;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void TryBootstrap()
		{
			if (hasRun)
			{
				return;
			}

			Scene scene = SceneManager.GetActiveScene();
			if (scene.name != BattleSceneName)
			{
				return;
			}

			ServiceLocator locator = ServiceLocator.Instance;
			if (locator == null)
			{
				Debug.LogError("BattleDebugBootstrap: ServiceLocator is not initialized.");
				return;
			}

			if (locator.BattleBoardService != null && locator.BattleBoardService.HasData)
			{
				return;
			}

			World.Service worldService = locator.WorldService;
			if (worldService == null)
			{
				Debug.LogError("BattleDebugBootstrap: World.Service is not available.");
				return;
			}

			hasRun = true;

			for (int x = 0; x <= 2; x++)
			{
				for (int z = 0; z <= 2; z++)
				{
					worldService.GetOrCreateChunk(new World.Chunk.Model.Coordinates(x, 0, z));
				}
			}

			int centerX = (World.Chunk.Model.Data.SizeX * 1) + (World.Chunk.Model.Data.SizeX / 2);
			int centerZ = (World.Chunk.Model.Data.SizeZ * 1) + (World.Chunk.Model.Data.SizeZ / 2);
			var centerWorldPosition = new Vector3(centerX, 0, centerZ);

			if (locator.PlayerService != null)
			{
				locator.PlayerService.UpdatePlayerPosition(centerWorldPosition);
			}

			Vector2Int boardArea = new Vector2Int(10, 10);
			if (locator.EncounterService != null)
			{
				var encounterTable = locator.EncounterService.GetEncounterTable(new World.Chunk.Model.Coordinates(1, 0, 1));
				if (encounterTable != null)
				{
					boardArea = encounterTable.BoardArea;
				}
			}

			Voxel.Model.Cell[,,] cells = worldService.ExtrudeCells(new Vector2Int(centerX, centerZ), boardArea);
			locator.BattleBoardService.Setup(cells);

			Debug.Log("BattleDebugBootstrap: Mocked world chunks 0/0/0 to 2/0/2 and initialized battle board.");
		}
	}
#endif
}
