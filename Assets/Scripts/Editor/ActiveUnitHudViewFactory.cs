#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ActiveUnitHudViewFactory
{
	private const string MenuPath = "GameObject/UI/Erelia/Active Unit HUD";

	private static readonly Vector2 Size = new Vector2(
		ActiveUnitHudView.BarWidth * 3 + ActiveUnitHudView.BarSpacing * 2,
		ActiveUnitHudView.BarHeight);

	[MenuItem(MenuPath, false, 2038)]
	private static void CreateActiveUnitHud(MenuCommand menuCommand)
	{
		GameObject hud = EditorUiFactoryUtility.CreateUiObject<ActiveUnitHudView>("Active Unit HUD", menuCommand, Size);
		hud.GetComponent<ActiveUnitHudView>().RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(hud);
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateCreate() => true;
}
#endif
