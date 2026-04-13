using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityListElementUI : MonoBehaviour
{
	[SerializeField] private AbilityCardElementUI abilityCardPrefab;
	[SerializeField] private int minimumVisibleSlotCount = 1;
	[SerializeField] private bool displayShortcutLabels = false;
	[SerializeField] private RectTransform cardContainerRoot;

	private readonly List<AbilityCardElementUI> abilityCardElements = new List<AbilityCardElementUI>();
	private IReadOnlyList<Ability> linkedAbilities;

	public event Action<Ability, int> AbilityHovered;
	public event Action AbilityHoverEnded;

	private void Awake()
	{
		ResolveReferences();
		CacheExistingCards();
	}

	public void Bind(IReadOnlyList<Ability> p_abilities)
	{
		ResolveReferences();
		linkedAbilities = p_abilities;

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

			abilityCardElementUI.Bind(index < actualCount ? p_abilities[index] : null, index, displayShortcutLabels);
		}

		gameObject.SetActive(targetCount > 0);
	}

	public void Clear()
	{
		Bind(null);
	}

	public void SetDisplayShortcutLabels(bool p_value)
	{
		if (displayShortcutLabels == p_value)
		{
			return;
		}

		displayShortcutLabels = p_value;
		Bind(linkedAbilities);
	}

	private void OnDestroy()
	{
		for (int index = 0; index < abilityCardElements.Count; index++)
		{
			if (abilityCardElements[index] == null)
			{
				continue;
			}

			abilityCardElements[index].Hovered -= HandleAbilityCardHovered;
			abilityCardElements[index].HoverEnded -= HandleAbilityCardHoverEnded;
		}
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		CacheExistingCards();

		while (abilityCardElements.Count < p_targetCount)
		{
			AbilityCardElementUI element = Instantiate(abilityCardPrefab, cardContainerRoot, false);
			element.Hovered += HandleAbilityCardHovered;
			element.HoverEnded += HandleAbilityCardHoverEnded;
			abilityCardElements.Add(element);
		}
	}

	private void ResolveReferences()
	{
		cardContainerRoot ??= transform as RectTransform;
	}

	private void CacheExistingCards()
	{
		if (cardContainerRoot == null)
		{
			return;
		}

		for (int index = 0; index < cardContainerRoot.childCount; index++)
		{
			AbilityCardElementUI element = cardContainerRoot.GetChild(index).GetComponent<AbilityCardElementUI>();
			if (element == null || abilityCardElements.Contains(element))
			{
				continue;
			}

			element.Hovered += HandleAbilityCardHovered;
			element.HoverEnded += HandleAbilityCardHoverEnded;
			abilityCardElements.Add(element);
		}
	}

	private void HandleAbilityCardHovered(AbilityCardElementUI p_card, Ability p_ability, int p_index)
	{
		AbilityHovered?.Invoke(p_ability, p_index);
	}

	private void HandleAbilityCardHoverEnded(AbilityCardElementUI p_card)
	{
		AbilityHoverEnded?.Invoke();
	}
}
