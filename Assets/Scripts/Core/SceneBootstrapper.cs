using Erelia.Core.Event;
using UnityEngine;

namespace Erelia.Core
{
	/// <summary>
	/// Central entry point responsible for registering scene-loading handlers at application startup.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This type is initialized automatically via <see cref="RuntimeInitializeOnLoadMethodAttribute"/> before the first scene loads.
	/// It registers event-bus handlers so that gameplay systems can request a scene transition by emitting:
	/// </para>
	/// <list type="bullet">
	/// <item><description><see cref="Erelia.Core.Event.ExplorationSceneDataRequest"/> to load the exploration scene.</description></item>
	/// <item><description><see cref="Erelia.Core.Event.BattleSceneDataRequest"/> to load the battle scene.</description></item>
	/// </list>
	/// <para>
	/// Scene loading is performed with <see cref="UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(string, UnityEngine.SceneManagement.LoadSceneMode)"/>
	/// using <see cref="UnityEngine.SceneManagement.LoadSceneMode.Single"/>.
	/// </para>
	/// <para>
	/// The scenes must be included in Build Settings for runtime loading.
	/// </para>
	/// </remarks>
	public sealed class SceneBootstrapper
	{
		/// <summary>
		/// Singleton instance used to ensure event subscriptions are registered only once.
		/// </summary>
		private static SceneBootstrapper instance;

		/// <summary>
		/// Asset path to the exploration scene.
		/// </summary>
		public const string ExplorationScenePath = "Assets/Scene/ExplorationScene.unity";

		/// <summary>
		/// Asset path to the battle scene.
		/// </summary>
		public const string BattleScenePath = "Assets/Scene/BattleScene.unity";

		/// <summary>
		/// Unity runtime initialization hook executed before the first scene load.
		/// </summary>
		/// <remarks>
		/// Ensures the bootstrapper is created exactly once so it can subscribe to the event bus.
		/// </remarks>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			// Prevent duplicate initialization in case the method is invoked more than once.
			if (instance != null)
			{
				return;
			}

			// Constructing the instance triggers event subscriptions.
			instance = new SceneBootstrapper();
		}

		/// <summary>
		/// Initializes a new bootstrapper and registers scene-loading handlers.
		/// </summary>
		/// <remarks>
		/// The constructor subscribes to scene request events and maps them to a call to <see cref="LoadScene"/>.
		/// </remarks>
		private SceneBootstrapper()
		{
			// Subscribe to exploration scene requests.
			Event.Bus.Subscribe<Erelia.Core.Event.ExplorationSceneDataRequest>(_ => LoadScene(SceneKind.Exploration));

			// Subscribe to battle scene requests.
			Event.Bus.Subscribe<Erelia.Core.Event.BattleSceneDataRequest>(evt =>
			{
				Context.Instance.SetBattle(evt.EnemyTeam, evt.BattleBoard);

				LoadScene(SceneKind.Battle);
			});
		}

		/// <summary>
		/// Loads the scene associated with the provided <see cref="SceneKind"/>.
		/// </summary>
		/// <param name="scene">The scene kind to load.</param>
		/// <returns>
		/// The Unity <see cref="AsyncOperation"/> representing the asynchronous scene load.
		/// </returns>
		/// <remarks>
		/// Scenes are loaded in <see cref="UnityEngine.SceneManagement.LoadSceneMode.Single"/> mode, replacing the current scene.
		/// </remarks>
		public static AsyncOperation LoadScene(SceneKind scene)
		{
			// Resolve the asset path and start the async load.
			string path = GetScenePath(scene);
			return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
				path,
				UnityEngine.SceneManagement.LoadSceneMode.Single);
		}

		/// <summary>
		/// Resolves the asset path for a given <see cref="SceneKind"/>.
		/// </summary>
		/// <param name="scene">Scene kind to resolve.</param>
		/// <returns>The corresponding scene asset path.</returns>
		/// <remarks>
		/// If an unknown value is provided, exploration is returned as a safe default.
		/// </remarks>
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

	/// <summary>
	/// Identifies the supported scenes that can be loaded through <see cref="SceneBootstrapper"/>.
	/// </summary>
	public enum SceneKind
	{
		/// <summary>
		/// Main exploration scene.
		/// </summary>
		Exploration,

		/// <summary>
		/// Battle scene.
		/// </summary>
		Battle
	}
}
