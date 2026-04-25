#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ProgressBarViewFactory
{
	private const string MenuPath = "GameObject/UI/Progress Bar";

	[MenuItem(MenuPath, false, 2031)]
	private static void CreateProgressBar(MenuCommand menuCommand)
	{
		GameObject progressBar = EditorUiFactoryUtility.CreateUiObject<ProgressBarView>("Progress Bar", menuCommand, new Vector2(260f, 28f));
		progressBar.GetComponent<ProgressBarView>().RefreshNow();
		EditorUiFactoryUtility.SelectAndMarkDirty(progressBar);
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateCreateProgressBar() => true;
}
#endif
