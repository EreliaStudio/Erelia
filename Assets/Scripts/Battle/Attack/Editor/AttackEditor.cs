using UnityEditor;
using UnityEngine;

namespace Erelia.Battle.Editor
{
	[CustomEditor(typeof(Erelia.Battle.Attack), true)]
	public sealed class AttackEditor : UnityEditor.Editor
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

				Erelia.Battle.Effects.Kind currentKind = ResolveKind(effectProp);
				EditorGUI.BeginChangeCheck();
				var nextKind = (Erelia.Battle.Effects.Kind)EditorGUILayout.EnumPopup(
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
					CreateEffect(Erelia.Battle.Effects.Kind.HealthModification);
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
				CreateEffect(Erelia.Battle.Effects.Kind.HealthModification);
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

		private static Erelia.Battle.Effects.Kind ResolveKind(SerializedProperty effectProp)
		{
			object effect = effectProp.managedReferenceValue;
			if (effect is Erelia.Battle.Effects.HealthModification)
			{
				return Erelia.Battle.Effects.Kind.HealthModification;
			}

			return Erelia.Battle.Effects.Kind.HealthModification;
		}

		private static Erelia.Battle.Effects.AttackEffect CreateEffect(
			Erelia.Battle.Effects.Kind kind)
		{
			switch (kind)
			{
				case Erelia.Battle.Effects.Kind.HealthModification:
				default:
					return new Erelia.Battle.Effects.HealthModification();
			}
		}
	}
}

