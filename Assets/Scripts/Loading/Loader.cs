using UnityEngine;

namespace Erelia.Loading
{
	public sealed class Loader : MonoBehaviour
	{
		[SerializeField] private TextAsset playerTeamJson;

		[SerializeField] private string playerTeamResourcePath = "Creature/Team/PlayerTeam";

		private void Awake()
		{
			InitializeContext();

			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.ExplorationSceneDataRequest());
		}

		public void InitializeContext()
		{
			var context = Erelia.Core.Context.Instance;
			EnsurePlayerTeam(context.SystemData);
		}

		private void EnsurePlayerTeam(Erelia.Core.SystemData systemData)
		{
			if (systemData == null || systemData.PlayerTeam != null)
			{
				return;
			}

			Erelia.Core.Creature.Team team = null;
			if (playerTeamJson != null && !string.IsNullOrEmpty(playerTeamJson.text))
			{
				team = JsonUtility.FromJson<Erelia.Core.Creature.Team>(playerTeamJson.text);
			}
			else if (!string.IsNullOrEmpty(playerTeamResourcePath))
			{
				team = Erelia.Core.Utils.JsonIO.Load<Erelia.Core.Creature.Team>(playerTeamResourcePath);
			}

			if (team == null)
			{
				Debug.LogWarning("[Erelia.Loading.Loader] Player team JSON is missing or invalid.");
				return;
			}

			team.NormalizeSlots();
			systemData.SetPlayerTeam(team);
		}
	}
}
