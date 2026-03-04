using UnityEngine;

namespace Erelia.Exploration
{
	/// <summary>
	/// Scene-level loader that binds exploration models to their presenters.
	/// On Awake, pulls exploration data from context and assigns world/player models to presenters.
	/// </summary>
	public class Loader : MonoBehaviour
	{
		/// <summary>
		/// World presenter responsible for chunk visuals.
		/// </summary>
		[SerializeField] World.Presenter worldPresenter;

		/// <summary>
		/// Player presenter responsible for player movement and view.
		/// </summary>
		[SerializeField] Player.Presenter playerPresenter;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Warn if required references are missing.
			if (worldPresenter == null)
			{
				Debug.LogWarning("[Erelia.Exploration.Loader] World presenter is not assigned.");
			}
			if (playerPresenter == null)
			{
				Debug.LogWarning("[Erelia.Exploration.Loader] Player presenter is not assigned.");
			}

			// Bind current exploration data to presenters.
			BindFromContext();
		}

		/// <summary>
		/// Pulls exploration data from the global context and applies it to presenters.
		/// </summary>
		private void BindFromContext()
		{
			// Resolve global context.
			var context = Erelia.Core.Context.Instance;

			// Validate available exploration data.
			Erelia.Exploration.Data data = context.ExplorationData;
			if (data == null || data.WorldModel == null || data.PlayerModel == null)
			{
				Debug.LogWarning("[Erelia.Exploration.Loader] Exploration data is missing.");
				return;
			}

			// Apply models to presenters.
			worldPresenter.SetModel(data.WorldModel);
			playerPresenter.SetModel(data.PlayerModel);
		}
	}
}
