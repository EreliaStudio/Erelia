using System.Collections.Generic;
using UnityEngine;

public class StatusListElementUI : MonoBehaviour
{
	[SerializeField] private StatusCardElementUI statusCardPrefab;

	private readonly List<StatusCardElementUI> spawnedStatusCardElements = new List<StatusCardElementUI>();

	public void Bind(IReadOnlyList<Status> p_statuses)
	{
		int targetCount = p_statuses != null ? p_statuses.Count : 0;
		EnsurePoolSize(targetCount);

		for (int index = 0; index < spawnedStatusCardElements.Count; index++)
		{
			StatusCardElementUI statusCardElementUI = spawnedStatusCardElements[index];

			if (p_statuses == null || index >= p_statuses.Count || p_statuses[index] == null)
			{
				statusCardElementUI.Clear();
				statusCardElementUI.gameObject.SetActive(false);
				continue;
			}

			statusCardElementUI.gameObject.SetActive(true);
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
			StatusCardElementUI statusCardElementUI = Instantiate(statusCardPrefab, transform, false);
			statusCardElementUI.gameObject.SetActive(false);
			spawnedStatusCardElements.Add(statusCardElementUI);
		}
	}
}