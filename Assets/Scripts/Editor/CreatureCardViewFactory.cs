#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreatureCardViewFactory
{
	private const string MenuPath = "GameObject/UI/Creature Card";

	[MenuItem(MenuPath, false, 2032)]
	private static void CreateCreatureCard(MenuCommand menuCommand)
	{
		GameObject creatureCard = EditorUiFactoryUtility.CreateUiObject<CreatureCardView>("Creature Card", menuCommand, new Vector2(320f, 80f));
		EditorUiFactoryUtility.SelectAndMarkDirty(creatureCard);
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateCreateCreatureCard() => true;
}
#endif
