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
	[SerializeField] private LayoutElement layoutElement;
	[SerializeField] private RectTransform mainContentRectTransform;

	private CreatureUnit linkedCreatureUnit;
	private bool isExpanded;

	public CreatureUnit LinkedCreatureUnit => linkedCreatureUnit;
	public bool IsExpanded => isExpanded;

	private void Awake()
	{
		SetExpanded(false, true);
		Clear();
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
	}

	public void Refresh()
	{
		RenderHeader(linkedCreatureUnit);
		RenderExpanded(linkedCreatureUnit);
		RebuildLayout();
	}

	public void OnPointerEnter(PointerEventData p_eventData)
	{
		if (linkedCreatureUnit == null)
		{
			return;
		}

		SetExpanded(true, false);
	}

	public void OnPointerExit(PointerEventData p_eventData)
	{
		SetExpanded(false, false);
	}

	private void Clear()
	{
		RenderHeader(null);
		RenderExpanded(null);
		RebuildLayout();
	}

	private void SetExpanded(bool p_value, bool p_force)
	{
		if (isExpanded == p_value && p_force == false)
		{
			return;
		}

		isExpanded = p_value;

		if (expandedRoot != null)
		{
			expandedRoot.SetActive(isExpanded);
		}

		RebuildLayout();
	}

	private void RenderHeader(CreatureUnit p_creatureUnit)
	{
		if (headerElementUI == null)
		{
			return;
		}

		if (p_creatureUnit == null)
		{
			headerElementUI.Clear();
			return;
		}

		headerElementUI.Bind(p_creatureUnit);
	}

	private void RenderExpanded(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null)
		{
			RenderAttributes(null);
			RenderAbilities(null);
			RenderStatuses(null);
			return;
		}

		RenderAttributes(p_creatureUnit.Attributes);
		RenderAbilities(p_creatureUnit.Abilities);
		RenderStatuses(p_creatureUnit.PermanentPassives);
	}

	private void RenderAttributes(Attributes p_attributes)
	{
		if (attributesElementUI == null)
		{
			return;
		}

		attributesElementUI.Bind(p_attributes);
	}

	private void RenderAbilities(IReadOnlyList<Ability> p_abilities)
	{
		if (abilityListElementUI == null)
		{
			return;
		}

		abilityListElementUI.Bind(p_abilities);
	}

	private void RenderStatuses(IReadOnlyList<Status> p_statuses)
	{
		if (statusListElementUI == null)
		{
			return;
		}

		statusListElementUI.Bind(p_statuses);
	}

	private void RebuildLayout()
	{
		if (mainContentRectTransform == null || layoutElement == null)
		{
			return;
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(mainContentRectTransform);

		float preferredHeight = LayoutUtility.GetPreferredHeight(mainContentRectTransform);

		layoutElement.preferredHeight = preferredHeight;

		if (transform.parent is RectTransform parentRectTransform)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);
		}
	}
}