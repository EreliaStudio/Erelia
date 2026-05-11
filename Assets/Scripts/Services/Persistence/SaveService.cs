using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class SaveService
{
	private readonly IOFileService ioFileService;
	private readonly ReferenceRegistry referenceRegistry;

	private GameContext gameContext;
	private PlayerService playerService;
	private GameSaveData boundSaveData;

	public SaveService(IOFileService p_ioFileService, ReferenceRegistry p_referenceRegistry)
	{
		ioFileService = p_ioFileService;
		referenceRegistry = p_referenceRegistry;
	}

	public void Initialize()
	{
		EventCenter.SaveRequested += OnSaveRequested;
	}

	public void Shutdown()
	{
		EventCenter.SaveRequested -= OnSaveRequested;
		gameContext = null;
		playerService = null;
		boundSaveData = null;
	}

	public GameContext CreateGameContext(GameSaveData p_saveData)
	{
		boundSaveData = p_saveData ?? new GameSaveData();
		gameContext = GameContext.CreateFromSave(boundSaveData);
		return gameContext;
	}

	public void BindRuntimeServices(GameContext p_gameContext, PlayerService p_playerService)
	{
		gameContext = p_gameContext;
		playerService = p_playerService;
	}

	public void BindSaveData(GameSaveData p_saveData)
	{
		boundSaveData = p_saveData;
	}

	public bool Save(GameSaveData p_targetSaveData = null)
	{
		GameSaveData target = p_targetSaveData ?? boundSaveData;
		if (target == null || gameContext == null || playerService == null)
		{
			EventCenter.EmitSaveCompleted(target, false);
			return false;
		}

		JObject saveJson = BuildSaveJson(target);
		bool success = ioFileService == null || ioFileService.TrySave(saveJson);

		if (success)
		{
			UpdateRuntimeSaveData(target, saveJson);
		}

		EventCenter.EmitSaveCompleted(target, success);
		return success;
	}

	public bool TryLoadFromFile(GameSaveData p_targetSaveData = null)
	{
		if (ioFileService == null || !ioFileService.TryLoad(out JObject saveJson))
		{
			return false;
		}

		return TryLoad(saveJson, p_targetSaveData);
	}

	public bool TryLoad(JObject p_saveJson, GameSaveData p_targetSaveData = null)
	{
		GameSaveData target = p_targetSaveData ?? boundSaveData;
		if (p_saveJson == null || target == null || gameContext == null || playerService == null)
		{
			return false;
		}

		int worldSeed = p_saveJson["worldSeed"]?.Value<int>() ?? 0;
		gameContext.World.ApplySeed(worldSeed);

		if (!playerService.LoadFromJson(p_saveJson["player"] as JObject, referenceRegistry))
		{
			return false;
		}

		UpdateRuntimeSaveData(target, p_saveJson);
		return true;
	}

	public JObject CreateSaveJson(GameSaveData p_targetSaveData = null)
	{
		return BuildSaveJson(p_targetSaveData ?? boundSaveData);
	}

	private JObject BuildSaveJson(GameSaveData p_target)
	{
		return new JObject
		{
			["worldSeed"] = gameContext?.World?.Seed ?? p_target?.WorldSeed ?? 0,
			["respawnPoint"] = SaveHelper.ToJson(p_target?.RespawnPoint ?? default),
			["player"] = playerService?.ToJson(referenceRegistry) ?? new JObject()
		};
	}

	private void OnSaveRequested(GameSaveData p_targetSaveData)
	{
		Save(p_targetSaveData);
	}

	private void UpdateRuntimeSaveData(GameSaveData p_target, JObject p_saveJson)
	{
		if (p_target == null || p_saveJson == null)
		{
			return;
		}

		p_target.SetWorldSeed(p_saveJson["worldSeed"]?.Value<int>() ?? 0);
		p_target.SetRespawnPoint(SaveHelper.ToVector3Int(p_saveJson["respawnPoint"] as JObject));

		if (gameContext?.Player != null)
		{
			p_target.CopyPlayerFrom(gameContext.Player);

			JObject playerJson = p_saveJson["player"] as JObject;
			if (playerJson != null)
			{
				Vector3 position = SaveHelper.ToVector3(playerJson["position"] as JObject);
				p_target.Player?.SetPosition(position, true);
			}
		}
	}
}
