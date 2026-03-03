using UnityEngine;

namespace Erelia.Loading
{
	/// <summary>
	/// Scene-level bootstrapper responsible for initializing the game context when the loading scene starts.
	/// </summary>
	/// <remarks>
	/// On <see cref="Awake"/>, this component:
	/// <list type="number">
	/// <item><description>Initializes/clears relevant parts of the global <c>Context</c>.</description></item>
	/// <item><description>Emits an <c>ExplorationSceneDataRequest</c> event to trigger loading of exploration scene data.</description></item>
	/// </list>
	/// </remarks>
	public sealed class Loader : MonoBehaviour
	{
		/// <summary>
		/// Unity callback invoked when the component is loaded.
		/// </summary>
		/// <remarks>
		/// Initializes the global context, then asks the event bus to provide the data required
		/// to enter the exploration scene.
		/// </remarks>
		private void Awake()
		{
			// Prepare/reset runtime state before requesting scene data.
			InitializeContext();

			// Request exploration scene data (listeners on the bus should respond to this event).
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.ExplorationSceneDataRequest());
		}

		/// <summary>
		/// Initializes the global game context for the loading flow.
		/// </summary>
		/// <remarks>
		/// Current behavior clears any battle-related state to ensure the exploration scene starts from a clean state.
		/// </remarks>
		public void InitializeContext()
		{
			// Get the singleton context and reset battle state.
			var context = Erelia.Core.Context.Instance;
			context.ClearBattle();
		}
	}
}