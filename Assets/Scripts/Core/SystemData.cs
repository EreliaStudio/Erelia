using System.IO;
using UnityEngine;

namespace Erelia.Core
{
	[System.Serializable]
	public sealed class SystemData
	{
		[SerializeField] private Erelia.Core.Creature.Team playerTeam;

		public Erelia.Core.Creature.Team PlayerTeam => playerTeam;

		public void SetPlayerTeam(Erelia.Core.Creature.Team team)
		{
			playerTeam = team;
		}

		public void Save(string path)
		{
			string json = JsonUtility.ToJson(this, true);
			File.WriteAllText(path, json);
		}

		public bool Load(string path)
		{
			string json = Erelia.Core.Utils.PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				Debug.LogWarning($"[Erelia.Core.SystemData] Save file not found at '{path}'.");
				return false;
			}

			SystemData data = JsonUtility.FromJson<SystemData>(json);
			if (data == null)
			{
				Debug.LogWarning("[Erelia.Core.SystemData] Failed to parse save data.");
				return false;
			}

			playerTeam = data.playerTeam;
			return true;
		}
	}
}
