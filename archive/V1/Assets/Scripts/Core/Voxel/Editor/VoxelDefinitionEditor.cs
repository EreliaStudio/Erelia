using UnityEditor;
using UnityEngine;

namespace Erelia.Core.Voxel.Editor
{
	[CustomEditor(typeof(Erelia.Core.Voxel.VoxelDefinition), true)]
	public class VoxelDefinitionEditor : UnityEditor.Editor
	{
		protected SerializedProperty propertiesProp;
		protected SerializedProperty shapeTypeProp;
		protected SerializedProperty shapeProp;
		protected SerializedProperty maskShapeProp;

		protected virtual void OnEnable()
		{
			propertiesProp = serializedObject.FindProperty("properties");
			shapeTypeProp = serializedObject.FindProperty("shapeType");
			shapeProp = serializedObject.FindProperty("shape");
			maskShapeProp = serializedObject.FindProperty("maskShape");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(propertiesProp, true);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(shapeTypeProp);
			bool shapeTypeChanged = EditorGUI.EndChangeCheck();

			EnsureShapeInstance(shapeTypeChanged);
			EnsureMaskShapeInstance(shapeTypeChanged);

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(shapeProp, true);
			EditorGUILayout.PropertyField(maskShapeProp, true);

			serializedObject.ApplyModifiedProperties();
		}

		protected void EnsureShapeInstance(bool forceReplace)
		{
			var shapeType = (Erelia.Core.Voxel.VoxelDefinition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetShapeClassType(shapeType);
			var current = shapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				shapeProp.managedReferenceValue = CreateShapeInstance(shapeType);
			}
		}

		protected virtual System.Type GetShapeClassType(Erelia.Core.Voxel.VoxelDefinition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slab:
					return typeof(Erelia.Core.Voxel.ShapeType.Slab);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slope:
					return typeof(Erelia.Core.Voxel.ShapeType.Slope);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Stair:
					return typeof(Erelia.Core.Voxel.ShapeType.Stair);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.CrossPlane:
					return typeof(Erelia.Core.Voxel.ShapeType.CrossPlane);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Cube:
				default:
					return typeof(Erelia.Core.Voxel.ShapeType.Cube);
			}
		}

		protected virtual Erelia.Core.Voxel.Shape CreateShapeInstance(Erelia.Core.Voxel.VoxelDefinition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slab:
					return new Erelia.Core.Voxel.ShapeType.Slab();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slope:
					return new Erelia.Core.Voxel.ShapeType.Slope();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Stair:
					return new Erelia.Core.Voxel.ShapeType.Stair();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.CrossPlane:
					return new Erelia.Core.Voxel.ShapeType.CrossPlane();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Cube:
				default:
					return new Erelia.Core.Voxel.ShapeType.Cube();
			}
		}

		protected void EnsureMaskShapeInstance(bool forceReplace)
		{
			if (maskShapeProp == null)
			{
				return;
			}

			var shapeType = (Erelia.Core.Voxel.VoxelDefinition.ShapeType)shapeTypeProp.enumValueIndex;
			System.Type expectedType = GetMaskShapeClassType(shapeType);
			object current = maskShapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				maskShapeProp.managedReferenceValue = CreateMaskShapeInstance(shapeType);
			}
		}

		protected virtual System.Type GetMaskShapeClassType(Erelia.Core.Voxel.VoxelDefinition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slab:
					return typeof(Erelia.Battle.Voxel.ShapeType.Slab);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slope:
					return typeof(Erelia.Battle.Voxel.ShapeType.Slope);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Stair:
					return typeof(Erelia.Battle.Voxel.ShapeType.Stair);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.CrossPlane:
					return typeof(Erelia.Battle.Voxel.ShapeType.CrossPlane);
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Cube:
				default:
					return typeof(Erelia.Battle.Voxel.ShapeType.Cube);
			}
		}

		protected virtual Erelia.Battle.Voxel.Mask.Shape CreateMaskShapeInstance(
			Erelia.Core.Voxel.VoxelDefinition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slab:
					return new Erelia.Battle.Voxel.ShapeType.Slab();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Slope:
					return new Erelia.Battle.Voxel.ShapeType.Slope();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Stair:
					return new Erelia.Battle.Voxel.ShapeType.Stair();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.CrossPlane:
					return new Erelia.Battle.Voxel.ShapeType.CrossPlane();
				case Erelia.Core.Voxel.VoxelDefinition.ShapeType.Cube:
				default:
					return new Erelia.Battle.Voxel.ShapeType.Cube();
			}
		}
	}
}

