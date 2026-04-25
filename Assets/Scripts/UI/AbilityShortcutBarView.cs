using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(HorizontalLayoutGroup))]
[ExecuteAlways]
public sealed class AbilityShortcutBarView : ExecuteAlwaysView
{
	public const int SlotCount = 8;

	[SerializeField, Min(0f)] private float slotWidth = 56f;
	[SerializeField, Min(0f)] private float slotHeight = 48f;
	[SerializeField, Min(0f)] private float slotSpacing = 4f;
	[SerializeField] private AbilityShortcutView shortcutPrefab;

	private readonly List<AbilityShortcutView> shortcutInstances = new();
	private IReadOnlyList<Ability> boundAbilities;
	private int pageIndex;
	private UnityEngine.Object lastPrefabReference;

	public event Action<int, Ability> AbilityClicked;
	public float SlotWidth => slotWidth;
	public float SlotHeight => slotHeight;
	public float PreferredWidth => (slotWidth * SlotCount) + (slotSpacing * (SlotCount - 1));

	private void Reset()
	{
		EnsureShortcuts();
	}

	private void Awake()
	{
		EnsureShortcuts();
	}

	private void OnEnable()
	{
		EnsureShortcuts();
		RefreshBindings();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
		EnsureShortcuts();
		RefreshBindings();
#endif
	}

	public void Bind(IReadOnlyList<Ability> abilities, int page)
	{
		boundAbilities = abilities;
		pageIndex = Mathf.Max(0, page);
		EnsureShortcuts();
		RefreshBindings();
	}

	public AbilityShortcutView GetShortcut(int index)
	{
		EnsureShortcuts();

		if (index < 0 || index >= shortcutInstances.Count)
		{
			return null;
		}

		return shortcutInstances[index];
	}

	public bool TrySelectShortcut(int slotIndex)
	{
		EnsureShortcuts();

		if (slotIndex < 0 || slotIndex >= SlotCount)
		{
			return false;
		}

		int abilityIndex = (pageIndex * SlotCount) + slotIndex;
		Ability ability = boundAbilities != null && abilityIndex < boundAbilities.Count ? boundAbilities[abilityIndex] : null;
		if (ability == null)
		{
			return false;
		}

		AbilityClicked?.Invoke(abilityIndex, ability);
		return true;
	}

	[ContextMenu("Refresh")]
	public void RefreshNow()
	{
		EnsureShortcuts();
		ApplySerializedState();
		RefreshBindings();
	}

	public void ConfigureDefaultLayout(float defaultSlotWidth, float defaultSlotHeight, float defaultSlotSpacing)
	{
		slotWidth = Mathf.Max(0f, defaultSlotWidth);
		slotHeight = Mathf.Max(0f, defaultSlotHeight);
		slotSpacing = Mathf.Max(0f, defaultSlotSpacing);
		ApplySerializedState();
	}

	private void EnsureShortcuts()
	{
		if (UiViewUtility.IsPersistentAssetContext(this))
		{
			return;
		}

		CollectShortcuts();

		if (!NeedsRebuild())
		{
			return;
		}

		RebuildShortcuts();
	}

	private void CollectShortcuts()
	{
		shortcutInstances.Clear();

		for (int index = 0; index < transform.childCount; index++)
		{
			Transform child = transform.GetChild(index);
			if (child != null && child.TryGetComponent(out AbilityShortcutView shortcut))
			{
				shortcutInstances.Add(shortcut);
			}
		}
	}

	private bool NeedsRebuild()
	{
		if (!ReferenceEquals(lastPrefabReference, shortcutPrefab))
		{
			return true;
		}

		if (shortcutInstances.Count != SlotCount || transform.childCount != shortcutInstances.Count)
		{
			return true;
		}

		for (int index = 0; index < shortcutInstances.Count; index++)
		{
			if (shortcutInstances[index] == null)
			{
				return true;
			}
		}

		return false;
	}

	private void RebuildShortcuts()
	{
		DestroyAllChildren();
		shortcutInstances.Clear();

		for (int index = 0; index < SlotCount; index++)
		{
			AbilityShortcutView instance = shortcutPrefab != null
				? CreateShortcutInstance(index)
				: CreateDefaultShortcutInstance(index);

			if (instance != null)
			{
				shortcutInstances.Add(instance);
			}
		}

		lastPrefabReference = shortcutPrefab;
		RefreshBindings();
	}

	private AbilityShortcutView CreateShortcutInstance(int index)
	{
		if (UiViewUtility.IsPersistentAssetContext(this))
		{
			return null;
		}

		GameObject instanceObject = InstantiateShortcutPrefab();
		if (instanceObject == null)
		{
			return null;
		}

		instanceObject.name = $"{shortcutPrefab.name} {index + 1}";
		if (instanceObject.transform.parent != transform)
		{
			instanceObject.transform.SetParent(transform, false);
		}

		if (!instanceObject.TryGetComponent(out AbilityShortcutView shortcut))
		{
			UiViewUtility.DestroyGeneratedObject(instanceObject);
			return null;
		}

		if (!instanceObject.TryGetComponent(out LayoutElement layoutElement))
		{
			layoutElement = instanceObject.AddComponent<LayoutElement>();
		}

		ApplyShortcutLayout(layoutElement);
		shortcut.Clicked += HandleShortcutClicked;
		return shortcut;
	}

	private AbilityShortcutView CreateDefaultShortcutInstance(int index)
	{
		if (UiViewUtility.IsPersistentAssetContext(this))
		{
			return null;
		}

		GameObject instanceObject = new GameObject($"Ability Shortcut {index + 1}", typeof(RectTransform), typeof(AbilityShortcutView), typeof(LayoutElement));
		instanceObject.transform.SetParent(transform, false);

		ApplyShortcutLayout(instanceObject.GetComponent<LayoutElement>());

		AbilityShortcutView shortcut = instanceObject.GetComponent<AbilityShortcutView>();
		shortcut.Clicked += HandleShortcutClicked;
		shortcut.RefreshNow();
		return shortcut;
	}

	private GameObject InstantiateShortcutPrefab()
	{
		if (shortcutPrefab == null)
		{
			return null;
		}

#if UNITY_EDITOR
		if (UiViewUtility.IsPersistentAssetContext(this))
		{
			return null;
		}

		if (!Application.isPlaying)
		{
			UnityEngine.Object prefabSource = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(shortcutPrefab.gameObject);
			UnityEngine.Object prefabObject = prefabSource != null ? prefabSource : shortcutPrefab.gameObject;
			return UnityEditor.PrefabUtility.InstantiatePrefab(prefabObject, transform) as GameObject;
		}
#endif

		return Instantiate(shortcutPrefab.gameObject, transform);
	}

	private void RefreshBindings()
	{
		ApplySerializedState();
		ApplyShortcutLayouts();

		for (int index = 0; index < shortcutInstances.Count; index++)
		{
			AbilityShortcutView shortcut = shortcutInstances[index];
			if (shortcut == null)
			{
				continue;
			}

			shortcut.Clicked -= HandleShortcutClicked;
			shortcut.Clicked += HandleShortcutClicked;

			int abilityIndex = (pageIndex * SlotCount) + index;
			Ability ability = boundAbilities != null && abilityIndex < boundAbilities.Count ? boundAbilities[abilityIndex] : null;
			shortcut.Bind(ability, index);
		}
	}

	private void ApplyShortcutLayouts()
	{
		for (int index = 0; index < shortcutInstances.Count; index++)
		{
			AbilityShortcutView shortcut = shortcutInstances[index];
			if (shortcut == null)
			{
				continue;
			}

			if (!shortcut.TryGetComponent(out LayoutElement layoutElement))
			{
				layoutElement = shortcut.gameObject.AddComponent<LayoutElement>();
			}

			ApplyShortcutLayout(layoutElement);
		}
	}

	private void ApplySerializedState()
	{
		if (TryGetComponent(out HorizontalLayoutGroup layoutGroup))
		{
			layoutGroup.spacing = slotSpacing;
		}
	}

	private void ApplyShortcutLayout(LayoutElement layoutElement)
	{
		if (layoutElement == null)
		{
			return;
		}

		layoutElement.minWidth = slotWidth;
		layoutElement.preferredWidth = slotWidth;
		layoutElement.flexibleWidth = 0f;
		layoutElement.minHeight = slotHeight;
		layoutElement.preferredHeight = slotHeight;
		layoutElement.flexibleHeight = 0f;
	}

	private void HandleShortcutClicked(int slotIndex, Ability ability)
	{
		int abilityIndex = (pageIndex * SlotCount) + slotIndex;
		AbilityClicked?.Invoke(abilityIndex, ability);
	}

	private void DestroyAllChildren()
	{
		for (int index = transform.childCount - 1; index >= 0; index--)
		{
			UiViewUtility.DestroyGeneratedObject(transform.GetChild(index).gameObject);
		}
	}

#if UNITY_EDITOR
	protected override void OnEditorRefresh()
	{
		EnsureShortcuts();
		RefreshBindings();
	}
#endif
}
