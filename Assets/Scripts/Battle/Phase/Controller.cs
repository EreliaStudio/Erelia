using UnityEngine.Scripting.APIUpdating;

namespace Erelia.Battle.Phase
{
	/// <summary>
	/// Base class for handling confirm/cancel input per battle phase.
	/// </summary>
	[System.Serializable]
	[MovedFrom(true, sourceNamespace: "Erelia.Battle", sourceAssembly: "Assembly-CSharp", sourceClassName: "PhaseController")]
	public abstract class Controller
	{
		/// <summary>
		/// Called when the player confirms an action.
		/// </summary>
		public virtual void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			// Default implementation does nothing.
		}

		/// <summary>
		/// Called when the player cancels an action.
		/// </summary>
		public virtual void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
			// Default implementation does nothing.
		}
	}
}
