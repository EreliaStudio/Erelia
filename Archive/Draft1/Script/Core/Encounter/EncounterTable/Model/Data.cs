using UnityEngine;
using System.Collections.Generic;

namespace Core.Encounter.Table.Model
{
	[CreateAssetMenu(menuName = "Battle/Encounter Table", fileName = "NewEncounterTable")]
	public class Data : ScriptableObject
	{
		[SerializeField, Range(0f, 1f)] private float fightChance = 0.1f;
		public float FightChance => fightChance;
		
		[SerializeField] private Vector2Int boardArea = new Vector2Int(25, 25);
		public Vector2Int BoardArea => boardArea;

		[SerializeField, Range(0, Core.Creature.Model.Team.MaxSize)] private int maxCreaturesToPlace = Core.Creature.Model.Team.MaxSize;
		public int MaxCreaturesToPlace => maxCreaturesToPlace;

		[SerializeField] private List<Battle.Agent.Model.AgentTeam> agentTeams = new List<Battle.Agent.Model.AgentTeam>();
		public IReadOnlyList<Battle.Agent.Model.AgentTeam> AgentTeams => agentTeams;
	}
}
