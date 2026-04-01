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
		EnsureSlotSetup();

		for (int index = 0; index < creatureCardElements.Count; index++)
		{
			BattleUnit battleUnit = null;

			if (p_team != null && index < p_team.Length)
			{
				battleUnit = p_team[index];
			}

			creatureCardElements[index].Bind(battleUnit);
		}
	}

	public void Bind(CreatureUnit[] p_team)
	{
		Bind(CreatePreviewBattleTeam(p_team));
	}

	public void Clear()
	{
		EnsureSlotSetup();

		for (int index = 0; index < creatureCardElements.Count; index++)
		{
			creatureCardElements[index].ClearBinding();
		}
	}

	private void OnTransformChildrenChanged()
	{
		CacheDirectChildSlots();
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
			Transform childTransform = transform.GetChild(index);

			if (childTransform.TryGetComponent(out CreatureCardElementUI creatureCardElementUI))
			{
				creatureCardElements.Add(creatureCardElementUI);
			}
		}
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (creatureCardElements.Count < p_targetCount)
		{
			if (creatureCardPrefab == null)
			{
				Debug.LogWarning($"{nameof(CreatureCardListElementUI)} on {name} is missing a creature card prefab.");
				return;
			}

			CreatureCardElementUI creatureCardElementUI = Instantiate(creatureCardPrefab, transform, false);
			creatureCardElementUI.gameObject.SetActive(true);
			CacheDirectChildSlots();
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
			battleTeam[index] = p_team[index] != null
				? BattleUnit.CreateFromSource(p_team[index])
				: null;
		}

		return battleTeam;
	}
}
