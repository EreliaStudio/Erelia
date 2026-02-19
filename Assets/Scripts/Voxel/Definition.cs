using System;
using UnityEngine;

namespace Voxel
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

		[SerializeField] private int id = -1;

		[SerializeField] private Voxel.Data data = new Voxel.Data();
		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		[SerializeReference] private Voxel.Shape shape = null;

		public int Id => id;
		public Voxel.Data Data => data;
		public ShapeType Type => shapeType;
		public Voxel.Shape Shape => shape;

		private void OnEnable()
		{
			shape?.Initialize();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			shape?.Initialize();
		}
#endif
	}
}
