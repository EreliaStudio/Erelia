using System.Collections.Generic;
using UnityEngine;

public class StatusListElementUI : MonoBehaviour
{
	[SerializeField] private StatusCardElementUI statusCardPrefab;
	[SerializeField] private int minimumVisibleSlotCount = 1;
	[SerializeField] private RectTransform cardContainerRoot;

	private readonly List<StatusCardElementUI> statusCardElements = new List<StatusCardElementUI>();

	public void Bind(BattleStatuses p_statuses)
	{
		int actualCount = p_statuses?.Count ?? 0;
		int targetCount = Mathf.Max(actualCount, minimumVisibleSlotCount);

		EnsurePoolSize(targetCount);

		for (int index = 0; index < statusCardElements.Count; index++)
		{
			StatusCardElementUI statusCardElementUI = statusCardElements[index];
			bool isVisible = index < targetCount;

			statusCardElementUI.gameObject.SetActive(isVisible);
			if (isVisible == false)
			{
				statusCardElementUI.Clear();
				continue;
			}

			statusCardElementUI.Bind(index < actualCount ? p_statuses[index] : null);
		}

		gameObject.SetActive(targetCount > 0);
	}

	public void Clear()
	{
		Bind(null);
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		while (statusCardElements.Count < p_targetCount)
		{
			StatusCardElementUI element = Instantiate(statusCardPrefab, cardContainerRoot, false);
			statusCardElements.Add(element);
		}
	}
}
