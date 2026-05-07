public sealed class SaveService
{
	private readonly GameContext gameContext;

	private GameSaveData boundSaveData;

	public SaveService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public void Initialize()
	{
		EventCenter.SaveRequested += OnSaveRequested;
	}

	public void Shutdown()
	{
		EventCenter.SaveRequested -= OnSaveRequested;
		boundSaveData = null;
	}

	public void BindSaveData(GameSaveData p_saveData)
	{
		boundSaveData = p_saveData;
	}

	public bool Save(GameSaveData p_targetSaveData = null)
	{
		GameSaveData target = p_targetSaveData ?? boundSaveData;
		if (target == null || gameContext == null)
		{
			EventCenter.EmitSaveCompleted(target, false);
			return false;
		}

		target.CopyPlayerFrom(gameContext.Player);
		EventCenter.EmitSaveCompleted(target, true);
		return true;
	}

	private void OnSaveRequested(GameSaveData p_targetSaveData)
	{
		Save(p_targetSaveData);
	}
}
