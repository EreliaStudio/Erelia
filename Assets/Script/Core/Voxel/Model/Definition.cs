using System;
using UnityEngine;

namespace Core.Voxel.Model
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

		[SerializeField] private Core.Voxel.Model.Data data = new Core.Voxel.Model.Data();
		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		[SerializeReference] private Core.Voxel.Geometry.Shape shape = new Core.Voxel.Geometry.Cube();

		public Core.Voxel.Model.Data Data => data;
		public ShapeType Type => shapeType;
		public Core.Voxel.Geometry.Shape Shape => shape;
	}
}
