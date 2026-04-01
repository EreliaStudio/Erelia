using System.Collections.Generic;
using UnityEngine;

public class CreatureCardListElementUI : MonoBehaviour
{
	[SerializeField] private CreatureCardElementUI creatureCardPrefab;
	[SerializeField] private int creatureCardCount = 6;

	private readonly List<CreatureCardElementUI> spawnedCreatureCardElements = new List<CreatureCardElementUI>();

	private void Awake()
	{
		EnsurePoolSize(creatureCardCount);
		Clear();
	}

	public void Bind(CreatureUnit[] p_team)
	{
		EnsurePoolSize(creatureCardCount);

		for (int index = 0; index < spawnedCreatureCardElements.Count; index++)
		{
			CreatureUnit creatureUnit = null;

			if (p_team != null && index < p_team.Length)
			{
				creatureUnit = p_team[index];
			}

			spawnedCreatureCardElements[index].Bind(creatureUnit);
		}
	}

	public void Clear()
	{
		for (int index = 0; index < spawnedCreatureCardElements.Count; index++)
		{
			spawnedCreatureCardElements[index].ClearBinding();
		}
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (spawnedCreatureCardElements.Count < p_targetCount)
		{
			CreatureCardElementUI creatureCardElementUI = Instantiate(creatureCardPrefab, transform, false);
			creatureCardElementUI.gameObject.SetActive(true);
			spawnedCreatureCardElements.Add(creatureCardElementUI);
		}
	}
}