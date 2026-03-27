using UnityEngine;

namespace Erelia.Core.Encounter
{
	[System.Serializable]
	public sealed class EncounterTable
	{
		[System.Serializable]
		public struct TeamEntry
		{
			public string TeamPath;

			[Min(1)]
			public int Weight;
		}

		[Range(0f, 1f)]
		public float EncounterChance;

		public int BaseRadius = 10;

		public int NoiseAmplitude = 4;

		public float NoiseScale = 0.15f;

		public int NoiseSeed = 1337;

		public TeamEntry[] Teams;

		public bool TryLoadRandomTeam(out Erelia.Core.Creature.Team team)
		{
			return TryLoadRandomTeam(this, out team);
		}

		public static bool TryLoadRandomTeam(EncounterTable table, out Erelia.Core.Creature.Team team)
		{
			team = null;

			TeamEntry[] entries = table?.Teams;
			if (entries == null || entries.Length == 0)
			{
				return false;
			}

			int selectedIndex = SelectTeamIndex(entries);
			if (TryLoadTeam(entries, selectedIndex, out team))
			{
				return true;
			}

			for (int i = 0; i < entries.Length; i++)
			{
				if (i == selectedIndex)
				{
					continue;
				}

				if (TryLoadTeam(entries, i, out team))
				{
					return true;
				}
			}

			return false;
		}

		private static int SelectTeamIndex(TeamEntry[] entries)
		{
			int totalWeight = 0;
			for (int i = 0; i < entries.Length; i++)
			{
				totalWeight += Mathf.Max(0, entries[i].Weight);
			}

			if (totalWeight <= 0)
			{
				return 0;
			}

			int roll = Random.Range(0, totalWeight);
			int cumulativeWeight = 0;
			for (int i = 0; i < entries.Length; i++)
			{
				cumulativeWeight += Mathf.Max(0, entries[i].Weight);
				if (roll < cumulativeWeight)
				{
					return i;
				}
			}

			return entries.Length - 1;
		}

		private static bool TryLoadTeam(TeamEntry[] entries, int index, out Erelia.Core.Creature.Team team)
		{
			team = null;

			if (entries == null || index < 0 || index >= entries.Length)
			{
				return false;
			}

			string teamPath = entries[index].TeamPath;
			if (string.IsNullOrEmpty(teamPath))
			{
				return false;
			}

			if (Erelia.Core.Utils.JsonIO.TryLoad(teamPath, out team))
			{
				team.NormalizeSlots();
				return true;
			}

			Debug.LogWarning($"[EncounterTable] Failed to load encounter team at '{teamPath}'.");
			return false;
		}
	}
}
