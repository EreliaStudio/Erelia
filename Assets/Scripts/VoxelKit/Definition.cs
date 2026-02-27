using System;
using UnityEngine;

namespace VoxelKit
{
	[CreateAssetMenu(menuName = "Voxel/Definition", fileName = "NewVoxelDefinition")]
	public class Definition : ScriptableObject
	{
		public enum ShapeType
		{
			Cube,
			Slab,
			Slope,
			Stair,
			CrossPlane
		}

		[SerializeField] private VoxelKit.Data data = new VoxelKit.Data();
		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		[SerializeReference] private VoxelKit.Shape shape = null;

		public VoxelKit.Data Data => data;
		public ShapeType Type => shapeType;
		public VoxelKit.Shape Shape => shape;

		protected virtual void Initialize()
		{
			shape?.Initialize();
		}

		private void OnEnable()
		{
			Initialize();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			Initialize();
		}
#endif
	}
}



