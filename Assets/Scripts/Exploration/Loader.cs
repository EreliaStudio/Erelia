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
			var context = Erelia.Context.Instance;
			
			worldPresenter.SetModel(context.WorldModel);
			playerPresenter.SetModel(context.PlayerModel);
		}
	}
}
