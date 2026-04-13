using System.Collections.Generic;
using UnityEngine;

public class CreatureCardListElementUI : MonoBehaviour
{
	[SerializeField] private CreatureCardElementUI creatureCardPrefab;
	[SerializeField] private int creatureCardCount = 6;

	private readonly List<CreatureCardElementUI> creatureCardElements = new List<CreatureCardElementUI>();

	private void Awake()
	{
		EnsureSlotSetup();
		Clear();
	}

	public void Bind(BattleUnit[] p_team)
	{
		EnsurePoolSize(creatureCardCount);

		for (int index = 0; index < creatureCardElements.Count; index++)
		{
			BattleUnit battleUnit = p_team != null && index < p_team.Length ? p_team[index] : null;
			creatureCardElements[index].Bind(battleUnit);
		}
	}

	public void Bind(CreatureUnit[] p_team)
	{
		Bind(CreatePreviewBattleTeam(p_team));
	}

	public void Clear()
	{
		EnsurePoolSize(creatureCardCount);

		for (int index = 0; index < creatureCardElements.Count; index++)
		{
			creatureCardElements[index].ClearBinding();
		}
	}

	private void EnsureSlotSetup()
	{
		CacheDirectChildSlots();
		EnsurePoolSize(creatureCardCount);
	}

	private void CacheDirectChildSlots()
	{
		creatureCardElements.Clear();

		for (int index = 0; index < transform.childCount; index++)
		{
			creatureCardElements.Add(transform.GetChild(index).GetComponent<CreatureCardElementUI>());
		}
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (creatureCardElements.Count < p_targetCount)
		{
			creatureCardElements.Add(Instantiate(creatureCardPrefab, transform, false));
		}
	}

	private static BattleUnit[] CreatePreviewBattleTeam(CreatureUnit[] p_team)
	{
		if (p_team == null)
		{
			return null;
		}

		BattleUnit[] battleTeam = new BattleUnit[p_team.Length];
		for (int index = 0; index < p_team.Length; index++)
		{
			if (p_team[index] == null)
			{
				battleTeam[index] = null;
				continue;
			}

			FeatProgressionService.ApplyProgress(p_team[index]);
			battleTeam[index] = new BattleUnit(p_team[index], BattleSide.Neutral);
		}

		return battleTeam;
	}
}
