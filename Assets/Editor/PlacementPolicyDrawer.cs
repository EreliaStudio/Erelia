using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Battle.Agent.Editor
{
	[CustomPropertyDrawer(typeof(Model.PlacementPolicyBase), true)]
	public class PlacementPolicyDrawer : PropertyDrawer
	{
		private static Type[] cachedTypes;
		private static string[] cachedTypeNames;

		private static void EnsureTypes()
		{
			if (cachedTypes != null && cachedTypeNames != null)
			{
				return;
			}

			var types = TypeCache.GetTypesDerivedFrom<Model.PlacementPolicyBase>()
				.Where(t => !t.IsAbstract)
				.ToArray();

			cachedTypes = types;
			cachedTypeNames = new string[types.Length + 1];
			cachedTypeNames[0] = "None";
			for (int i = 0; i < types.Length; i++)
			{
				cachedTypeNames[i + 1] = types[i].Name;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUIUtility.singleLineHeight;

			if (!property.isExpanded || property.managedReferenceValue == null)
			{
				return height;
			}

			var iterator = property.Copy();
			var end = iterator.GetEndProperty();
			bool enterChildren = true;

			while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
			{
				height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
				enterChildren = false;
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EnsureTypes();

			EditorGUI.BeginProperty(position, label, property);

			Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			Rect foldoutRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
			Rect popupRect = new Rect(line.x + EditorGUIUtility.labelWidth, line.y, line.width - EditorGUIUtility.labelWidth, line.height);

			property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

			int currentIndex = 0;
			var currentValue = property.managedReferenceValue;
			if (currentValue != null)
			{
				Type currentType = currentValue.GetType();
				for (int i = 0; i < cachedTypes.Length; i++)
				{
					if (cachedTypes[i] == currentType)
					{
						currentIndex = i + 1;
						break;
					}
				}
			}

			int selectedIndex = EditorGUI.Popup(popupRect, currentIndex, cachedTypeNames);
			if (selectedIndex != currentIndex)
			{
				if (selectedIndex == 0)
				{
					property.managedReferenceValue = null;
				}
				else
				{
					Type selectedType = cachedTypes[selectedIndex - 1];
					property.managedReferenceValue = Activator.CreateInstance(selectedType);
				}

				property.serializedObject.ApplyModifiedProperties();
			}

			if (property.isExpanded && property.managedReferenceValue != null)
			{
				var iterator = property.Copy();
				var end = iterator.GetEndProperty();
				bool enterChildren = true;
				float y = line.y + line.height + EditorGUIUtility.standardVerticalSpacing;

				while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
				{
					float height = EditorGUI.GetPropertyHeight(iterator, true);
					Rect fieldRect = new Rect(position.x, y, position.width, height);
					EditorGUI.PropertyField(fieldRect, iterator, true);
					y += height + EditorGUIUtility.standardVerticalSpacing;
					enterChildren = false;
				}
			}

			EditorGUI.EndProperty();
		}
	}
}
