using UnityEngine;

[DisallowMultipleComponent]
public class ModeManager : MonoBehaviour
{
	[SerializeField] private ExplorationMode explorationMode;
	[SerializeField] private BattleMode battleMode;

	private Mode currentMode;
	private GameContext currentGameContext;

	public Mode CurrentMode => currentMode;
	public GameContext CurrentGameContext => currentGameContext;

	private void Awake()
	{
		if (explorationMode == null)
		{
			Logger.LogError("[ModeManager] ExplorationMode is not assigned in the inspector. Please assign an ExplorationMode to the ModeManager component.", Logger.Severity.Critical, this);
		}

		if (battleMode == null)
		{
			Logger.LogError("[ModeManager] BattleMode is not assigned in the inspector. Please assign a BattleMode to the ModeManager component.", Logger.Severity.Critical, this);
		}

		if (explorationMode != null)
		{
			explorationMode.gameObject.SetActive(false);
		}

		if (battleMode != null)
		{
			battleMode.gameObject.SetActive(false);
		}

		EnterExplorationMode();
	}

	private void OnEnable()
	{
		EventCenter.BattleStartRequested += OnBattleStartRequested;
	}

	private void OnDisable()
	{
		EventCenter.BattleStartRequested -= OnBattleStartRequested;
	}

	public void SetGameContext(GameContext gameContext)
	{
		currentGameContext = gameContext;
	}

	public void EnterExplorationMode(GameContext gameContext = null)
	{
		if (gameContext != null)
		{
			currentGameContext = gameContext;
		}

		if (currentGameContext == null || explorationMode == null)
		{
			return;
		}

		SwitchTo(explorationMode);
		explorationMode.Enter(currentGameContext);
	}

	public void EnterBattleMode(BattleSetup setup)
	{
		if (setup == null || setup.Board == null || battleMode == null)
		{
			return;
		}

		SwitchTo(battleMode);
		battleMode.Enter(setup);
	}

	public void EndBattle()
	{
		EnterExplorationMode(currentGameContext);
	}

	private void OnBattleStartRequested(BattleSetup setup)
	{
		EnterBattleMode(setup);
	}

	private void SwitchTo(Mode nextMode)
	{
		if (nextMode == null)
		{
			return;
		}

		if (currentMode == nextMode)
		{
			return;
		}

		if (currentMode != null)
		{
			currentMode.Exit();
		}

		currentMode = nextMode;
	}
}