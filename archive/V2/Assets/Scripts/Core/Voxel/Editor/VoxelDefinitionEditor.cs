#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelDefinition), true)]
public class VoxelDefinitionEditor : Editor
{
	private static readonly Comparison<Type> ShapeTypeComparison =
		(left, right) => string.CompareOrdinal(GetShapeDisplayName(left), GetShapeDisplayName(right));

	private SerializedProperty dataProp;
	private SerializedProperty shapeProp;

	private void OnEnable()
	{
		RefreshProperties();
	}

	private void RefreshProperties()
	{
		dataProp = serializedObject.FindProperty("data");
		shapeProp = serializedObject.FindProperty("shape");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawSectionBody("Data", dataProp);

		object current = shapeProp.managedReferenceValue;
		Type[] shapeTypes = ManagedReferenceTypePicker.GetConcreteTypes(typeof(VoxelShape), ShapeTypeComparison);
		int currentIndex = ManagedReferenceTypePicker.GetSelectionIndex(current?.GetType(), shapeTypes, includeNullOption: false);
		int displayedIndex = currentIndex >= 0 ? currentIndex : 0;

		int selectedIndex = DrawShapeHeader(shapeTypes, displayedIndex, out bool shapeTypeChanged);
		Type selectedShapeType = ManagedReferenceTypePicker.GetTypeForSelection(selectedIndex, shapeTypes, includeNullOption: false);

		bool hasKnownShape = currentIndex >= 0;
		bool shapeReplaced = EnsureShapeInstance(current, selectedShapeType, shapeTypeChanged);
		if (shapeReplaced)
		{
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			RefreshProperties();

			current = shapeProp.managedReferenceValue;
			hasKnownShape = ManagedReferenceTypePicker.GetSelectionIndex(current?.GetType(), shapeTypes, includeNullOption: false) >= 0;
		}

		if (!hasKnownShape && current != null)
		{
			EditorGUILayout.HelpBox($"Unknown shape type '{current.GetType().Name}'. Changing the selector will replace it.", MessageType.Info);
		}

		DrawPropertyChildren(shapeProp);

		serializedObject.ApplyModifiedProperties();

		if (target is VoxelDefinition definition)
		{
			definition.Initialize();
		}
	}

	private int DrawShapeHeader(Type[] shapeTypes, int displayedIndex, out bool shapeTypeChanged)
	{
		Rect rowRect = EditorGUILayout.GetControlRect();
		Rect popupRect = EditorGUI.PrefixLabel(rowRect, new GUIContent("Shape"));
		string[] options = ManagedReferenceTypePicker.BuildDisplayNames(shapeTypes, GetShapeDisplayName, includeNullOption: false);

		EditorGUI.BeginChangeCheck();
		int selectedIndex = EditorGUI.Popup(popupRect, displayedIndex, options);
		shapeTypeChanged = EditorGUI.EndChangeCheck();

		return selectedIndex;
	}

	private void DrawSectionBody(string label, SerializedProperty property)
	{
		EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
		DrawPropertyChildren(property);
	}

	private void DrawPropertyChildren(SerializedProperty property)
	{
		if (property == null)
		{
			return;
		}

		EditorGUI.indentLevel++;

		SerializedProperty iterator = property.Copy();
		SerializedProperty endProperty = iterator.GetEndProperty();
		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
		{
			EditorGUILayout.PropertyField(iterator, true);
			enterChildren = false;
		}

		EditorGUI.indentLevel--;
	}

	private bool EnsureShapeInstance(object current, Type selectedShapeType, bool forceReplace)
	{
		if (current == null || forceReplace)
		{
			shapeProp.managedReferenceValue = ManagedReferenceTypePicker.CreateInstance(selectedShapeType);
			return true;
		}

		return false;
	}

	private static string GetShapeDisplayName(Type shapeType)
	{
		return ManagedReferenceTypePicker.NicifyTypeName(shapeType, prefixToTrim: "Voxel", suffixToTrim: "Shape");
	}
}
#endif
