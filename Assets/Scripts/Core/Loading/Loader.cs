using UnityEngine;

namespace Erelia.Core.Loading
{
	public sealed class Loader : MonoBehaviour
	{
		private void Awake()
		{
			InitializeContext();

			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.ExplorationSceneDataRequest());
		}

		public void InitializeContext()
		{
			var context = Erelia.Core.Context.Instance;
			context.ClearBattle();
		}
	}
}
