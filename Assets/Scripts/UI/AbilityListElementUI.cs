using System.Collections.Generic;
using UnityEngine;

public class AbilityListElementUI : MonoBehaviour
{
	[SerializeField] private AbilityCardElementUI abilityCardPrefab;

	private readonly List<AbilityCardElementUI> spawnedAbilityCardElements = new List<AbilityCardElementUI>();

	public void Bind(IReadOnlyList<Ability> p_abilities)
	{
		int targetCount = p_abilities != null ? p_abilities.Count : 0;
		EnsurePoolSize(targetCount);

		for (int index = 0; index < spawnedAbilityCardElements.Count; index++)
		{
			AbilityCardElementUI abilityCardElementUI = spawnedAbilityCardElements[index];

			if (p_abilities == null || index >= p_abilities.Count || p_abilities[index] == null)
			{
				abilityCardElementUI.Clear();
				abilityCardElementUI.gameObject.SetActive(false);
				continue;
			}

			abilityCardElementUI.gameObject.SetActive(true);
			abilityCardElementUI.Bind(p_abilities[index]);
		}
	}

	public void Clear()
	{
		Bind(null);
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (spawnedAbilityCardElements.Count < p_targetCount)
		{
			AbilityCardElementUI abilityCardElementUI = Instantiate(abilityCardPrefab, transform, false);
			abilityCardElementUI.gameObject.SetActive(false);
			spawnedAbilityCardElements.Add(abilityCardElementUI);
		}
	}
}