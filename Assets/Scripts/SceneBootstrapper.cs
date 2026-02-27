using Erelia.Event;
using UnityEngine;

namespace Erelia
{
	public sealed class SceneBootstrapper
	{
		private static SceneBootstrapper instance;
		public const string ExplorationScenePath = "Assets/Scene/ExplorationScene.unity";
		public const string BattleScenePath = "Assets/Scene/BattleScene.unity";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
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
			Event.Bus.Subscribe<Erelia.Event.EncounterTriggerEvent>(handleEncounterTriggerEvent);
		}

		private void handleEncounterTriggerEvent(Erelia.Event.EncounterTriggerEvent encounterEvent)
		{
			LoadScene(SceneKind.Battle);
		}

		public static AsyncOperation LoadScene(SceneKind scene)
		{
			string path = GetScenePath(scene);
			return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(path, UnityEngine.SceneManagement.LoadSceneMode.Single);
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
