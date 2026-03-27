using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Erelia.Core.Voxel
{
	[CreateAssetMenu(menuName = "Voxel/Definition", fileName = "NewVoxelDefinition")]
	public class VoxelDefinition : ScriptableObject
	{
		public enum ShapeType
		{
			Cube,

			Slab,

			Slope,

			Stair,

			CrossPlane
		}

		[FormerlySerializedAs("data")]
		[SerializeField] private Erelia.Core.Voxel.VoxelProperties properties = new Erelia.Core.Voxel.VoxelProperties();

		[SerializeField] private ShapeType shapeType = ShapeType.Cube;

		[SerializeReference] private Erelia.Core.Voxel.Shape shape = null;

		[HideInInspector]
		[SerializeReference]
		private Erelia.Battle.Voxel.Mask.Shape maskShape = null;

		public Erelia.Core.Voxel.VoxelProperties Properties => properties;

		public ShapeType Type => shapeType;

		public Erelia.Core.Voxel.Shape Shape => shape;

		public Erelia.Battle.Voxel.Mask.Shape MaskShape => maskShape;

		protected virtual void Initialize()
		{
			shape?.Initialize();
			maskShape?.Initialize();
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

