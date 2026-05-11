public sealed class SaveService
{
	private readonly IOFileService ioFileService;

	private GameContext gameContext;
	private PlayerService playerService;
	private GameSaveData boundSaveData;

	public SaveService(IOFileService p_ioFileService)
	{
		ioFileService = p_ioFileService;
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

		GameSaveFileData saveFileData = CreateSaveFileData(target);
		UpdateRuntimeSaveData(target, saveFileData);

		bool success = ioFileService == null || ioFileService.TrySave(saveFileData);
		EventCenter.EmitSaveCompleted(target, success);
		return success;
	}

	public bool TryLoadFromFile(GameSaveData p_targetSaveData = null)
	{
		if (ioFileService == null ||
			!ioFileService.TryLoad(out GameSaveFileData saveFileData))
		{
			return false;
		}

		return TryLoad(saveFileData, p_targetSaveData);
	}

	public bool TryLoad(GameSaveFileData p_saveFileData, GameSaveData p_targetSaveData = null)
	{
		GameSaveData target = p_targetSaveData ?? boundSaveData;
		if (p_saveFileData == null ||
			target == null ||
			gameContext == null ||
			playerService == null ||
			p_saveFileData.Player == null)
		{
			return false;
		}

		gameContext.World.ApplySeed(p_saveFileData.WorldSeed);
		if (!playerService.LoadFromSaveData(p_saveFileData.Player))
		{
			return false;
		}

		UpdateRuntimeSaveData(target, p_saveFileData);
		return true;
	}

	public GameSaveFileData CreateSaveFileData(GameSaveData p_targetSaveData = null)
	{
		GameSaveData target = p_targetSaveData ?? boundSaveData;
		return new GameSaveFileData
		{
			Version = GameSaveFileData.CurrentVersion,
			WorldSeed = gameContext?.World?.Seed ?? target?.WorldSeed ?? 0,
			RespawnPoint = SerializableVector3Int.From(target?.RespawnPoint ?? default),
			Player = playerService?.CreateSaveData() ?? new PlayerSaveData()
		};
	}

	private void OnSaveRequested(GameSaveData p_targetSaveData)
	{
		Save(p_targetSaveData);
	}

	private void UpdateRuntimeSaveData(GameSaveData p_targetSaveData, GameSaveFileData p_saveFileData)
	{
		if (p_targetSaveData == null || p_saveFileData == null)
		{
			return;
		}

		p_targetSaveData.SetWorldSeed(p_saveFileData.WorldSeed);
		p_targetSaveData.SetRespawnPoint(p_saveFileData.RespawnPoint.ToVector3Int());
		if (gameContext?.Player != null)
		{
			p_targetSaveData.CopyPlayerFrom(gameContext.Player);
			if (p_saveFileData.Player != null)
			{
				p_targetSaveData.Player?.SetPosition(p_saveFileData.Player.Position, true);
			}
		}
	}
}
