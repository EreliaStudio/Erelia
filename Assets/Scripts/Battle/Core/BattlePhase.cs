namespace Erelia.Battle
{
	/// <summary>
	/// Base class for battle phases with enter/exit/tick hooks.
	/// Implementations provide an Id and optional behavior during the battle flow.
	/// </summary>
	[System.Serializable]
	public abstract class BattlePhase
	{
		/// <summary>
		/// Gets the phase identifier.
		/// </summary>
		public abstract BattlePhaseId Id { get; }

		/// <summary>
		/// Called when the phase becomes active.
		/// </summary>
		public virtual void Enter(BattleManager manager)
		{
			// Default implementation is a no-op.
		}

		/// <summary>
		/// Called when the phase is exited.
		/// </summary>
		public virtual void Exit(BattleManager manager)
		{
			// Default implementation is a no-op.
		}

		/// <summary>
		/// Called every frame while the phase is active.
		/// </summary>
		public virtual void Tick(BattleManager manager, float deltaTime)
		{
			// Default implementation is a no-op.
		}
	}
}
