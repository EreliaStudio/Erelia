using UnityEngine.Scripting.APIUpdating;
using PhaseId = Erelia.Battle.Phase.Id;
using PhaseRoot = Erelia.Battle.Phase.Root;

namespace Erelia.Battle.Phase.EnemyTurn
{
	[System.Serializable]
	[MovedFrom(true, sourceNamespace: "Erelia.Battle", sourceAssembly: "Assembly-CSharp", sourceClassName: "EnemyTurnPhase")]
	public sealed class MainRoot : PhaseRoot
	{
		public override PhaseId Id => PhaseId.EnemyTurn;
	}
}
