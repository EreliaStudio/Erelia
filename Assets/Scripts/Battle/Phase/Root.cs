using UnityEngine.Scripting.APIUpdating;

namespace Erelia.Battle.Phase
{
	/// <summary>
	/// Base class for battle phases with enter/exit/tick hooks and optional input handling.
	/// Implementations provide an Id and behavior during the battle flow.
	/// </summary>
	[System.Serializable]
	[MovedFrom(true, sourceNamespace: "Erelia.Battle", sourceAssembly: "Assembly-CSharp", sourceClassName: "BattlePhase")]
	public abstract class Root : Controller
	{
		/// <summary>
		/// Gets the phase identifier.
		/// </summary>
		public abstract Id Id { get; }

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
