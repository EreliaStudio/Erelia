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

	}
}
