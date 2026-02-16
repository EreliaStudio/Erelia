using UnityEditor;
using UnityEngine;
using Core.Voxel;
using Core.Voxel.Geometry;

namespace Erelia.Editor
{
	[CustomEditor(typeof(Core.Voxel.Model.Definition))]
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
			if (EditorGUI.EndChangeCheck())
			{
				var shape = shapeProp.managedReferenceValue as Shape;
				// if (shape != null)
				// {
				// 	shape.OnEnable();
				// }
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void EnsureShapeInstance(bool forceReplace)
		{
			var shapeType = (Core.Voxel.Model.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetShapeClassType(shapeType);
			var current = shapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				shapeProp.managedReferenceValue = CreateShapeInstance(shapeType);
			}
		}

		private static System.Type GetShapeClassType(Core.Voxel.Model.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Core.Voxel.Model.Definition.ShapeType.Slab:
					return typeof(Slab);
				case Core.Voxel.Model.Definition.ShapeType.Slope:
					return typeof(Slope);
				case Core.Voxel.Model.Definition.ShapeType.Stair:
					return typeof(Stair);
				case Core.Voxel.Model.Definition.ShapeType.CrossPlane:
					return typeof(CrossPlane);
				case Core.Voxel.Model.Definition.ShapeType.Cube:
				default:
					return typeof(Cube);
			}
		}

		private static Shape CreateShapeInstance(Core.Voxel.Model.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Core.Voxel.Model.Definition.ShapeType.Slab:
					return new Slab();
				case Core.Voxel.Model.Definition.ShapeType.Slope:
					return new Slope();
				case Core.Voxel.Model.Definition.ShapeType.Stair:
					return new Stair();
				case Core.Voxel.Model.Definition.ShapeType.CrossPlane:
					return new CrossPlane();
				case Core.Voxel.Model.Definition.ShapeType.Cube:
				default:
					return new Cube();
			}
		}
	}
}
