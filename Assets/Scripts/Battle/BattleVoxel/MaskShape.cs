using System.Collections.Generic;
using UnityEngine;

namespace Erelia.BattleVoxel
{
	[System.Serializable]
	public abstract class MaskShape
	{
		private Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> maskFaces
			= new Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>>();
		private Dictionary<VoxelKit.FlipOrientation, CardinalPointSet> cardinalPoints
			= new Dictionary<VoxelKit.FlipOrientation, CardinalPointSet>();

		public IReadOnlyDictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> MaskFaces => maskFaces;
		public IReadOnlyDictionary<VoxelKit.FlipOrientation, CardinalPointSet> CardinalPoints => cardinalPoints;

		protected abstract Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> ConstructMaskFaces();
		protected virtual Dictionary<VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			CardinalPointSet set = CardinalPointSet.CreateDefault();
			return new Dictionary<VoxelKit.FlipOrientation, CardinalPointSet>
			{
				[VoxelKit.FlipOrientation.PositiveY] = set,
				[VoxelKit.FlipOrientation.NegativeY] = set
			};
		}

		public void Initialize()
		{
			maskFaces = ConstructMaskFaces() ?? new Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>>();
			cardinalPoints = ConstructCardinalPoints() ?? new Dictionary<VoxelKit.FlipOrientation, CardinalPointSet>();
		}

		public Vector3 GetCardinalPoint(
			VoxelKit.CardinalPoint entryPoint,
			VoxelKit.Orientation orientation,
			VoxelKit.FlipOrientation flipOrientation)
		{
			if (cardinalPoints == null || cardinalPoints.Count == 0)
			{
				cardinalPoints = ConstructCardinalPoints() ?? new Dictionary<VoxelKit.FlipOrientation, CardinalPointSet>();
			}

			if (!cardinalPoints.TryGetValue(flipOrientation, out CardinalPointSet set) || set == null)
			{
				if (!cardinalPoints.TryGetValue(VoxelKit.FlipOrientation.PositiveY, out set))
				{
					set = CardinalPointSet.CreateDefault();
				}
			}

			if (Erelia.BattleVoxel.Mesherutils.CardinalPointSetCache.TryGetValue(
					set,
					orientation,
					VoxelKit.FlipOrientation.PositiveY,
					out CardinalPointSet transformed))
			{
				return transformed.Get(entryPoint);
			}

			return set.Get(entryPoint);
		}
	}
}

