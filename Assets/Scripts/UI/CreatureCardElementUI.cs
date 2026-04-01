using System.Collections;
using System.Runtime.CompilerServices;
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
	[SerializeField] private LayoutElement layoutElement;
	[SerializeField] private float preferredWidth = 320f;
	[SerializeField] private float collapsedHeight = 56f;
	[SerializeField] private float expandedHeight = 0f;

	[Header("Hover")]
	[SerializeField] private float collapseDelay = 0.1f;

	private BattleUnit linkedBattleUnit;
	private bool isExpanded;
	private Coroutine collapseCoroutine;
	private VerticalLayoutGroup verticalLayoutGroup;
	private RectTransform expandedRootRectTransform;
	private int lastBindingVersionHash;

	private void Awake()
	{
		layoutElement ??= GetComponent<LayoutElement>();
		verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
		expandedRootRectTransform = expandedRoot != null ? expandedRoot.transform as RectTransform : null;
		ApplyVisualState();
	}

	private void OnValidate()
	{
		layoutElement ??= GetComponent<LayoutElement>();
	}

	public void Bind(BattleUnit p_battleUnit)
	{
		linkedBattleUnit = p_battleUnit;
		Refresh();
		lastBindingVersionHash = ComputeBindingVersionHash();
	}

	public void ClearBinding()
	{
		linkedBattleUnit = null;
		Refresh();
		lastBindingVersionHash = 0;
		SetExpanded(false, true);
	}

	public void Refresh()
	{
		RenderHeader();
		RenderExpanded();

		if (CanExpand() == false)
		{
			SetExpanded(false, true);
		}
		else
		{
			ApplyVisualState();
		}
	}

	public void OnPointerEnter(PointerEventData p_eventData)
	{
		if (CanExpand() == false)
		{
			return;
		}

		if (collapseCoroutine != null)
		{
			StopCoroutine(collapseCoroutine);
			collapseCoroutine = null;
		}

		Refresh();
		SetExpanded(true, false);
	}

	public void OnPointerExit(PointerEventData p_eventData)
	{
		if (collapseCoroutine != null)
		{
			StopCoroutine(collapseCoroutine);
		}

		collapseCoroutine = StartCoroutine(CollapseDelayed());
	}

	private IEnumerator CollapseDelayed()
	{
		yield return new WaitForSeconds(collapseDelay);
		collapseCoroutine = null;
		SetExpanded(false, false);
	}

	private void LateUpdate()
	{
		int currentBindingVersionHash = ComputeBindingVersionHash();
		if (currentBindingVersionHash == lastBindingVersionHash)
		{
			return;
		}

		lastBindingVersionHash = currentBindingVersionHash;
		Refresh();
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

		if (expandedRoot != null)
		{
			expandedRoot.SetActive(shouldShowExpandedRoot);
		}

		if (layoutElement != null)
		{
			layoutElement.preferredWidth = preferredWidth;
			layoutElement.preferredHeight = shouldShowExpandedRoot
				? Mathf.Max(expandedHeight, CalculateExpandedPreferredHeight())
				: collapsedHeight;
		}
	}

	private float CalculateExpandedPreferredHeight()
	{
		if (expandedRootRectTransform == null)
		{
			return expandedHeight;
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(expandedRootRectTransform);

		float preferredHeight = collapsedHeight + LayoutUtility.GetPreferredHeight(expandedRootRectTransform);

		if (verticalLayoutGroup != null)
		{
			preferredHeight += verticalLayoutGroup.spacing;
		}

		return preferredHeight;
	}

	private void RenderHeader()
	{
		if (headerElementUI == null)
		{
			return;
		}

		CreatureUnit sourceUnit = linkedBattleUnit != null ? linkedBattleUnit.SourceUnit : null;
		if (sourceUnit == null)
		{
			headerElementUI.Clear();
			return;
		}

		headerElementUI.Bind(sourceUnit);
	}

	private void RenderExpanded()
	{
		if (linkedBattleUnit == null ||
			linkedBattleUnit.SourceUnit == null ||
			linkedBattleUnit.SourceUnit.Species == null)
		{
			if (attributesElementUI != null)
			{
				attributesElementUI.Clear();
			}

			if (abilityListElementUI != null)
			{
				abilityListElementUI.Clear();
			}

			if (statusListElementUI != null)
			{
				statusListElementUI.Clear();
			}

			return;
		}

		if (attributesElementUI != null)
		{
			attributesElementUI.Bind(linkedBattleUnit);
		}

		if (abilityListElementUI != null)
		{
			abilityListElementUI.Bind(linkedBattleUnit.SourceUnit.Abilities);
		}

		if (statusListElementUI != null)
		{
			statusListElementUI.Bind(linkedBattleUnit.Statuses);
		}
	}

	private int ComputeBindingVersionHash()
	{
		if (linkedBattleUnit == null)
		{
			return 0;
		}

		unchecked
		{
			int hash = 17;
			hash = (hash * 31) + RuntimeHelpers.GetHashCode(linkedBattleUnit);

			CreatureUnit sourceUnit = linkedBattleUnit.SourceUnit;
			hash = (hash * 31) + (sourceUnit != null ? RuntimeHelpers.GetHashCode(sourceUnit) : 0);
			hash = (hash * 31) + (sourceUnit?.Species != null ? RuntimeHelpers.GetHashCode(sourceUnit.Species) : 0);
			hash = (hash * 31) + (sourceUnit?.CurrentFormID != null ? sourceUnit.CurrentFormID.GetHashCode() : 0);
			hash = (hash * 31) + linkedBattleUnit.CurrentHealth;
			hash = (hash * 31) + linkedBattleUnit.CurrentActionPoints;
			hash = (hash * 31) + linkedBattleUnit.CurrentMovementPoints;
			hash = (hash * 31) + linkedBattleUnit.MaxHealth;
			hash = (hash * 31) + linkedBattleUnit.MaxActionPoints;
			hash = (hash * 31) + linkedBattleUnit.MaxMovementPoints;
			hash = (hash * 31) + linkedBattleUnit.IsDefeated.GetHashCode();

			if (sourceUnit != null)
			{
				hash = (hash * 31) + GetListSignature(sourceUnit.Abilities);
			}

			hash = (hash * 31) + GetBattleStatusSignature(linkedBattleUnit.Statuses);
			return hash;
		}
	}

	private static int GetListSignature<TItem>(System.Collections.Generic.IReadOnlyList<TItem> p_items)
		where TItem : class
	{
		if (p_items == null)
		{
			return 0;
		}

		unchecked
		{
			int hash = p_items.Count;
			for (int index = 0; index < p_items.Count; index++)
			{
				TItem item = p_items[index];
				hash = (hash * 31) + (item != null ? RuntimeHelpers.GetHashCode(item) : 0);
			}

			return hash;
		}
	}

	private static int GetBattleStatusSignature(System.Collections.Generic.IReadOnlyList<BattleStatus> p_statuses)
	{
		if (p_statuses == null)
		{
			return 0;
		}

		unchecked
		{
			int hash = p_statuses.Count;
			for (int index = 0; index < p_statuses.Count; index++)
			{
				BattleStatus battleStatus = p_statuses[index];
				if (battleStatus == null)
				{
					hash *= 31;
					continue;
				}

				hash = (hash * 31) + (battleStatus.Status != null ? RuntimeHelpers.GetHashCode(battleStatus.Status) : 0);
				hash = (hash * 31) + battleStatus.Stack;
				hash = (hash * 31) + GetDurationSignature(battleStatus.RemainingDuration);
			}

			return hash;
		}
	}

	private static int GetDurationSignature(Duration p_duration)
	{
		if (p_duration == null)
		{
			return 0;
		}

		unchecked
		{
			int hash = 17;
			hash = (hash * 31) + (int) p_duration.Type;
			hash = (hash * 31) + p_duration.Turns;
			hash = (hash * 31) + p_duration.Seconds.GetHashCode();
			return hash;
		}
	}
}
