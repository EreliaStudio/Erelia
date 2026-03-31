using System.Collections.Generic;
using UnityEngine;

public class CreatureCardListElementUI : MonoBehaviour
{
	[SerializeField] private CreatureCardElementUI creatureCardPrefab;

	private readonly List<CreatureCardElementUI> spawnedCards = new List<CreatureCardElementUI>();

	private void Awake()
	{
		while (spawnedCards.Count < GameRule.TeamMemberCount)
		{
			CreatureCardElementUI card = Instantiate(creatureCardPrefab, gameObject.transform);
			card.gameObject.SetActive(true);
			spawnedCards.Add(card);
		}
	}

	public void Bind(CreatureUnit[] p_team)
	{
		for (int i = 0; i < GameRule.TeamMemberCount; i++)
		{
			CreatureCardElementUI card = spawnedCards[i];

			CreatureUnit unit = null;
			if (p_team != null && i < p_team.Length)
			{
				unit = p_team[i];
			}

			card.Bind(unit);
		}
	}

	public void Clear()
	{
		for (int i = 0; i < spawnedCards.Count; i++)
		{
			spawnedCards[i].ClearBinding();
		}
	}
}