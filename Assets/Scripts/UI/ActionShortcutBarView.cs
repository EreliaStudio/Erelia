using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(HorizontalLayoutGroup))]
[ExecuteAlways]
public sealed class ActionShortcutBarView : ExecuteAlwaysView
{
	private const float PageSelectorWidth = 26f;
	private const float BarHeight = AbilityShortcutBarView.SlotHeight;

	[SerializeField] private AbilityShortcutBarView shortcutBar;
	[SerializeField] private ShortcutBarPageSelectorView pageSelector;

	private BattleUnit boundUnit;

	public event Action<int, Ability> AbilityClicked;
	public BattleUnit BoundUnit => boundUnit;

	private void Reset()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void Awake()
	{
		EnsureHierarchy(true);
	}

	private void OnEnable()
	{
		EnsureHierarchy(true);
		SubscribeChildren();
		RefreshBindings();
	}

	private void OnDisable()
	{
		UnsubscribeChildren();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
		EnsureHierarchy(true);
		RefreshBindings();
#endif
	}

	public void Bind(BattleUnit unit)
	{
		boundUnit = unit;
		EnsureHierarchy(true);
		RefreshBindings();
	}

	[ContextMenu("Refresh")]
	public void RefreshNow()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
		RefreshBindings();
	}

	public bool TrySelectShortcut(int slotIndex)
	{
		EnsureHierarchy(true);
		return shortcutBar != null && shortcutBar.TrySelectShortcut(slotIndex);
	}

	private void EnsureHierarchy(bool allowCreate)
	{
		shortcutBar = ResolveShortcutBar(allowCreate);
		pageSelector = ResolvePageSelector(allowCreate);
	}

	private AbilityShortcutBarView ResolveShortcutBar(bool allowCreate)
	{
		if (shortcutBar != null)
		{
			return shortcutBar;
		}

		Transform child = transform.Find("Shortcut Bar");
		if (child != null && child.TryGetComponent(out AbilityShortcutBarView existing))
		{
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = new GameObject("Shortcut Bar", typeof(RectTransform), typeof(AbilityShortcutBarView), typeof(LayoutElement));
		childObject.transform.SetParent(transform, false);
		LayoutElement layoutElement = childObject.GetComponent<LayoutElement>();
		layoutElement.preferredWidth = AbilityShortcutBarView.PreferredWidth;
		layoutElement.preferredHeight = BarHeight;
		layoutElement.flexibleWidth = 0f;
		layoutElement.flexibleHeight = 0f;
		return childObject.GetComponent<AbilityShortcutBarView>();
	}

	private ShortcutBarPageSelectorView ResolvePageSelector(bool allowCreate)
	{
		if (pageSelector != null)
		{
			return pageSelector;
		}

		Transform child = transform.Find("Page Selector");
		if (child != null && child.TryGetComponent(out ShortcutBarPageSelectorView existing))
		{
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = new GameObject("Page Selector", typeof(RectTransform), typeof(ShortcutBarPageSelectorView), typeof(LayoutElement));
		childObject.transform.SetParent(transform, false);
		LayoutElement layoutElement = childObject.GetComponent<LayoutElement>();
		layoutElement.minWidth = PageSelectorWidth;
		layoutElement.preferredWidth = PageSelectorWidth;
		layoutElement.minHeight = BarHeight;
		layoutElement.preferredHeight = BarHeight;
		layoutElement.flexibleWidth = 0f;
		layoutElement.flexibleHeight = 0f;
		return childObject.GetComponent<ShortcutBarPageSelectorView>();
	}

	private void ApplySerializedState()
	{
		LayoutElement shortcutLayout = shortcutBar != null ? shortcutBar.GetComponent<LayoutElement>() : null;
		if (shortcutLayout != null)
		{
			shortcutLayout.preferredHeight = BarHeight;
		}

		LayoutElement selectorLayout = pageSelector != null ? pageSelector.GetComponent<LayoutElement>() : null;
		if (selectorLayout != null)
		{
			selectorLayout.minHeight = BarHeight;
			selectorLayout.preferredHeight = BarHeight;
		}
	}

	private void SubscribeChildren()
	{
		UnsubscribeChildren();

		if (shortcutBar != null)
		{
			shortcutBar.AbilityClicked += HandleAbilityClicked;
		}

		if (pageSelector != null)
		{
			pageSelector.IndexChanged += HandlePageIndexChanged;
		}
	}

	private void UnsubscribeChildren()
	{
		if (shortcutBar != null)
		{
			shortcutBar.AbilityClicked -= HandleAbilityClicked;
		}

		if (pageSelector != null)
		{
			pageSelector.IndexChanged -= HandlePageIndexChanged;
		}
	}

	private void RefreshBindings()
	{
		IReadOnlyList<Ability> abilities = GetBoundAbilities();
		int maxPageIndex = GetMaxPageIndex(abilities);

		if (pageSelector != null)
		{
			pageSelector.SetRange(maxPageIndex);
		}

		int pageIndex = pageSelector != null ? pageSelector.CurrentIndex : 0;
		shortcutBar?.Bind(abilities, pageIndex);
	}

	private void HandlePageIndexChanged(int pageIndex)
	{
		shortcutBar?.Bind(GetBoundAbilities(), pageIndex);
	}

	private void HandleAbilityClicked(int abilityIndex, Ability ability)
	{
		AbilityClicked?.Invoke(abilityIndex, ability);
	}

	private static int GetMaxPageIndex(IReadOnlyList<Ability> abilities)
	{
		int abilityCount = abilities?.Count ?? 0;
		if (abilityCount <= 0)
		{
			return 0;
		}

		return (abilityCount - 1) / AbilityShortcutBarView.SlotCount;
	}

	private IReadOnlyList<Ability> GetBoundAbilities()
	{
		return boundUnit?.SourceUnit?.Abilities;
	}

#if UNITY_EDITOR
	protected override void OnEditorRefresh()
	{
		EnsureHierarchy(true);
		RefreshBindings();
	}
#endif
}
