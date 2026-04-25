#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreatureTeamViewFactory
{
	private const string MenuPath = "GameObject/UI/Creature Team";

	[MenuItem(MenuPath, false, 2033)]
	private static void CreateCreatureTeam(MenuCommand menuCommand)
	{
		GameObject creatureTeam = EditorUiFactoryUtility.CreateUiObject<CreatureTeamView>("Creature Team", menuCommand, new Vector2(320f, 520f));
		EditorUiFactoryUtility.SelectAndMarkDirty(creatureTeam);
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateCreateCreatureTeam() => true;
}
#endif
