using System.Collections.Generic;
using UnityEngine;

namespace Erelia.BattleVoxel
{
	[System.Serializable]
	public abstract class MaskShape
	{
		private Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> maskFaces
			= new Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>>();
		private Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>> cardinalPoints
			= new Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>>();

		public IReadOnlyDictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> MaskFaces => maskFaces;

		public IReadOnlyDictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>> CardinalPoints => cardinalPoints;

		protected abstract Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> ConstructMaskFaces();
		protected abstract Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>> ConstructCardinalPoints();

		public void Initialize()
		{
			maskFaces = ConstructMaskFaces() ?? new Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>>();
			cardinalPoints = ConstructCardinalPoints()
				?? new Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>>();
		}
	}
}

