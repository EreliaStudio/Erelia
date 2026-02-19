using UnityEditor;
using UnityEngine;

namespace Erelia.Editor
{
	[CustomEditor(typeof(Voxel.Definition))]
	public class VoxelDefinitionEditor : UnityEditor.Editor
	{
		private SerializedProperty dataProp;
		private SerializedProperty shapeTypeProp;
		private SerializedProperty shapeProp;

		private void OnEnable()
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

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(shapeProp, true);

			serializedObject.ApplyModifiedProperties();
		}

		private void EnsureShapeInstance(bool forceReplace)
		{
			var shapeType = (Voxel.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetShapeClassType(shapeType);
			var current = shapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				shapeProp.managedReferenceValue = CreateShapeInstance(shapeType);
			}
		}

		private static System.Type GetShapeClassType(Voxel.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Voxel.Definition.ShapeType.Slab:
					return typeof(Voxel.ShapeType.Slab);
				case Voxel.Definition.ShapeType.Slope:
					return typeof(Voxel.ShapeType.Slope);
				case Voxel.Definition.ShapeType.Stair:
					return typeof(Voxel.ShapeType.Stair);
				case Voxel.Definition.ShapeType.CrossPlane:
					return typeof(Voxel.ShapeType.CrossPlane);
				case Voxel.Definition.ShapeType.Cube:
				default:
					return typeof(Voxel.ShapeType.Cube);
			}
		}

		private static Voxel.Shape CreateShapeInstance(Voxel.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Voxel.Definition.ShapeType.Slab:
					return new Voxel.ShapeType.Slab();
				case Voxel.Definition.ShapeType.Slope:
					return new Voxel.ShapeType.Slope();
				case Voxel.Definition.ShapeType.Stair:
					return new Voxel.ShapeType.Stair();
				case Voxel.Definition.ShapeType.CrossPlane:
					return new Voxel.ShapeType.CrossPlane();
				case Voxel.Definition.ShapeType.Cube:
				default:
					return new Voxel.ShapeType.Cube();
			}
		}
	}
}
