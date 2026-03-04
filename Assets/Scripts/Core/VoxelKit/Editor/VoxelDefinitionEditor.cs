using UnityEditor;
using UnityEngine;

namespace Erelia.Core.VoxelKit.Editor
{
	[CustomEditor(typeof(Erelia.Core.VoxelKit.Definition), true)]
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
			var shapeType = (Erelia.Core.VoxelKit.Definition.ShapeType)shapeTypeProp.enumValueIndex;
			var expectedType = GetShapeClassType(shapeType);
			var current = shapeProp.managedReferenceValue;

			if (forceReplace || current == null || current.GetType() != expectedType)
			{
				shapeProp.managedReferenceValue = CreateShapeInstance(shapeType);
			}
		}

		protected virtual System.Type GetShapeClassType(Erelia.Core.VoxelKit.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slab:
					return typeof(Erelia.Core.VoxelKit.ShapeType.Slab);
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slope:
					return typeof(Erelia.Core.VoxelKit.ShapeType.Slope);
				case Erelia.Core.VoxelKit.Definition.ShapeType.Stair:
					return typeof(Erelia.Core.VoxelKit.ShapeType.Stair);
				case Erelia.Core.VoxelKit.Definition.ShapeType.CrossPlane:
					return typeof(Erelia.Core.VoxelKit.ShapeType.CrossPlane);
				case Erelia.Core.VoxelKit.Definition.ShapeType.Cube:
				default:
					return typeof(Erelia.Core.VoxelKit.ShapeType.Cube);
			}
		}

		protected virtual Erelia.Core.VoxelKit.Shape CreateShapeInstance(Erelia.Core.VoxelKit.Definition.ShapeType shapeType)
		{
			switch (shapeType)
			{
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slab:
					return new Erelia.Core.VoxelKit.ShapeType.Slab();
				case Erelia.Core.VoxelKit.Definition.ShapeType.Slope:
					return new Erelia.Core.VoxelKit.ShapeType.Slope();
				case Erelia.Core.VoxelKit.Definition.ShapeType.Stair:
					return new Erelia.Core.VoxelKit.ShapeType.Stair();
				case Erelia.Core.VoxelKit.Definition.ShapeType.CrossPlane:
					return new Erelia.Core.VoxelKit.ShapeType.CrossPlane();
				case Erelia.Core.VoxelKit.Definition.ShapeType.Cube:
				default:
					return new Erelia.Core.VoxelKit.ShapeType.Cube();
			}
		}
	}
}


