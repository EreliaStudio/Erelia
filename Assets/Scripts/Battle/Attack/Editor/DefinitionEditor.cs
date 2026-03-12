using UnityEditor;
using UnityEngine;

namespace Erelia.Battle.Attack.Editor
{
	[CustomEditor(typeof(Erelia.Battle.Attack.Definition), true)]
	public sealed class DefinitionEditor : UnityEditor.Editor
	{
		private SerializedProperty effectsProp;

		private void OnEnable()
		{
			effectsProp = serializedObject.FindProperty("effects");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			DrawPropertiesExcluding(serializedObject, "m_Script", "effects");

			EditorGUILayout.Space();
			DrawEffects();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawEffects()
		{
			EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

			if (effectsProp.arraySize == 0)
			{
				EditorGUILayout.HelpBox(
					"This attack has no effects configured.",
					MessageType.Info);
			}

			for (int index = 0; index < effectsProp.arraySize; index += 1)
			{
				SerializedProperty effectProp = effectsProp.GetArrayElementAtIndex(index);
				if (EnsureEffectInstance(effectProp))
				{
					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
					return;
				}

				EditorGUILayout.BeginVertical(EditorStyles.helpBox);

				EditorGUILayout.BeginHorizontal();

				Erelia.Battle.Attack.Effect.Kind currentKind = ResolveKind(effectProp);
				EditorGUI.BeginChangeCheck();
				var nextKind = (Erelia.Battle.Attack.Effect.Kind)EditorGUILayout.EnumPopup(
					"Effect",
					currentKind);
				if (EditorGUI.EndChangeCheck())
				{
					effectProp.managedReferenceValue = CreateEffect(nextKind);
					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
					return;
				}

				if (GUILayout.Button("Remove", GUILayout.Width(70f)))
				{
					effectsProp.DeleteArrayElementAtIndex(index);
					serializedObject.ApplyModifiedProperties();
					serializedObject.Update();
					return;
				}

				EditorGUILayout.EndHorizontal();

				DrawEffectFields(effectProp);

				EditorGUILayout.EndVertical();
			}

			if (GUILayout.Button("Add Effect"))
			{
				int newIndex = effectsProp.arraySize;
				effectsProp.arraySize += 1;
				effectsProp.GetArrayElementAtIndex(newIndex).managedReferenceValue =
					CreateEffect(Erelia.Battle.Attack.Effect.Kind.HealthModification);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}
		}

		private static bool EnsureEffectInstance(SerializedProperty effectProp)
		{
			if (effectProp.managedReferenceValue != null)
			{
				return false;
			}

			effectProp.managedReferenceValue =
				CreateEffect(Erelia.Battle.Attack.Effect.Kind.HealthModification);
			return true;
		}

		private static void DrawEffectFields(SerializedProperty effectProp)
		{
			using (new EditorGUI.IndentLevelScope())
			{
				SerializedProperty child = effectProp.Copy();
				SerializedProperty end = child.GetEndProperty();
				bool enterChildren = true;

				while (child.NextVisible(enterChildren) &&
					!SerializedProperty.EqualContents(child, end))
				{
					EditorGUILayout.PropertyField(child, true);
					enterChildren = false;
				}
			}
		}

		private static Erelia.Battle.Attack.Effect.Kind ResolveKind(SerializedProperty effectProp)
		{
			object effect = effectProp.managedReferenceValue;
			if (effect is Erelia.Battle.Attack.Effect.HealthModification)
			{
				return Erelia.Battle.Attack.Effect.Kind.HealthModification;
			}

			return Erelia.Battle.Attack.Effect.Kind.HealthModification;
		}

		private static Erelia.Battle.Attack.Effect.Definition CreateEffect(
			Erelia.Battle.Attack.Effect.Kind kind)
		{
			switch (kind)
			{
				case Erelia.Battle.Attack.Effect.Kind.HealthModification:
				default:
					return new Erelia.Battle.Attack.Effect.HealthModification();
			}
		}
	}
}
