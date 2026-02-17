using System.Collections.Generic;
using UnityEngine;

namespace Core.Creature.Model
{
	[CreateAssetMenu(menuName = "Creature/Team", fileName = "CreatureTeam")]
	public class Team : ScriptableObject
	{
		public const int MaxSize = 6;

		[SerializeField] private List<Core.Creature.Definition> members = new List<Core.Creature.Definition>();

		public IReadOnlyList<Core.Creature.Definition> Members => members;
		public int Count => members.Count;

		public Core.Creature.Definition GetAt(int index)
		{
			if (index < 0 || index >= members.Count)
			{
				return null;
			}

			return members[index];
		}
	}
}
