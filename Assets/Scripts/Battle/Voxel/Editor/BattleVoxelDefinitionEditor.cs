using UnityEditor;
using UnityEngine;

namespace Erelia.Battle.Voxel.Editor
{
	/// <summary>
	/// Custom editor for battle voxel definitions.
	/// Exposes battle data and mask shape fields in the inspector.
	/// </summary>
	[CustomEditor(typeof(Erelia.Battle.Voxel.Definition))]
	public class BattleVoxelDefinitionEditor : Erelia.Core.VoxelKit.Editor.VoxelDefinitionEditor
	{
		/// <summary>
		/// Serialized property for battle data.
		/// </summary>
		private SerializedProperty battleDataProp;
		/// <summary>
		/// Serialized property for the mask shape.
		/// </summary>
		private SerializedProperty maskShapeProp;

		/// <summary>
		/// Unity callback invoked when the editor is enabled.
		/// </summary>
		protected override void OnEnable()
		{
			// Cache serialized properties for custom fields.
			base.OnEnable();
			battleDataProp = serializedObject.FindProperty("battleData");
			maskShapeProp = serializedObject.FindProperty("maskShape");
		}

		/// <summary>
		/// Draws battle-specific fields in the inspector.
		/// </summary>
		protected override void DrawCustomFields()
		{
			// Draw battle data and mask shape fields.
			EditorGUILayout.PropertyField(battleDataProp, true);
			EditorGUILayout.PropertyField(maskShapeProp, true);
		}

		/// <summary>
		/// Ensures the mask shape instance matches the voxel shape type.
		/// </summary>
		protected override void AfterEnsureShapeInstance(bool shapeTypeChanged)
		{
			// Recreate the mask shape when the voxel shape changes.
			EnsureMaskShapeInstance(shapeTypeChanged);
		}

		/// <summary>
		/// Creates or replaces the mask shape instance as needed.
		/// </summary>
		private void EnsureMaskShapeInstance(bool forceReplace)
		{
			// Instantiate a mask shape that matches the current shape type.
			var shapeType = (Erelia.Core.VoxelKit.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetMaskShapeClassType(shapeType);
			var current = maskShapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				maskShapeProp.managedReferenceValue = CreateMaskShapeInstance(shapeType);
			}
		}

		/// <summary>
		/// Maps voxel shape types to mask shape classes.
		/// </summary>
		private static System.Type GetMaskShapeClassType(Erelia.Core.VoxelKit.Definition.ShapeType shapeType)
		{
			// Select the mask shape type that matches the voxel shape.
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

		/// <summary>
		/// Creates a mask shape instance for the given voxel shape type.
		/// </summary>
		private static Erelia.Battle.Voxel.Mask.Shape CreateMaskShapeInstance(Erelia.Core.VoxelKit.Definition.ShapeType shapeType)
		{
			// Instantiate the appropriate mask shape.
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

