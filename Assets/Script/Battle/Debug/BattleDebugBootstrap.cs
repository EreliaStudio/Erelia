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

			if (locator.BattleBoardService != null && locator.BattleBoardService.Data != null)
			{
				return;
			}

			Exploration.World.Service worldService = locator.WorldService;
			if (worldService == null)
			{
				Debug.LogError("BattleDebugBootstrap: Exploration.World.Service is not available.");
				return;
			}

			hasRun = true;

			for (int x = 0; x <= 2; x++)
			{
				for (int z = 0; z <= 2; z++)
				{
					worldService.GetOrCreateChunk(new Exploration.World.Chunk.Model.Coordinates(x, 0, z));
				}
			}

			int generatedSizeX = Exploration.World.Chunk.Model.Data.SizeX * 3;
			int generatedSizeZ = Exploration.World.Chunk.Model.Data.SizeZ * 3;

			int sizeX = Mathf.FloorToInt(generatedSizeX / 2.0f);
			int sizeZ = Mathf.FloorToInt(generatedSizeZ / 2.0f);
			int centerX = generatedSizeX / 2;
			int centerZ = generatedSizeZ / 2;

			Core.Voxel.Model.Cell[,,] cells = worldService.ExtrudeCells(new Vector2Int(centerX, centerZ), new Vector2Int(sizeX, sizeZ));
			locator.BattleBoardService.SetData(cells);

			Debug.Log("BattleDebugBootstrap: Mocked world chunks 0/0/0 to 2/0/2 and initialized battle board.");
		}
	}
#endif
}
