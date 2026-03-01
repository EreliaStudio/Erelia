using UnityEditor;
using UnityEngine;

namespace Erelia.Battle.Voxel.Editor
{
	[CustomEditor(typeof(Erelia.Battle.Voxel.Definition))]
	public class BattleVoxelDefinitionEditor : Erelia.Core.VoxelKit.Editor.VoxelDefinitionEditor
	{
		private SerializedProperty battleDataProp;
		private SerializedProperty maskShapeProp;

		protected override void OnEnable()
		{
			base.OnEnable();
			battleDataProp = serializedObject.FindProperty("battleData");
			maskShapeProp = serializedObject.FindProperty("maskShape");
		}

		protected override void DrawCustomFields()
		{
			EditorGUILayout.PropertyField(battleDataProp, true);
			EditorGUILayout.PropertyField(maskShapeProp, true);
		}

		protected override void AfterEnsureShapeInstance(bool shapeTypeChanged)
		{
			EnsureMaskShapeInstance(shapeTypeChanged);
		}

		private void EnsureMaskShapeInstance(bool forceReplace)
		{
			var shapeType = (Erelia.Core.VoxelKit.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetMaskShapeClassType(shapeType);
			var current = maskShapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				maskShapeProp.managedReferenceValue = CreateMaskShapeInstance(shapeType);
			}
		}

		private static System.Type GetMaskShapeClassType(Erelia.Core.VoxelKit.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slab:
					return typeof(Erelia.Battle.Voxel.ShapeType.Slab);
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slope:
					return typeof(Erelia.Battle.Voxel.ShapeType.Slope);
				case Erelia.Core.VoxelKit.Definition.ShapeType.Stair:
					return typeof(Erelia.Battle.Voxel.ShapeType.Stair);
				case Erelia.Core.VoxelKit.Definition.ShapeType.CrossPlane:
					return typeof(Erelia.Battle.Voxel.ShapeType.CrossPlane);
				case Erelia.Core.VoxelKit.Definition.ShapeType.Cube:
				default:
					return typeof(Erelia.Battle.Voxel.ShapeType.Cube);
			}
		}

		private static Erelia.Battle.Voxel.MaskShape CreateMaskShapeInstance(Erelia.Core.VoxelKit.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slab:
					return new Erelia.Battle.Voxel.ShapeType.Slab();
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slope:
					return new Erelia.Battle.Voxel.ShapeType.Slope();
				case Erelia.Core.VoxelKit.Definition.ShapeType.Stair:
					return new Erelia.Battle.Voxel.ShapeType.Stair();
				case Erelia.Core.VoxelKit.Definition.ShapeType.CrossPlane:
					return new Erelia.Battle.Voxel.ShapeType.CrossPlane();
				case Erelia.Core.VoxelKit.Definition.ShapeType.Cube:
				default:
					return new Erelia.Battle.Voxel.ShapeType.Cube();
			}
		}
	}
}

