using UnityEngine;

namespace Battle.Context
{
	public class Service
	{
		private Model.TeamPlacement playerPlacement;
		public Model.TeamPlacement PlayerPlacement => playerPlacement;

		public void InitializeFromPlayerTeam(Core.Creature.Model.Team team)
		{
			int maxPlacements = team != null ? team.Count : 0;

			var encounterService = Utils.ServiceLocator.Instance.EncounterService;
			var playerData = Utils.ServiceLocator.Instance.PlayerService.PlayerData;
			if (encounterService != null && playerData != null)
			{
				var encounterTable = encounterService.GetEncounterTable(playerData.ChunkCoordinates);
				if (encounterTable != null)
				{
					maxPlacements = Mathf.Clamp(encounterTable.MaxCreaturesToPlace, 0, maxPlacements);
				}
			}

			playerPlacement = new Model.TeamPlacement(team, Model.Side.Player, maxPlacements);
		}
	}
}
