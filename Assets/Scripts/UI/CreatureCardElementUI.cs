using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreatureCardElementUI :
	MonoBehaviour,
	IPointerEnterHandler,
	IPointerExitHandler
{
	[Header("Sub Elements")]
	[SerializeField] private CreatureCardHeaderElementUI headerElementUI;
	[SerializeField] private GameObject expandedRoot;
	[SerializeField] private AttributesElementUI attributesElementUI;
	[SerializeField] private AbilityListElementUI abilityListElementUI;
	[SerializeField] private StatusListElementUI statusListElementUI;

	[Header("Layout")]
	[SerializeField] private float preferredWidth = 320f;
	[SerializeField] private float collapsedHeight = 56f;
	[SerializeField] private float expandedHeight = 0f;

	[Header("Hover")]
	[SerializeField] private float collapseDelay = 0.1f;

	private BattleUnit linkedBattleUnit;
	private LayoutElement layoutElement;
	private VerticalLayoutGroup verticalLayoutGroup;
	private RectTransform expandedRootRectTransform;
	private bool isExpanded;
	private Coroutine collapseCoroutine;

	private void Awake()
	{
		layoutElement = GetComponent<LayoutElement>();
		verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
		expandedRootRectTransform = (RectTransform) expandedRoot.transform;
		ApplyVisualState();
	}

	public void Bind(BattleUnit p_battleUnit)
	{
		if (ReferenceEquals(linkedBattleUnit, p_battleUnit) == false)
		{
			UnsubscribeFromBattleUnit(linkedBattleUnit);
			linkedBattleUnit = p_battleUnit;
			SubscribeToBattleUnit(linkedBattleUnit);
		}

		Refresh();
	}

	public void ClearBinding()
	{
		UnsubscribeFromBattleUnit(linkedBattleUnit);
		linkedBattleUnit = null;
		SetExpanded(false, true);
		Refresh();
	}

	private void OnDestroy()
	{
		UnsubscribeFromBattleUnit(linkedBattleUnit);
	}

	public void Refresh()
	{
		if (CanExpand())
		{
			headerElementUI.Bind(linkedBattleUnit);
			attributesElementUI.Bind(linkedBattleUnit);
			abilityListElementUI.Bind(linkedBattleUnit.Abilities);
			statusListElementUI.Bind(linkedBattleUnit.Statuses);
		}
		else
		{
			headerElementUI.Clear();
			attributesElementUI.Clear();
			abilityListElementUI.Clear();
			statusListElementUI.Clear();
			SetExpanded(false, true);
			return;
		}

		ApplyVisualState();
	}

	public void OnPointerEnter(PointerEventData p_eventData)
	{
		if (CanExpand() == false)
		{
			return;
		}

		StopCollapseCoroutine();
		SetExpanded(true, false);
	}

	public void OnPointerExit(PointerEventData p_eventData)
	{
		StopCollapseCoroutine();
		collapseCoroutine = StartCoroutine(CollapseDelayed());
	}

	private IEnumerator CollapseDelayed()
	{
		yield return new WaitForSeconds(collapseDelay);
		collapseCoroutine = null;
		SetExpanded(false, false);
	}

	private bool CanExpand()
	{
		return linkedBattleUnit != null &&
			   linkedBattleUnit.SourceUnit != null &&
			   linkedBattleUnit.SourceUnit.Species != null;
	}

	private void SetExpanded(bool p_isExpanded, bool p_force)
	{
		if (CanExpand() == false)
		{
			p_isExpanded = false;
		}

		if (isExpanded == p_isExpanded && p_force == false)
		{
			return;
		}

		isExpanded = p_isExpanded;
		ApplyVisualState();
	}

	private void ApplyVisualState()
	{
		bool shouldShowExpandedRoot = isExpanded && CanExpand();
		expandedRoot.SetActive(shouldShowExpandedRoot);
		layoutElement.preferredWidth = preferredWidth;
		layoutElement.preferredHeight = shouldShowExpandedRoot
			? Mathf.Max(expandedHeight, CalculateExpandedPreferredHeight())
			: collapsedHeight;
	}

	private float CalculateExpandedPreferredHeight()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(expandedRootRectTransform);
		return collapsedHeight + LayoutUtility.GetPreferredHeight(expandedRootRectTransform) + verticalLayoutGroup.spacing;
	}

	private void SubscribeToBattleUnit(BattleUnit p_battleUnit)
	{
		if (p_battleUnit == null)
		{
			return;
		}

		p_battleUnit.Statuses.Changed += HandleStatusesChanged;
	}

	private void UnsubscribeFromBattleUnit(BattleUnit p_battleUnit)
	{
		if (p_battleUnit == null)
		{
			return;
		}

		p_battleUnit.Statuses.Changed -= HandleStatusesChanged;
	}

	private void HandleStatusesChanged(BattleStatuses p_statuses)
	{
		if (linkedBattleUnit == null)
		{
			return;
		}

		statusListElementUI.Bind(p_statuses);
		ApplyVisualState();
	}

	private void StopCollapseCoroutine()
	{
		if (collapseCoroutine == null)
		{
			return;
		}

		StopCoroutine(collapseCoroutine);
		collapseCoroutine = null;
	}
}
