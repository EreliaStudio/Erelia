using System.Collections.Generic;
using UnityEngine;

namespace Core.Creature.Model
{
	[CreateAssetMenu(menuName = "Creature/Team", fileName = "CreatureTeam")]
	public class Team : ScriptableObject
	{
		public const int MaxSize = 6;

		[SerializeField] private List<Definition> members = new List<Definition>();

		public IReadOnlyList<Definition> Members => members;
		public int Count => members.Count;

		public Definition GetAt(int index)
		{
			if (index < 0 || index >= members.Count)
			{
				return null;
			}

			return members[index];
		}
	}
}
