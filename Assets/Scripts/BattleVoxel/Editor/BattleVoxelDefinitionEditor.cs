using UnityEditor;
using UnityEngine;

namespace Erelia.Editor
{
	[CustomEditor(typeof(Erelia.BattleVoxel.Definition))]
	public class BattleVoxelDefinitionEditor : Erelia.Editor.VoxelDefinitionEditor
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
			var shapeType = (Erelia.Voxel.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetMaskShapeClassType(shapeType);
			var current = maskShapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				maskShapeProp.managedReferenceValue = CreateMaskShapeInstance(shapeType);
			}
		}

		private static System.Type GetMaskShapeClassType(Erelia.Voxel.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Voxel.Definition.ShapeType.Slab:
					return typeof(Erelia.BattleVoxel.ShapeType.Slab);
				case Erelia.Voxel.Definition.ShapeType.Slope:
					return typeof(Erelia.BattleVoxel.ShapeType.Slope);
				case Erelia.Voxel.Definition.ShapeType.Stair:
					return typeof(Erelia.BattleVoxel.ShapeType.Stair);
				case Erelia.Voxel.Definition.ShapeType.CrossPlane:
					return typeof(Erelia.BattleVoxel.ShapeType.CrossPlane);
				case Erelia.Voxel.Definition.ShapeType.Cube:
				default:
					return typeof(Erelia.BattleVoxel.ShapeType.Cube);
			}
		}

		private static Erelia.BattleVoxel.MaskShape CreateMaskShapeInstance(Erelia.Voxel.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Voxel.Definition.ShapeType.Slab:
					return new Erelia.BattleVoxel.ShapeType.Slab();
				case Erelia.Voxel.Definition.ShapeType.Slope:
					return new Erelia.BattleVoxel.ShapeType.Slope();
				case Erelia.Voxel.Definition.ShapeType.Stair:
					return new Erelia.BattleVoxel.ShapeType.Stair();
				case Erelia.Voxel.Definition.ShapeType.CrossPlane:
					return new Erelia.BattleVoxel.ShapeType.CrossPlane();
				case Erelia.Voxel.Definition.ShapeType.Cube:
				default:
					return new Erelia.BattleVoxel.ShapeType.Cube();
			}
		}
	}
}
