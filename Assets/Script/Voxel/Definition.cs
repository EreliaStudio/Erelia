using System;
using UnityEngine;

namespace Voxel
{
	[CreateAssetMenu(menuName = "Voxel/Definition")]
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

		[SerializeField] private Voxel.Model.Data data = new Voxel.Model.Data();
		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		[SerializeReference] private Voxel.View.Shape shape = new Voxel.View.Cube();

		public Voxel.Model.Data Data => data;
		public ShapeType Type => shapeType;
		public Voxel.View.Shape Shape => shape;
	}
}
