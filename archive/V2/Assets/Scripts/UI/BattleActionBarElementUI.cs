using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class BattleActionBarElementUI : MonoBehaviour
{
	[Header("Active Creature")]
	[SerializeField] private CreatureCardHeaderElementUI activeCreatureHeaderElementUI;
	[SerializeField] private StatusListElementUI statusListElementUI;
	[SerializeField] private ObservableResourceBarElementUI healthBarElementUI;
	[SerializeField] private ObservableResourceBarElementUI actionPointsBarElementUI;
	[SerializeField] private ObservableResourceBarElementUI movementPointsBarElementUI;

	[Header("Actions")]
	[SerializeField] private AbilityListElementUI abilityListElementUI;
	[SerializeField] private ActionInfoElementUI actionInfoElementUI;
	[SerializeField] private bool displayAbilityShortcuts = true;
	[SerializeField] private int defaultFocusedAbilityIndex = 0;

	[Header("Controls")]
	[SerializeField] private Button endTurnButton;
	[SerializeField] private UnityEvent endTurnRequested;

	private BattleUnit linkedBattleUnit;
	private int previewedAbilityIndex = -1;

	private void Awake()
	{
		AutoResolveReferences();
		RegisterEvents();
		Refresh();
	}

	private void OnValidate()
	{
		AutoResolveReferences();
	}

	private void OnDestroy()
	{
		UnregisterEvents();
		UnsubscribeFromBattleUnit(linkedBattleUnit);
	}

	[ContextMenu("Auto Resolve")]
	public void AutoResolveReferences()
	{
		activeCreatureHeaderElementUI ??= GetComponentInChildren<CreatureCardHeaderElementUI>(true);
		statusListElementUI ??= FindComponentByName<StatusListElementUI>("status");
		abilityListElementUI ??= GetComponentInChildren<AbilityListElementUI>(true);
		actionInfoElementUI ??= GetComponentInChildren<ActionInfoElementUI>(true);
		endTurnButton ??= FindComponentByName<Button>("end");

		ObservableResourceBarElementUI[] bars = GetComponentsInChildren<ObservableResourceBarElementUI>(true);
		healthBarElementUI ??= FindComponentByName(bars, "health", "hp");
		actionPointsBarElementUI ??= FindComponentByName(bars, "action", "ap");
		movementPointsBarElementUI ??= FindComponentByName(bars, "movement", "move", "mp");
	}

	public void Bind(BattleUnit p_battleUnit)
	{
		if (!ReferenceEquals(linkedBattleUnit, p_battleUnit))
		{
			UnsubscribeFromBattleUnit(linkedBattleUnit);
			linkedBattleUnit = p_battleUnit;
			SubscribeToBattleUnit(linkedBattleUnit);
		}

		previewedAbilityIndex = -1;
		Refresh();
	}

	public void Clear()
	{
		Bind(null);
	}

	public void PreviewAbilityAt(int p_index)
	{
		if (!TryGetAbilityAt(p_index, out Ability ability))
		{
			previewedAbilityIndex = -1;
			ShowDefaultAbilityDetails();
			return;
		}

		previewedAbilityIndex = p_index;
		actionInfoElementUI?.Bind(ability);
	}

	private void Refresh()
	{
		activeCreatureHeaderElementUI?.Bind(linkedBattleUnit);
		statusListElementUI?.Bind(linkedBattleUnit?.Statuses);
		healthBarElementUI?.SubscribeTo(linkedBattleUnit?.BattleAttributes?.Health);
		actionPointsBarElementUI?.SubscribeTo(linkedBattleUnit?.BattleAttributes?.ActionPoints);
		movementPointsBarElementUI?.SubscribeTo(linkedBattleUnit?.BattleAttributes?.MovementPoints);

		if (abilityListElementUI != null)
		{
			abilityListElementUI.SetDisplayShortcutLabels(displayAbilityShortcuts);
			abilityListElementUI.Bind(linkedBattleUnit?.Abilities);
		}

		if (linkedBattleUnit == null)
		{
			actionInfoElementUI?.Clear();
		}
		else
		{
			ShowDefaultAbilityDetails();
		}

		if (endTurnButton != null)
		{
			endTurnButton.interactable = linkedBattleUnit != null;
		}
	}

	private void RegisterEvents()
	{
		if (abilityListElementUI != null)
		{
			abilityListElementUI.AbilityHovered -= HandleAbilityHovered;
			abilityListElementUI.AbilityHoverEnded -= HandleAbilityHoverEnded;
			abilityListElementUI.AbilityHovered += HandleAbilityHovered;
			abilityListElementUI.AbilityHoverEnded += HandleAbilityHoverEnded;
		}

		if (statusListElementUI != null)
		{
			statusListElementUI.StatusHovered -= HandleStatusHovered;
			statusListElementUI.StatusHoverEnded -= HandleStatusHoverEnded;
			statusListElementUI.StatusHovered += HandleStatusHovered;
			statusListElementUI.StatusHoverEnded += HandleStatusHoverEnded;
		}

		if (endTurnButton != null)
		{
			endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
			endTurnButton.onClick.AddListener(HandleEndTurnClicked);
		}
	}

	private void UnregisterEvents()
	{
		if (abilityListElementUI != null)
		{
			abilityListElementUI.AbilityHovered -= HandleAbilityHovered;
			abilityListElementUI.AbilityHoverEnded -= HandleAbilityHoverEnded;
		}

		if (statusListElementUI != null)
		{
			statusListElementUI.StatusHovered -= HandleStatusHovered;
			statusListElementUI.StatusHoverEnded -= HandleStatusHoverEnded;
		}

		if (endTurnButton != null)
		{
			endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
		}
	}

	private void SubscribeToBattleUnit(BattleUnit p_battleUnit)
	{
		if (p_battleUnit == null)
		{
			return;
		}

		ObservableValue<BattleAttributes> battleAttributes = p_battleUnit.BattleAttributes;
		if (battleAttributes != null)
		{
			battleAttributes.Changed += HandleBattleAttributesChanged;
		}

		BattleStatuses statuses = p_battleUnit.Statuses;
		if (statuses != null)
		{
			statuses.Changed += HandleStatusesChanged;
		}
	}

	private void UnsubscribeFromBattleUnit(BattleUnit p_battleUnit)
	{
		if (p_battleUnit == null)
		{
			return;
		}

		ObservableValue<BattleAttributes> battleAttributes = p_battleUnit.BattleAttributes;
		if (battleAttributes != null)
		{
			battleAttributes.Changed -= HandleBattleAttributesChanged;
		}

		BattleStatuses statuses = p_battleUnit.Statuses;
		if (statuses != null)
		{
			statuses.Changed -= HandleStatusesChanged;
		}
	}

	private void ShowDefaultAbilityDetails()
	{
		if (previewedAbilityIndex >= 0)
		{
			PreviewAbilityAt(previewedAbilityIndex);
			return;
		}

		if (!TryGetAbilityAt(defaultFocusedAbilityIndex, out Ability defaultAbility))
		{
			defaultAbility = GetFirstAvailableAbility();
		}

		if (defaultAbility == null)
		{
			actionInfoElementUI?.Clear();
			return;
		}

		actionInfoElementUI?.Bind(defaultAbility);
	}

	private Ability GetFirstAvailableAbility()
	{
		IReadOnlyList<Ability> abilities = linkedBattleUnit?.Abilities;
		if (abilities == null)
		{
			return null;
		}

		for (int index = 0; index < abilities.Count; index++)
		{
			if (abilities[index] != null)
			{
				return abilities[index];
			}
		}

		return null;
	}

	private bool TryGetAbilityAt(int p_index, out Ability p_ability)
	{
		IReadOnlyList<Ability> abilities = linkedBattleUnit?.Abilities;
		if (abilities != null && p_index >= 0 && p_index < abilities.Count)
		{
			p_ability = abilities[p_index];
			return p_ability != null;
		}

		p_ability = null;
		return false;
	}

	private void HandleAbilityHovered(Ability p_ability, int p_index)
	{
		previewedAbilityIndex = p_index;
		actionInfoElementUI?.Bind(p_ability);
	}

	private void HandleAbilityHoverEnded()
	{
		previewedAbilityIndex = -1;
		ShowDefaultAbilityDetails();
	}

	private void HandleStatusHovered(BattleStatus p_status)
	{
		actionInfoElementUI?.Bind(p_status);
	}

	private void HandleStatusHoverEnded()
	{
		ShowDefaultAbilityDetails();
	}

	private void HandleBattleAttributesChanged(BattleAttributes p_battleAttributes)
	{
		activeCreatureHeaderElementUI?.Bind(linkedBattleUnit);
	}

	private void HandleStatusesChanged(BattleStatuses p_statuses)
	{
		statusListElementUI?.Bind(p_statuses);
	}

	private void HandleEndTurnClicked()
	{
		endTurnRequested?.Invoke();
	}

	private T FindComponentByName<T>(string p_token) where T : Component
	{
		if (string.IsNullOrWhiteSpace(p_token))
		{
			return null;
		}

		return FindComponentByName(GetComponentsInChildren<T>(true), p_token);
	}

	private static T FindComponentByName<T>(IReadOnlyList<T> p_components, params string[] p_tokens) where T : Component
	{
		for (int index = 0; index < p_components.Count; index++)
		{
			T component = p_components[index];
			if (component == null)
			{
				continue;
			}

			string componentName = component.name.ToLowerInvariant();
			for (int tokenIndex = 0; tokenIndex < p_tokens.Length; tokenIndex++)
			{
				if (componentName.Contains(p_tokens[tokenIndex].ToLowerInvariant()))
				{
					return component;
				}
			}
		}

		return null;
	}
}
