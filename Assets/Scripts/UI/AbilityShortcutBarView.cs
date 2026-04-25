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
	public const float SlotWidth = 56f;
	public const float SlotHeight = 48f;
	public const float SlotSpacing = 4f;
	public const float PreferredWidth = (SlotWidth * SlotCount) + (SlotSpacing * (SlotCount - 1));

	[SerializeField] private AbilityShortcutView shortcutPrefab;

	private readonly List<AbilityShortcutView> shortcutInstances = new();
	private IReadOnlyList<Ability> boundAbilities;
	private int pageIndex;
	private UnityEngine.Object lastPrefabReference;

	public event Action<int, Ability> AbilityClicked;

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
		RefreshBindings();
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

	private static void ApplyShortcutLayout(LayoutElement layoutElement)
	{
		if (layoutElement == null)
		{
			return;
		}

		layoutElement.minWidth = SlotWidth;
		layoutElement.preferredWidth = SlotWidth;
		layoutElement.flexibleWidth = 0f;
		layoutElement.minHeight = SlotHeight;
		layoutElement.preferredHeight = SlotHeight;
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
