using UnityEditor;
using UnityEngine;

namespace Erelia.Editor
{
	[CustomEditor(typeof(Erelia.Voxel.Definition), true)]
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
			var shapeType = (Erelia.Voxel.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetShapeClassType(shapeType);
			var current = shapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				shapeProp.managedReferenceValue = CreateShapeInstance(shapeType);
			}
		}

		protected virtual System.Type GetShapeClassType(Erelia.Voxel.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Voxel.Definition.ShapeType.Slab:
					return typeof(Erelia.Voxel.ShapeType.Slab);
				case Erelia.Voxel.Definition.ShapeType.Slope:
					return typeof(Erelia.Voxel.ShapeType.Slope);
				case Erelia.Voxel.Definition.ShapeType.Stair:
					return typeof(Erelia.Voxel.ShapeType.Stair);
				case Erelia.Voxel.Definition.ShapeType.CrossPlane:
					return typeof(Erelia.Voxel.ShapeType.CrossPlane);
				case Erelia.Voxel.Definition.ShapeType.Cube:
				default:
					return typeof(Erelia.Voxel.ShapeType.Cube);
			}
		}

		protected virtual Erelia.Voxel.Shape CreateShapeInstance(Erelia.Voxel.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Voxel.Definition.ShapeType.Slab:
					return new Erelia.Voxel.ShapeType.Slab();
				case Erelia.Voxel.Definition.ShapeType.Slope:
					return new Erelia.Voxel.ShapeType.Slope();
				case Erelia.Voxel.Definition.ShapeType.Stair:
					return new Erelia.Voxel.ShapeType.Stair();
				case Erelia.Voxel.Definition.ShapeType.CrossPlane:
					return new Erelia.Voxel.ShapeType.CrossPlane();
				case Erelia.Voxel.Definition.ShapeType.Cube:
				default:
					return new Erelia.Voxel.ShapeType.Cube();
			}
		}
	}
}

