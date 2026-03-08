using UnityEngine.Scripting.APIUpdating;

namespace Erelia.Battle.Phase
{
	/// <summary>
	/// Identifiers for the battle phase state machine.
	/// Used by the manager to select and transition between phases.
	/// </summary>
	[MovedFrom(true, sourceNamespace: "Erelia.Battle", sourceAssembly: "Assembly-CSharp", sourceClassName: "BattlePhaseId")]
	public enum Id
	{
		None = 0,
		Initialize = 1,
		Placement = 2,
		PlayerTurn = 3,
		EnemyTurn = 4,
		ResolveAction = 5,
		Victory = 6,
		Defeat = 7,
		Cleanup = 8
	}
}
