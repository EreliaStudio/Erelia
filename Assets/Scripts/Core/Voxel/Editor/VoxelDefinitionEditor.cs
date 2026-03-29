#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VoxelDefinition), true)]
public class VoxelDefinitionEditor : Editor
{
	private const float ShapePopupWidth = 140f;

	private enum ShapeSelection
	{
		Cube = 0,
		Slab = 1,
		Slope = 2,
		Stair = 3,
		CrossPlane = 4
	}

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

		EditorGUILayout.PropertyField(dataProp, true);

		object current = shapeProp.managedReferenceValue;
		bool hasKnownShape = TryGetShapeSelection(current?.GetType(), out ShapeSelection currentSelection);
		ShapeSelection displayedSelection = hasKnownShape ? currentSelection : ShapeSelection.Cube;

		ShapeSelection selectedShape = DrawShapeHeader(displayedSelection, out bool shapeTypeChanged);

		bool shapeReplaced = EnsureShapeInstance(current, selectedShape, shapeTypeChanged);
		if (shapeReplaced)
		{
			shapeProp.isExpanded = true;
			serializedObject.ApplyModifiedProperties();
			serializedObject.Update();
			RefreshProperties();

			current = shapeProp.managedReferenceValue;
			hasKnownShape = TryGetShapeSelection(current?.GetType(), out currentSelection);
		}

		if (!hasKnownShape && current != null)
		{
			EditorGUILayout.HelpBox($"Unknown shape type '{current.GetType().Name}'. Changing the selector will replace it.", MessageType.Info);
		}

		DrawShapeBody();

		serializedObject.ApplyModifiedProperties();

		if (target is VoxelDefinition definition)
		{
			definition.Initialize();
		}
	}

	private ShapeSelection DrawShapeHeader(ShapeSelection displayedSelection, out bool shapeTypeChanged)
	{
		Rect rowRect = EditorGUILayout.GetControlRect();
		Rect foldoutRect = rowRect;
		foldoutRect.width -= ShapePopupWidth + 4f;

		Rect popupRect = rowRect;
		popupRect.xMin = popupRect.xMax - ShapePopupWidth;

		shapeProp.isExpanded = EditorGUI.Foldout(foldoutRect, shapeProp.isExpanded, "Shape", true);

		EditorGUI.BeginChangeCheck();
		ShapeSelection selectedShape = (ShapeSelection)EditorGUI.EnumPopup(popupRect, displayedSelection);
		shapeTypeChanged = EditorGUI.EndChangeCheck();

		return selectedShape;
	}

	private void DrawShapeBody()
	{
		if (!shapeProp.isExpanded || shapeProp.managedReferenceValue == null)
		{
			return;
		}

		EditorGUI.indentLevel++;

		SerializedProperty iterator = shapeProp.Copy();
		SerializedProperty endProperty = iterator.GetEndProperty();
		bool enterChildren = true;

		while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
		{
			EditorGUILayout.PropertyField(iterator, true);
			enterChildren = false;
		}

		EditorGUI.indentLevel--;
	}

	private bool EnsureShapeInstance(object current, ShapeSelection selectedShape, bool forceReplace)
	{
		if (current == null || forceReplace)
		{
			shapeProp.managedReferenceValue = CreateShapeInstance(selectedShape);
			return true;
		}

		return false;
	}

	private static bool TryGetShapeSelection(System.Type shapeType, out ShapeSelection selection)
	{
		if (shapeType == typeof(VoxelCubeShape))
		{
			selection = ShapeSelection.Cube;
			return true;
		}

		if (shapeType == typeof(VoxelSlabShape))
		{
			selection = ShapeSelection.Slab;
			return true;
		}

		if (shapeType == typeof(VoxelSlopeShape))
		{
			selection = ShapeSelection.Slope;
			return true;
		}

		if (shapeType == typeof(VoxelStairShape))
		{
			selection = ShapeSelection.Stair;
			return true;
		}

		if (shapeType == typeof(VoxelCrossPlaneShape))
		{
			selection = ShapeSelection.CrossPlane;
			return true;
		}

		selection = ShapeSelection.Cube;
		return false;
	}

	private static VoxelShape CreateShapeInstance(ShapeSelection shapeType)
	{
		switch (shapeType)
		{
			case ShapeSelection.Slab:
				return new VoxelSlabShape();
			case ShapeSelection.Slope:
				return new VoxelSlopeShape();
			case ShapeSelection.Stair:
				return new VoxelStairShape();
			case ShapeSelection.CrossPlane:
				return new VoxelCrossPlaneShape();
			case ShapeSelection.Cube:
			default:
				return new VoxelCubeShape();
		}
	}
}
#endif
