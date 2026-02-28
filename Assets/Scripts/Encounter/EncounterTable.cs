using UnityEngine;

namespace Erelia.Encounter
{
	[System.Serializable]
	public sealed class EncounterTable
	{
		[Range(0f, 1f)]
		public float EncounterChance;
		public int BaseRadius = 10;
		public int NoiseAmplitude = 4;
		public float NoiseScale = 0.15f;
		public int NoiseSeed = 1337;
		[Min(0)]
		public int PlacementRadius = 3;
	}
}
