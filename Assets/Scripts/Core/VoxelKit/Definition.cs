using System;
using UnityEngine;

namespace Erelia.Core.VoxelKit
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

		[SerializeField] private Erelia.Core.VoxelKit.Data data = new Erelia.Core.VoxelKit.Data();
		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		[SerializeReference] private Erelia.Core.VoxelKit.Shape shape = null;

		public Erelia.Core.VoxelKit.Data Data => data;
		public ShapeType Type => shapeType;
		public Erelia.Core.VoxelKit.Shape Shape => shape;

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



