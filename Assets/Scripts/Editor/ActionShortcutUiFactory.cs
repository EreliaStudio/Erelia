#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ActionShortcutUiFactory
{
	private const string AbilityShortcutMenuPath = "GameObject/UI/Erelia/Ability Shortcut";
	private const string AbilityShortcutBarMenuPath = "GameObject/UI/Erelia/Ability Shortcut Bar";
	private const string ShortcutBarPageSelectorMenuPath = "GameObject/UI/Erelia/Shortcut Bar Page Selector";
	private const string ActionShortcutBarMenuPath = "GameObject/UI/Erelia/Action Shortcut Bar";
	private const float DefaultShortcutWidth = 56f;
	private const float DefaultShortcutHeight = 48f;
	private const float DefaultShortcutSpacing = 4f;
	private const float DefaultPageSelectorWidth = 26f;
	private const float DefaultPageSelectorButtonHeight = 14f;
	private const float DefaultPageSelectorLabelHeight = 14f;
	private const float DefaultPageSelectorVerticalPadding = 1f;
	private static readonly Vector2 DefaultShortcutFramePadding = new Vector2(2f, 2f);
	private static float DefaultShortcutBarWidth => (DefaultShortcutWidth * AbilityShortcutBarView.SlotCount) + (DefaultShortcutSpacing * (AbilityShortcutBarView.SlotCount - 1));

	[MenuItem(AbilityShortcutMenuPath, false, 2034)]
	private static void CreateAbilityShortcut(MenuCommand menuCommand)
	{
		GameObject abilityShortcut = EditorUiFactoryUtility.CreateUiObject<AbilityShortcutView>("Ability Shortcut", menuCommand, new Vector2(DefaultShortcutWidth, DefaultShortcutHeight));
		AbilityShortcutView view = abilityShortcut.GetComponent<AbilityShortcutView>();
		view.ConfigureDefaultLayout(6f, DefaultShortcutFramePadding);
		view.RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(abilityShortcut);
	}

	[MenuItem(AbilityShortcutBarMenuPath, false, 2035)]
	private static void CreateAbilityShortcutBar(MenuCommand menuCommand)
	{
		GameObject abilityShortcutBar = EditorUiFactoryUtility.CreateUiObject<AbilityShortcutBarView>("Ability Shortcut Bar", menuCommand, new Vector2(DefaultShortcutBarWidth, DefaultShortcutHeight));
		AbilityShortcutBarView view = abilityShortcutBar.GetComponent<AbilityShortcutBarView>();
		view.ConfigureDefaultLayout(DefaultShortcutWidth, DefaultShortcutHeight, DefaultShortcutSpacing);
		view.RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(abilityShortcutBar);
	}

	[MenuItem(ShortcutBarPageSelectorMenuPath, false, 2036)]
	private static void CreateShortcutBarPageSelector(MenuCommand menuCommand)
	{
		GameObject pageSelector = EditorUiFactoryUtility.CreateUiObject<ShortcutBarPageSelectorView>("Shortcut Bar Page Selector", menuCommand, new Vector2(DefaultPageSelectorWidth, DefaultShortcutHeight));
		ShortcutBarPageSelectorView view = pageSelector.GetComponent<ShortcutBarPageSelectorView>();
		view.ConfigureDefaultLayout(DefaultPageSelectorWidth, DefaultPageSelectorButtonHeight, DefaultPageSelectorLabelHeight, DefaultPageSelectorVerticalPadding);
		view.RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(pageSelector);
	}

	[MenuItem(ActionShortcutBarMenuPath, false, 2037)]
	private static void CreateActionShortcutBar(MenuCommand menuCommand)
	{
		GameObject actionShortcutBar = EditorUiFactoryUtility.CreateUiObject<ActionShortcutBarView>("Action Shortcut Bar", menuCommand, new Vector2(DefaultShortcutBarWidth + 34f, DefaultShortcutHeight));
		ActionShortcutBarView view = actionShortcutBar.GetComponent<ActionShortcutBarView>();
		view.ConfigureDefaultLayout(DefaultShortcutBarWidth, DefaultPageSelectorWidth, DefaultShortcutHeight);
		view.RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(actionShortcutBar);
	}

	[MenuItem(AbilityShortcutMenuPath, true)]
	[MenuItem(AbilityShortcutBarMenuPath, true)]
	[MenuItem(ShortcutBarPageSelectorMenuPath, true)]
	[MenuItem(ActionShortcutBarMenuPath, true)]
	private static bool ValidateCreateUi() => true;
}
#endif
