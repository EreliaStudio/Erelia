using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(HorizontalLayoutGroup))]
[ExecuteAlways]
public sealed class ActiveUnitHudView : ExecuteAlwaysView
{
	public const float BarWidth = 120f;
	public const float BarHeight = 20f;
	public const float BarSpacing = 6f;

	[SerializeField] private ProgressBarView healthBar;
	[SerializeField] private ProgressBarView actionPointsBar;
	[SerializeField] private ProgressBarView movementPointsBar;
	[SerializeField] private Button endTurnButton;

	private BattleUnit boundUnit;
	private ObservableResource subscribedHealth;
	private ObservableResource subscribedActionPoints;
	private ObservableResource subscribedMovementPoints;

	public event Action EndTurnClicked;

	private void Reset()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void Awake()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void OnEnable()
	{
		EnsureHierarchy(true);
		ApplySerializedState();

		if (endTurnButton != null)
		{
			endTurnButton.onClick.AddListener(HandleEndTurnClicked);
		}

		SubscribeToUnit();
	}

	private void OnDisable()
	{
		if (endTurnButton != null)
		{
			endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
		}

		UnsubscribeFromUnit();
	}

	private void OnDestroy()
	{
		UnsubscribeFromUnit();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
		EnsureHierarchy(true);
		ApplySerializedState();
#endif
	}

	public void Bind(BattleUnit unit)
	{
		if (ReferenceEquals(boundUnit, unit))
		{
			return;
		}

		UnsubscribeFromUnit();
		boundUnit = unit;
		SubscribeToUnit();
		RefreshAll();
	}

	[ContextMenu("Refresh")]
	public void RefreshNow()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void EnsureHierarchy(bool allowCreate)
	{
		healthBar = ResolveBar("Health Bar", healthBar, allowCreate);
		actionPointsBar = ResolveBar("Action Points Bar", actionPointsBar, allowCreate);
		movementPointsBar = ResolveBar("Movement Points Bar", movementPointsBar, allowCreate);
	}

	private ProgressBarView ResolveBar(string childName, ProgressBarView existing, bool allowCreate)
	{
		if (existing != null)
		{
			return existing;
		}

		Transform child = transform.Find(childName);
		if (child != null && child.TryGetComponent(out ProgressBarView found))
		{
			found.RefreshNow();
			return found;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = new GameObject(childName, typeof(RectTransform), typeof(LayoutElement));
		childObject.transform.SetParent(transform, false);

		LayoutElement layout = childObject.GetComponent<LayoutElement>();
		layout.minWidth = BarWidth;
		layout.preferredWidth = BarWidth;
		layout.flexibleWidth = 0f;

		layout.minHeight = BarHeight;
		layout.preferredHeight = BarHeight;
		layout.flexibleHeight = 0f;

		ProgressBarView bar = childObject.AddComponent<ProgressBarView>();
		bar.RefreshNow();
		return bar;
	}

	private void ApplySerializedState()
	{
		if (!TryGetComponent(out HorizontalLayoutGroup layoutGroup))
		{
			return;
		}

		layoutGroup.spacing = BarSpacing;
		layoutGroup.childAlignment = TextAnchor.MiddleLeft;

		layoutGroup.childControlWidth = true;
		layoutGroup.childControlHeight = true;

		layoutGroup.childForceExpandWidth = true;
		layoutGroup.childForceExpandHeight = true;
	}

	private void SubscribeToUnit()
	{
		if (!isActiveAndEnabled || boundUnit?.BattleAttributes == null)
		{
			return;
		}

		subscribedHealth = boundUnit.BattleAttributes.Health;
		subscribedHealth.Changed += HandleHealthChanged;

		subscribedActionPoints = boundUnit.BattleAttributes.ActionPoints;
		subscribedActionPoints.Changed += HandleActionPointsChanged;

		subscribedMovementPoints = boundUnit.BattleAttributes.MovementPoints;
		subscribedMovementPoints.Changed += HandleMovementPointsChanged;
	}

	private void UnsubscribeFromUnit()
	{
		if (subscribedHealth != null)
		{
			subscribedHealth.Changed -= HandleHealthChanged;
			subscribedHealth = null;
		}

		if (subscribedActionPoints != null)
		{
			subscribedActionPoints.Changed -= HandleActionPointsChanged;
			subscribedActionPoints = null;
		}

		if (subscribedMovementPoints != null)
		{
			subscribedMovementPoints.Changed -= HandleMovementPointsChanged;
			subscribedMovementPoints = null;
		}
	}

	private void HandleHealthChanged(ObservableResource resource) => RefreshBar(healthBar, resource);
	private void HandleActionPointsChanged(ObservableResource resource) => RefreshBar(actionPointsBar, resource);
	private void HandleMovementPointsChanged(ObservableResource resource) => RefreshBar(movementPointsBar, resource);
	private void HandleEndTurnClicked() => EndTurnClicked?.Invoke();

	private void RefreshAll()
	{
		RefreshBar(healthBar, boundUnit?.BattleAttributes?.Health);
		RefreshBar(actionPointsBar, boundUnit?.BattleAttributes?.ActionPoints);
		RefreshBar(movementPointsBar, boundUnit?.BattleAttributes?.MovementPoints);
	}

	private static void RefreshBar(ProgressBarView bar, ObservableResource resource)
	{
		if (bar == null)
		{
			return;
		}

		bar.SetValues(resource?.Current ?? 0, resource?.Max ?? 0);
	}

#if UNITY_EDITOR
	protected override void OnEditorRefresh()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
		UnsubscribeFromUnit();
		SubscribeToUnit();
		RefreshAll();
	}
#endif
}
