using UnityEngine;

namespace Erelia.Encounter
{
	[System.Serializable]
	public sealed class EncounterTable
	{
		[Range(0f, 1f)]
		public float EncounterChance;
	}
}
