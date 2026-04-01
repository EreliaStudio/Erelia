using System.Collections.Generic;
using UnityEngine;

public class AbilityListElementUI : MonoBehaviour
{
	[SerializeField] private AbilityCardElementUI abilityCardPrefab;
	[SerializeField] private int minimumVisibleSlotCount = 1;

	private readonly List<AbilityCardElementUI> spawnedAbilityCardElements = new List<AbilityCardElementUI>();
	[SerializeField] private RectTransform cardContainerRoot;

	public void Bind(IReadOnlyList<Ability> p_abilities)
	{
		int actualCount = p_abilities != null ? p_abilities.Count : 0;
		int targetCount = actualCount > 0
			? actualCount
			: Mathf.Max(0, minimumVisibleSlotCount);

		EnsurePoolSize(targetCount);

		for (int index = 0; index < spawnedAbilityCardElements.Count; index++)
		{
			AbilityCardElementUI abilityCardElementUI = spawnedAbilityCardElements[index];

			if (index >= targetCount)
			{
				abilityCardElementUI.Clear();
				abilityCardElementUI.gameObject.SetActive(false);
				continue;
			}

			abilityCardElementUI.gameObject.SetActive(true);

			if (p_abilities == null || index >= p_abilities.Count || p_abilities[index] == null)
			{
				abilityCardElementUI.Clear();
				continue;
			}

			abilityCardElementUI.Bind(p_abilities[index]);
		}

		gameObject.SetActive(targetCount > 0);
	}

	public void Clear()
	{
		Bind(null);
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (spawnedAbilityCardElements.Count < p_targetCount)
		{
			AbilityCardElementUI abilityCardElementUI = Instantiate(abilityCardPrefab, GetCardContainerRoot(), false);
			abilityCardElementUI.gameObject.SetActive(false);
			spawnedAbilityCardElements.Add(abilityCardElementUI);
		}
	}

	private Transform GetCardContainerRoot()
	{
		return cardContainerRoot != null ? cardContainerRoot : transform;
	}
}
