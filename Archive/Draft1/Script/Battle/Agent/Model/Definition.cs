using UnityEngine;

namespace Battle.Agent.Model
{
	[CreateAssetMenu(menuName = "Battle/Agent/Definition", fileName = "AgentDefinition")]
	public class Definition : ScriptableObject
	{
		[SerializeField] private Core.Creature.Definition creature = null;
		public Core.Creature.Definition Creature => creature;
	}
}
