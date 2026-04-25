#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreatureCardViewFactory
{
	private const string MenuPath = "GameObject/UI/Creature Card";
	private static readonly Vector2 DefaultFramePadding = new Vector2(2f, 2f);
	private const float DefaultContentPadding = 8f;
	private const float DefaultStaminaBarHeight = 20f;

	[MenuItem(MenuPath, false, 2032)]
	private static void CreateCreatureCard(MenuCommand menuCommand)
	{
		GameObject creatureCard = EditorUiFactoryUtility.CreateUiObject<CreatureCardView>("Creature Card", menuCommand, new Vector2(320f, 80f));
		creatureCard.GetComponent<CreatureCardView>().ConfigureDefaultLayout(DefaultFramePadding, DefaultContentPadding, DefaultStaminaBarHeight);
		EditorUiFactoryUtility.SelectAndMarkDirty(creatureCard);
	}

	[MenuItem(MenuPath, true)]
	private static bool ValidateCreateCreatureCard() => true;
}
#endif
