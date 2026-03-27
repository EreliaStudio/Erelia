using UnityEngine;

namespace Erelia.Exploration
{
	public class Loader : MonoBehaviour
	{
		[SerializeField] World.Presenter worldPresenter;

		[SerializeField] Player.Presenter playerPresenter;

		private void Awake()
		{
			if (worldPresenter == null)
			{
				Debug.LogWarning("[Erelia.Exploration.Loader] World presenter is not assigned.");
			}
			if (playerPresenter == null)
			{
				Debug.LogWarning("[Erelia.Exploration.Loader] Player presenter is not assigned.");
			}

			BindFromContext();
		}

		private void BindFromContext()
		{
			var context = Erelia.Core.GameContext.Instance;

			Erelia.Exploration.ExplorationState exploration = context.Exploration;
			if (exploration == null || exploration.World == null || exploration.Player == null)
			{
				Debug.LogWarning("[Erelia.Exploration.Loader] Exploration data is missing.");
				return;
			}

			worldPresenter.SetWorld(exploration.World);
			playerPresenter.SetPlayer(exploration.Player);

			if (!exploration.HasSafePosition && exploration.Player.HasWorldPosition)
			{
				Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.SetSafePosition(exploration.Player.WorldPosition));
			}
		}
	}
}




