using System.Collections.Generic;
using UnityEngine;

public class StatusListElementUI : MonoBehaviour
{
	[SerializeField] private StatusCardElementUI statusCardPrefab;
	[SerializeField] private int minimumVisibleSlotCount = 1;

	private readonly List<StatusCardElementUI> spawnedStatusCardElements = new List<StatusCardElementUI>();
	[SerializeField] private RectTransform cardContainerRoot;

	public void Bind(IReadOnlyList<Status> p_statuses)
	{
		int actualCount = p_statuses != null ? p_statuses.Count : 0;
		int targetCount = actualCount > 0
			? actualCount
			: Mathf.Max(1, minimumVisibleSlotCount);

		EnsurePoolSize(targetCount);

		for (int index = 0; index < spawnedStatusCardElements.Count; index++)
		{
			StatusCardElementUI statusCardElementUI = spawnedStatusCardElements[index];

			if (index >= targetCount)
			{
				statusCardElementUI.Clear();
				statusCardElementUI.gameObject.SetActive(false);
				continue;
			}

			statusCardElementUI.gameObject.SetActive(true);

			if (p_statuses == null || index >= p_statuses.Count || p_statuses[index] == null)
			{
				statusCardElementUI.Clear();
				continue;
			}

			statusCardElementUI.Bind(p_statuses[index]);
		}
	}

	public void Clear()
	{
		Bind(null);
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (spawnedStatusCardElements.Count < p_targetCount)
		{
			StatusCardElementUI statusCardElementUI = Instantiate(statusCardPrefab, GetCardContainerRoot(), false);
			statusCardElementUI.gameObject.SetActive(false);
			spawnedStatusCardElements.Add(statusCardElementUI);
		}
	}

	private Transform GetCardContainerRoot()
	{
		return cardContainerRoot != null ? cardContainerRoot : transform;
	}
}
