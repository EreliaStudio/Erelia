namespace Erelia.Battle
{
	[System.Serializable]
	public abstract class BattlePhase
	{
		public abstract BattlePhaseId Id { get; }

		public virtual void Enter(BattleManager manager)
		{
		}

		public virtual void Exit(BattleManager manager)
		{
		}

		public virtual void Tick(BattleManager manager, float deltaTime)
		{
		}
	}
}
