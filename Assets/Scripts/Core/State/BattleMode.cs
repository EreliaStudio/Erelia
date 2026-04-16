using UnityEngine;

public sealed class BattleMode : Mode
{
	[SerializeField] private BoardPresenter boardPresenter;

	private BattleSetup currentSetup;

	public override ModeKind Kind => ModeKind.Battle;
	public BattleSetup CurrentSetup => currentSetup;

	protected override void Reset()
	{
		base.Reset();
		if (boardPresenter == null)
		{
			boardPresenter = GetComponentInChildren<BoardPresenter>(true);
		}
	}

	protected override void OnEnter(ModeContext context)
	{
		currentSetup = context?.BattleSetup;
		if (boardPresenter == null)
		{
			boardPresenter = GetComponentInChildren<BoardPresenter>(true);
		}

		if (currentSetup?.Board == null)
		{
			LogDebug("Entered battle mode without a valid board.");
			return;
		}

		boardPresenter?.Assign(currentSetup.Board);
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
