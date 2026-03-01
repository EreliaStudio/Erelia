using UnityEngine;

namespace Erelia.Encounter
{
	[System.Serializable]
	public sealed class EncounterTable
	{
		[System.Serializable]
		public struct TeamEntry
		{
			public Creature.Team Team;
			[Min(0)]
			public int Weight;
		}

		[Range(0f, 1f)]
		public float EncounterChance;
		public int BaseRadius = 10;
		public int NoiseAmplitude = 4;
		public float NoiseScale = 0.15f;
		public int NoiseSeed = 1337;
		[Min(0)]
		public int PlacementRadius = 3;
		public TeamEntry[] Teams;
	}
}
