using System;
using UnityEngine;

namespace Erelia.Voxel
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

		[SerializeField] private Erelia.Voxel.Data data = new Erelia.Voxel.Data();
		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		[SerializeReference] private Erelia.Voxel.Shape shape = null;

		public Erelia.Voxel.Data Data => data;
		public ShapeType Type => shapeType;
		public Erelia.Voxel.Shape Shape => shape;

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


