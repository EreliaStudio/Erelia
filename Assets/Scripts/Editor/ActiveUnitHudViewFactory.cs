#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ActiveUnitHudViewFactory
{
	private const string MenuPath = "GameObject/UI/Erelia/Active Unit HUD";
	private const float DefaultBarWidth = 120f;
	private const float DefaultBarHeight = 20f;
	private const float DefaultBarSpacing = 6f;

	private static readonly Vector2 Size = new Vector2(
		DefaultBarWidth * 3 + DefaultBarSpacing * 2,
		DefaultBarHeight);

	[MenuItem(MenuPath, false, 2038)]
	private static void CreateActiveUnitHud(MenuCommand menuCommand)
	{
		GameObject hud = EditorUiFactoryUtility.CreateUiObject<ActiveUnitHudView>("Active Unit HUD", menuCommand, Size);
		ActiveUnitHudView view = hud.GetComponent<ActiveUnitHudView>();
		view.ConfigureDefaultLayout(DefaultBarWidth, DefaultBarHeight, DefaultBarSpacing);
		view.RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(hud);
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateCreate() => true;
}
#endif
