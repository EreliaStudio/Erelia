using Erelia.Core.Event;
using UnityEngine;

namespace Erelia.Core
{
	public sealed class SceneBootstrapper
	{
		private static SceneBootstrapper instance;

		public const string ExplorationScenePath = "Assets/Scene/ExplorationScene.unity";

		public const string BattleScenePath = "Assets/Scene/BattleScene.unity";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			if (instance != null)
			{
				return;
			}

			instance = new SceneBootstrapper();
		}

		private SceneBootstrapper()
		{
			Event.Bus.Subscribe<Erelia.Core.Event.ExplorationSceneDataRequest>(_ => LoadScene(SceneKind.Exploration));

			Event.Bus.Subscribe<Erelia.Core.Event.SetSafePosition>(evt =>
			{
				GameContext.Instance.Exploration?.SetSafePosition(evt.WorldPosition);
			});

			Event.Bus.Subscribe<Erelia.Core.Event.BattleSceneDataRequest>(evt =>
			{
				GameContext.Instance.SetBattle(evt.EnemyTeam, evt.BattleBoard);

				LoadScene(SceneKind.Battle);
			});
		}

		public static AsyncOperation LoadScene(SceneKind scene)
		{
			string path = GetScenePath(scene);
			return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
				path,
				UnityEngine.SceneManagement.LoadSceneMode.Single);
		}

		private static string GetScenePath(SceneKind scene)
		{
			switch (scene)
			{
				case SceneKind.Exploration:
					return ExplorationScenePath;

				case SceneKind.Battle:
					return BattleScenePath;

				default:
					return ExplorationScenePath;
			}
		}
	}

	public enum SceneKind
	{
		Exploration,

		Battle
	}
}
