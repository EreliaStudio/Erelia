using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Agent.Model
{
	[CreateAssetMenu(menuName = "Battle/Agent/Team", fileName = "AgentTeam")]
	public class AgentTeam : ScriptableObject
	{
		[Serializable]
		public class Entry : Data
		{
			[SerializeField] private Definition agent = null;
			public Definition Agent => agent;
		}

		public const int MaxSize = 6;

		[SerializeField] private List<Entry> entries = new List<Entry>();
		public IReadOnlyList<Entry> Entries => entries;
		public int Count => entries.Count;

		public Entry GetAt(int index)
		{
			if (index < 0 || index >= entries.Count)
			{
				return null;
			}

			return entries[index];
		}

		private void OnValidate()
		{
			if (entries.Count > MaxSize)
			{
				entries.RemoveRange(MaxSize, entries.Count - MaxSize);
			}
		}
	}
}
