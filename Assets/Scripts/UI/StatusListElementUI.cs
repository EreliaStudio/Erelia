using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusListElementUI : MonoBehaviour
{
	[SerializeField] private StatusCardElementUI statusCardPrefab;
	[SerializeField] private int minimumVisibleSlotCount = 1;
	[SerializeField] private RectTransform cardContainerRoot;

	private readonly List<StatusCardElementUI> statusCardElements = new List<StatusCardElementUI>();
	private LayoutElement layoutElement;

	public event Action<BattleStatus> StatusHovered;
	public event Action StatusHoverEnded;

	private void Awake()
	{
		ResolveReferences();
		CacheExistingCards();
	}

	public void Bind(BattleStatuses p_statuses)
	{
		ResolveReferences();
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

		UpdateLayoutMetrics(targetCount);
		gameObject.SetActive(targetCount > 0);
	}

	public void Clear()
	{
		Bind(null);
	}

	private void OnDestroy()
	{
		for (int index = 0; index < statusCardElements.Count; index++)
		{
			if (statusCardElements[index] == null)
			{
				continue;
			}

			statusCardElements[index].Hovered -= HandleStatusCardHovered;
			statusCardElements[index].HoverEnded -= HandleStatusCardHoverEnded;
		}
	}

	private void EnsurePoolSize(int p_targetCount)
	{
		ResolveReferences();
		CacheExistingCards();

		while (statusCardElements.Count < p_targetCount)
		{
			StatusCardElementUI element = Instantiate(statusCardPrefab, cardContainerRoot, false);
			element.Hovered += HandleStatusCardHovered;
			element.HoverEnded += HandleStatusCardHoverEnded;
			statusCardElements.Add(element);
		}
	}

	private void ResolveReferences()
	{
		cardContainerRoot ??= transform as RectTransform;
		layoutElement ??= GetComponent<LayoutElement>();
	}

	private void CacheExistingCards()
	{
		if (cardContainerRoot == null)
		{
			return;
		}

		for (int index = 0; index < cardContainerRoot.childCount; index++)
		{
			StatusCardElementUI element = cardContainerRoot.GetChild(index).GetComponent<StatusCardElementUI>();
			if (element == null || statusCardElements.Contains(element))
			{
				continue;
			}

			element.Hovered += HandleStatusCardHovered;
			element.HoverEnded += HandleStatusCardHoverEnded;
			statusCardElements.Add(element);
		}
	}

	private void HandleStatusCardHovered(StatusCardElementUI p_card, BattleStatus p_status)
	{
		StatusHovered?.Invoke(p_status);
	}

	private void HandleStatusCardHoverEnded(StatusCardElementUI p_card)
	{
		StatusHoverEnded?.Invoke();
	}

	private void UpdateLayoutMetrics(int p_visibleCount)
	{
		if (layoutElement == null)
		{
			return;
		}

		float preferredHeight = ComputePreferredHeight(p_visibleCount);
		layoutElement.minHeight = preferredHeight;
		layoutElement.preferredHeight = preferredHeight;

		if (transform is RectTransform rectTransform)
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}

		if (transform.parent is RectTransform parentRectTransform)
		{
			LayoutRebuilder.MarkLayoutForRebuild(parentRectTransform);
		}
	}

	private float ComputePreferredHeight(int p_visibleCount)
	{
		if (p_visibleCount <= 0 || cardContainerRoot == null)
		{
			return 0f;
		}

		GridLayoutGroup gridLayout = cardContainerRoot.GetComponent<GridLayoutGroup>();
		if (gridLayout == null)
		{
			return LayoutUtility.GetPreferredHeight(cardContainerRoot);
		}

		int rowCount = gridLayout.constraint switch
		{
			GridLayoutGroup.Constraint.FixedColumnCount => Mathf.CeilToInt((float) p_visibleCount / Mathf.Max(1, gridLayout.constraintCount)),
			GridLayoutGroup.Constraint.FixedRowCount => Mathf.Max(1, Mathf.Min(p_visibleCount, gridLayout.constraintCount)),
			_ => 1
		};

		return rowCount * gridLayout.cellSize.y + Mathf.Max(0, rowCount - 1) * gridLayout.spacing.y;
	}
}
