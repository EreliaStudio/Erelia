using UnityEngine;

[DisallowMultipleComponent]
public class ModeManager : MonoBehaviour
{
	[SerializeField] private ExplorationMode explorationMode;
	[SerializeField] private BattleMode battleMode;
	[SerializeField] private bool debugLogging;

	private Mode currentMode;
	private GameContext currentGameContext;

	public Mode CurrentMode => currentMode;
	public GameContext CurrentGameContext => currentGameContext;

	private void Reset()
	{
		if (explorationMode == null)
		{
			explorationMode = FindFirstObjectByType<ExplorationMode>(FindObjectsInactive.Include);
		}

		if (battleMode == null)
		{
			battleMode = FindFirstObjectByType<BattleMode>(FindObjectsInactive.Include);
		}
	}

	private void Awake()
	{
		if (explorationMode == null)
		{
			explorationMode = FindFirstObjectByType<ExplorationMode>(FindObjectsInactive.Include);
		}

		if (battleMode == null)
		{
			battleMode = FindFirstObjectByType<BattleMode>(FindObjectsInactive.Include);
		}

		if (explorationMode != null)
		{
			explorationMode.Exit();
		}

		if (battleMode != null)
		{
			battleMode.Exit();
		}
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

		SwitchTo(explorationMode, new ModeContext
		{
			GameContext = currentGameContext
		});
	}

	public void EnterBattleMode(BattleSetup setup)
	{
		if (setup == null || setup.Board == null)
		{
			LogDebug("Battle start ignored because the setup or board is missing.");
			return;
		}

		SwitchTo(battleMode, new ModeContext
		{
			BattleSetup = setup
		});
	}

	public void EndBattle()
	{
		EnterExplorationMode(currentGameContext);
	}

	private void OnBattleStartRequested(BattleSetup setup)
	{
		EnterBattleMode(setup);
	}

	private void SwitchTo(Mode nextMode, ModeContext context)
	{
		if (nextMode == null)
		{
			LogDebug("Requested mode switch to a null mode.");
			return;
		}

		if (currentMode == nextMode)
		{
			nextMode.Enter(context);
			return;
		}

		currentMode?.Exit(context);
		currentMode = nextMode;
		currentMode.Enter(context);
		LogDebug($"Current mode is now {currentMode.Kind}.");
	}

	private void LogDebug(string message)
	{
		if (!debugLogging)
		{
			return;
		}

		Debug.Log($"[ModeManager] {message}", this);
	}
}
