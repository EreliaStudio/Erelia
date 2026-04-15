using System;
using UnityEngine;

[Serializable]
public class BiomeEncounterRule
{
	[Range(0f, 1f)] public float BaseChancePerStep = 0.1f;
	public EncounterTable EncounterTable;
}
