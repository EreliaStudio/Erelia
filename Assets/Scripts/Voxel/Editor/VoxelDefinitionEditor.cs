using UnityEditor;
using UnityEngine;

namespace VoxelKit.Editor
{
	[CustomEditor(typeof(VoxelKit.Definition), true)]
	public class VoxelDefinitionEditor : UnityEditor.Editor
	{
		protected SerializedProperty dataProp;
		protected SerializedProperty shapeTypeProp;
		protected SerializedProperty shapeProp;

		protected virtual void OnEnable()
		{
			dataProp = serializedObject.FindProperty("data");
			shapeTypeProp = serializedObject.FindProperty("shapeType");
			shapeProp = serializedObject.FindProperty("shape");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(dataProp, true);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(shapeTypeProp);
			bool shapeTypeChanged = EditorGUI.EndChangeCheck();

			EnsureShapeInstance(shapeTypeChanged);
			AfterEnsureShapeInstance(shapeTypeChanged);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(shapeProp, true);

			DrawCustomFields();

			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void DrawCustomFields()
		{
		}

		protected virtual void AfterEnsureShapeInstance(bool shapeTypeChanged)
		{
		}

		protected void EnsureShapeInstance(bool forceReplace)
		{
			var shapeType = (VoxelKit.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetShapeClassType(shapeType);
			var current = shapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				shapeProp.managedReferenceValue = CreateShapeInstance(shapeType);
			}
		}

		protected virtual System.Type GetShapeClassType(VoxelKit.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case VoxelKit.Definition.ShapeType.Slab:
					return typeof(VoxelKit.ShapeType.Slab);
				case VoxelKit.Definition.ShapeType.Slope:
					return typeof(VoxelKit.ShapeType.Slope);
				case VoxelKit.Definition.ShapeType.Stair:
					return typeof(VoxelKit.ShapeType.Stair);
				case VoxelKit.Definition.ShapeType.CrossPlane:
					return typeof(VoxelKit.ShapeType.CrossPlane);
				case VoxelKit.Definition.ShapeType.Cube:
				default:
					return typeof(VoxelKit.ShapeType.Cube);
			}
		}

		protected virtual VoxelKit.Shape CreateShapeInstance(VoxelKit.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case VoxelKit.Definition.ShapeType.Slab:
					return new VoxelKit.ShapeType.Slab();
				case VoxelKit.Definition.ShapeType.Slope:
					return new VoxelKit.ShapeType.Slope();
				case VoxelKit.Definition.ShapeType.Stair:
					return new VoxelKit.ShapeType.Stair();
				case VoxelKit.Definition.ShapeType.CrossPlane:
					return new VoxelKit.ShapeType.CrossPlane();
				case VoxelKit.Definition.ShapeType.Cube:
				default:
					return new VoxelKit.ShapeType.Cube();
			}
		}
	}
}


