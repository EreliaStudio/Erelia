using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeEncounterRule
{
	[Range(0f, 1f)] public float BaseChancePerStep = 0.1f;
	public List<BoardConfiguration> BoardConfigurations = new List<BoardConfiguration>();
	public EncounterTable EncounterTable;

	public bool TryPickBoardConfiguration(out BoardConfiguration boardConfiguration, System.Random random = null)
	{
		boardConfiguration = null;

		if (BoardConfigurations == null || BoardConfigurations.Count == 0)
		{
			return false;
		}

		List<BoardConfiguration> validConfigurations = new List<BoardConfiguration>();
		for (int index = 0; index < BoardConfigurations.Count; index++)
		{
			BoardConfiguration candidate = BoardConfigurations[index];
			if (candidate != null)
			{
				validConfigurations.Add(candidate);
			}
		}

		if (validConfigurations.Count == 0)
		{
			return false;
		}

		int selectedIndex = (random != null) ?
			random.Next(0, validConfigurations.Count) :
			UnityEngine.Random.Range(0, validConfigurations.Count);

		boardConfiguration = validConfigurations[selectedIndex];
		return true;
	}
}
