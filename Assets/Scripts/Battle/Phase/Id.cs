

namespace Erelia.Battle.Phase
{
	/// <summary>
	/// Identifiers for the battle phase state machine.
	/// Used by the Orchestrator to select and transition between phases.
	/// </summary>
	public enum Id
	{
		None = 0,
		Initialize = 1,
		Placement = 2,
		Timeline = 3,
		PlayerTurn = 4,
		EnemyTurn = 5,
		ResolveAction = 6,
		Victory = 7,
		Defeat = 8,
		Cleanup = 9
	}
}
