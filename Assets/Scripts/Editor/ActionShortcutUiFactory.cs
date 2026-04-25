#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ActionShortcutUiFactory
{
	private const string AbilityShortcutMenuPath = "GameObject/UI/Erelia/Ability Shortcut";
	private const string AbilityShortcutBarMenuPath = "GameObject/UI/Erelia/Ability Shortcut Bar";
	private const string ShortcutBarPageSelectorMenuPath = "GameObject/UI/Erelia/Shortcut Bar Page Selector";
	private const string ActionShortcutBarMenuPath = "GameObject/UI/Erelia/Action Shortcut Bar";

	[MenuItem(AbilityShortcutMenuPath, false, 2034)]
	private static void CreateAbilityShortcut(MenuCommand menuCommand)
	{
		GameObject abilityShortcut = EditorUiFactoryUtility.CreateUiObject<AbilityShortcutView>("Ability Shortcut", menuCommand, new Vector2(56f, 48f));
		abilityShortcut.GetComponent<AbilityShortcutView>().RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(abilityShortcut);
	}

	[MenuItem(AbilityShortcutBarMenuPath, false, 2035)]
	private static void CreateAbilityShortcutBar(MenuCommand menuCommand)
	{
		GameObject abilityShortcutBar = EditorUiFactoryUtility.CreateUiObject<AbilityShortcutBarView>("Ability Shortcut Bar", menuCommand, new Vector2(AbilityShortcutBarView.PreferredWidth, AbilityShortcutBarView.SlotHeight));
		abilityShortcutBar.GetComponent<AbilityShortcutBarView>().RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(abilityShortcutBar);
	}

	[MenuItem(ShortcutBarPageSelectorMenuPath, false, 2036)]
	private static void CreateShortcutBarPageSelector(MenuCommand menuCommand)
	{
		GameObject pageSelector = EditorUiFactoryUtility.CreateUiObject<ShortcutBarPageSelectorView>("Shortcut Bar Page Selector", menuCommand, new Vector2(26f, AbilityShortcutBarView.SlotHeight));
		pageSelector.GetComponent<ShortcutBarPageSelectorView>().RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(pageSelector);
	}

	[MenuItem(ActionShortcutBarMenuPath, false, 2037)]
	private static void CreateActionShortcutBar(MenuCommand menuCommand)
	{
		GameObject actionShortcutBar = EditorUiFactoryUtility.CreateUiObject<ActionShortcutBarView>("Action Shortcut Bar", menuCommand, new Vector2(AbilityShortcutBarView.PreferredWidth + 34f, AbilityShortcutBarView.SlotHeight));
		actionShortcutBar.GetComponent<ActionShortcutBarView>().RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(actionShortcutBar);
	}

	[MenuItem(AbilityShortcutMenuPath, true)]
	[MenuItem(AbilityShortcutBarMenuPath, true)]
	[MenuItem(ShortcutBarPageSelectorMenuPath, true)]
	[MenuItem(ActionShortcutBarMenuPath, true)]
	private static bool ValidateCreateUi() => true;
}
#endif
