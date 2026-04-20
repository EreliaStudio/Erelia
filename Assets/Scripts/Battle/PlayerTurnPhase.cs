using System;

public sealed class PlayerTurnPhase : BattlePhase
{
	private readonly BattlePlayerController playerController;

	public PlayerTurnPhase(BattleContext p_context, BattlePlayerController p_playerController) : base(p_context)
	{
		playerController = p_playerController ?? throw new ArgumentNullException(nameof(p_playerController));
	}

	public override BattlePhaseId PhaseId => BattlePhaseId.PlayerTurn;

	public event Action<BattleAction> ActionChosen;

	public override void Enter()
	{
		Context.RefillTurnResources(Context.ActiveUnit);
		playerController.BindTurn(Context, Context.ActiveUnit, HandleActionChosen);
	}

	public override void Exit()
	{
		playerController.UnbindTurn();
	}

	private void HandleActionChosen(BattleAction p_action)
	{
		ActionChosen?.Invoke(p_action);
	}
}
