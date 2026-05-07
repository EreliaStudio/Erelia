public sealed class ServiceLocator
{
	public static ServiceLocator Instance { get; private set; }

	public BattleService BattleService { get; }
	public BattleActionCompositionService BattleActionCompositionService { get; }
	public PlayerService PlayerService { get; }
	public FeatBoardService FeatBoardService { get; }
	public TamingService TamingService { get; }
	public EncounterService EncounterService { get; }
	public WorldService WorldService { get; }
	public SaveService SaveService { get; }

	private ServiceLocator(GameContext p_gameContext)
	{
		PlayerService = new PlayerService(p_gameContext);
		WorldService = new WorldService(p_gameContext);
		FeatBoardService = new FeatBoardService(p_gameContext);
		TamingService = new TamingService(p_gameContext);
		EncounterService = new EncounterService(p_gameContext);
		SaveService = new SaveService(p_gameContext);
		BattleActionCompositionService = new BattleActionCompositionService();
		BattleService = new BattleService(p_gameContext);
	}

	public static void Create(GameContext p_gameContext)
	{
		Destroy();
		Instance = new ServiceLocator(p_gameContext);
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
		WorldService.Shutdown();
		PlayerService.Shutdown();
	}
}
