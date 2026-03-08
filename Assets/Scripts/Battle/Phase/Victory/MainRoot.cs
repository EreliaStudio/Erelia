using UnityEngine.Scripting.APIUpdating;
using PhaseId = Erelia.Battle.Phase.Id;
using PhaseRoot = Erelia.Battle.Phase.Root;

namespace Erelia.Battle.Phase.Victory
{
	[System.Serializable]
	[MovedFrom(true, sourceNamespace: "Erelia.Battle", sourceAssembly: "Assembly-CSharp", sourceClassName: "VictoryPhase")]
	public sealed class MainRoot : PhaseRoot
	{
		public override PhaseId Id => PhaseId.Victory;
	}
}
