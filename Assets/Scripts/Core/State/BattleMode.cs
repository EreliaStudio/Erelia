using UnityEngine;

public sealed class BattleMode : Mode
{
	[SerializeField] private BoardPresenter boardPresenter;

	private BattleSetup currentSetup;

	public override ModeKind Kind => ModeKind.Battle;
	public BattleSetup CurrentSetup => currentSetup;

	private void Awake()
	{
		if (boardPresenter == null)
		{
			Logger.LogError("[BattleMode] BoardPresenter is not assigned in the inspector. Please assign a BoardPresenter to the BattleMode component.", Logger.Severity.Critical, this);
		}
	}

	protected override void OnEnter(ModeContext context)
	{
		currentSetup = context?.BattleSetup;
		if (currentSetup == null || currentSetup.Board == null)
		{
			LogDebug("Entered battle mode without a valid board.");
			return;
		}

		if (boardPresenter != null)
		{
			boardPresenter.Assign(currentSetup.Board);
		}
		LogDebug($"Battle setup assigned. EnemyTeamSize={CountUnits(currentSetup.Team)}.");
	}

	protected override void OnExit(ModeContext context)
	{
		currentSetup = null;
	}

	private static int CountUnits(EncounterUnit[] team)
	{
		if (team == null)
		{
			return 0;
		}

		int count = 0;
		for (int index = 0; index < team.Length; index++)
		{
			if (team[index] != null)
			{
				count++;
			}
		}

		return count;
	}
}
