using System.Collections;
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
	[SerializeField] private float expandedHeight = 180f;

	[Header("Hover")]
	[SerializeField] private float collapseDelay = 0.1f;

	private CreatureUnit linkedCreatureUnit;
	private bool isExpanded;
	private Coroutine collapseCoroutine;
	private VerticalLayoutGroup verticalLayoutGroup;
	private RectTransform expandedRootRectTransform;

	private void Awake()
	{
		verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
		expandedRootRectTransform = expandedRoot != null ? expandedRoot.transform as RectTransform : null;
		ApplyVisualState();
	}

	public void Bind(CreatureUnit p_creatureUnit)
	{
		linkedCreatureUnit = p_creatureUnit;
		Refresh();
	}

	public void ClearBinding()
	{
		linkedCreatureUnit = null;
		Refresh();
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

	private bool CanExpand()
	{
		return linkedCreatureUnit != null &&
			   linkedCreatureUnit.Species != null;
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

		if (linkedCreatureUnit == null)
		{
			headerElementUI.Clear();
			return;
		}

		headerElementUI.Bind(linkedCreatureUnit);
	}

	private void RenderExpanded()
	{
		if (linkedCreatureUnit == null || linkedCreatureUnit.Species == null)
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
			attributesElementUI.Bind(linkedCreatureUnit.Attributes);
		}

		if (abilityListElementUI != null)
		{
			abilityListElementUI.Bind(linkedCreatureUnit.Abilities);
		}

		if (statusListElementUI != null)
		{
			statusListElementUI.Bind(linkedCreatureUnit.PermanentPassives);
		}
	}
}
