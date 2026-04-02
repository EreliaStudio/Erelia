using System.Collections.Generic;
using UnityEngine;

public class AbilityListElementUI : MonoBehaviour
{
	[SerializeField] private AbilityCardElementUI abilityCardPrefab;
	[SerializeField] private int minimumVisibleSlotCount = 1;
	[SerializeField] private RectTransform cardContainerRoot;

	private readonly List<AbilityCardElementUI> abilityCardElements = new List<AbilityCardElementUI>();

	public void Bind(IReadOnlyList<Ability> p_abilities)
	{
		int actualCount = p_abilities?.Count ?? 0;
		int targetCount = Mathf.Max(actualCount, minimumVisibleSlotCount);

		EnsurePoolSize(targetCount);

		for (int index = 0; index < abilityCardElements.Count; index++)
		{
			AbilityCardElementUI abilityCardElementUI = abilityCardElements[index];
			bool isVisible = index < targetCount;

			abilityCardElementUI.gameObject.SetActive(isVisible);
			if (isVisible == false)
			{
				abilityCardElementUI.Clear();
				continue;
			}

			abilityCardElementUI.Bind(index < actualCount ? p_abilities[index] : null);
		}

		gameObject.SetActive(targetCount > 0);
	}

	public void Clear()
	{
		Bind(null);
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (abilityCardElements.Count < p_targetCount)
		{
			AbilityCardElementUI element = Instantiate(abilityCardPrefab, cardContainerRoot, false);
			abilityCardElements.Add(element);
		}
	}
}
