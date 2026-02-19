using UnityEngine.SceneManagement;

namespace Utils
{
	public class SceneLoader
	{
		private const string ExplorationSceneName = "ExplorationView";
		private const string BattleSceneName = "BattleScene";

		public void LoadExplorationScene()
		{
			SceneManager.LoadScene(ExplorationSceneName);
		}
		
		public void LoadBattleScene()
		{
			SceneManager.LoadScene(BattleSceneName);
		}
	}
}
