using UnityEngine;

namespace Erelia.Loading
{
	public sealed class Loader : MonoBehaviour
	{
		private void Awake()
		{
			InitializeContext();

			Erelia.Event.Bus.Emit(new Erelia.Event.ExplorationSceneDataRequest());
		}

		public void InitializeContext()
		{
			var context = Erelia.Context.Instance;
			context.ClearBattle();
		}
	}
}
