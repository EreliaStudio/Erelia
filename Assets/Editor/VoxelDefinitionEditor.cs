using UnityEditor;
using UnityEngine;
using Voxel;
using Voxel.View;

namespace Erelia.Editor
{
	[CustomEditor(typeof(Voxel.Model.Definition))]
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
			var shapeType = (Voxel.Model.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetShapeClassType(shapeType);
			var current = shapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				shapeProp.managedReferenceValue = CreateShapeInstance(shapeType);
			}
		}

		private static System.Type GetShapeClassType(Voxel.Model.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Voxel.Model.Definition.ShapeType.Slab:
					return typeof(Slab);
				case Voxel.Model.Definition.ShapeType.Slope:
					return typeof(Slope);
				case Voxel.Model.Definition.ShapeType.Stair:
					return typeof(Stair);
				case Voxel.Model.Definition.ShapeType.CrossPlane:
					return typeof(CrossPlane);
				case Voxel.Model.Definition.ShapeType.Cube:
				default:
					return typeof(Cube);
			}
		}

		private static Shape CreateShapeInstance(Voxel.Model.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Voxel.Model.Definition.ShapeType.Slab:
					return new Slab();
				case Voxel.Model.Definition.ShapeType.Slope:
					return new Slope();
				case Voxel.Model.Definition.ShapeType.Stair:
					return new Stair();
				case Voxel.Model.Definition.ShapeType.CrossPlane:
					return new CrossPlane();
				case Voxel.Model.Definition.ShapeType.Cube:
				default:
					return new Cube();
			}
		}
	}
}
