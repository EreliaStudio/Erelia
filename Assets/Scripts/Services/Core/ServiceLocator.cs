public sealed class ServiceLocator
{
	public static ServiceLocator Instance { get; private set; }

	public ReferenceRegistry ReferenceRegistry { get; }
	public GameContext GameContext { get; }
	public BattleService BattleService { get; }
	public BattleActionCompositionService BattleActionCompositionService { get; }
	public PlayerService PlayerService { get; }
	public BattleLogService BattleLogService { get; }
	public FeatBoardService FeatBoardService { get; }
	public TamingService TamingService { get; }
	public EncounterService EncounterService { get; }
	public WorldService WorldService { get; }
	public SaveService SaveService { get; }
	public IOFileService IOFileService { get; }

	private ServiceLocator(GameSaveData p_saveData)
		: this(p_saveData, null, null)
	{
	}

	private ServiceLocator(
		GameSaveData p_saveData,
		string p_saveDirectoryPath,
		string p_saveFileName)
	{
		ReferenceRegistry = new ReferenceRegistry();
		IOFileService = new IOFileService(p_saveDirectoryPath, p_saveFileName);
		SaveService = new SaveService(IOFileService);
		GameContext = SaveService.CreateGameContext(p_saveData);
		PlayerService = new PlayerService(GameContext);
		SaveService.BindRuntimeServices(GameContext, PlayerService);
		WorldService = new WorldService(GameContext);
		BattleLogService = new BattleLogService();
		FeatBoardService = new FeatBoardService(GameContext, BattleLogService);
		TamingService = new TamingService(GameContext);
		EncounterService = new EncounterService(GameContext);
		BattleActionCompositionService = new BattleActionCompositionService();
		BattleService = new BattleService(GameContext);
	}

	public static void Create(GameSaveData p_saveData)
	{
		Destroy();
		Instance = new ServiceLocator(p_saveData);
		Instance.Initialize();
	}

	public static void CreateWithSaveFileOverride(
		GameSaveData p_saveData,
		string p_saveDirectoryPath,
		string p_saveFileName)
	{
		Destroy();
		Instance = new ServiceLocator(
			p_saveData,
			p_saveDirectoryPath,
			p_saveFileName);
		Instance.Initialize();
	}

	public static void Destroy()
	{
		Instance?.Shutdown();
		Instance = null;
	}

	private void Initialize()
	{
		PlayerService.Initialize();
		WorldService.Initialize();
		BattleLogService.Initialize();
		FeatBoardService.Initialize();
		TamingService.Initialize();
		EncounterService.Initialize();
		SaveService.Initialize();
		BattleActionCompositionService.Initialize();
		BattleService.Initialize();
	}

	private void Shutdown()
	{
		BattleService.Shutdown();
		BattleActionCompositionService.Shutdown();
		SaveService.Shutdown();
		EncounterService.Shutdown();
		TamingService.Shutdown();
		FeatBoardService.Shutdown();
		BattleLogService.Shutdown();
		WorldService.Shutdown();
		PlayerService.Shutdown();
	}
}
