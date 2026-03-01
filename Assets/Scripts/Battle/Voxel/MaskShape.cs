using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel
{
	[System.Serializable]
	public abstract class MaskShape
	{
		private Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> maskFaces
			= new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>>();
		private Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> cardinalPoints
			= new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>();

		public IReadOnlyDictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> MaskFaces => maskFaces;
		public IReadOnlyDictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> CardinalPoints => cardinalPoints;

		protected abstract Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> ConstructMaskFaces();
		protected virtual Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			CardinalPointSet set = CardinalPointSet.CreateDefault();
			return new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>
			{
				[Erelia.Core.VoxelKit.FlipOrientation.PositiveY] = set,
				[Erelia.Core.VoxelKit.FlipOrientation.NegativeY] = set
			};
		}

		public void Initialize()
		{
			maskFaces = ConstructMaskFaces() ?? new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>>();
			cardinalPoints = ConstructCardinalPoints() ?? new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>();
		}

		public Vector3 GetCardinalPoint(
			Erelia.Core.VoxelKit.CardinalPoint entryPoint,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			if (cardinalPoints == null || cardinalPoints.Count == 0)
			{
				cardinalPoints = ConstructCardinalPoints() ?? new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>();
			}

			if (!cardinalPoints.TryGetValue(flipOrientation, out CardinalPointSet set) || set == null)
			{
				if (!cardinalPoints.TryGetValue(Erelia.Core.VoxelKit.FlipOrientation.PositiveY, out set))
				{
					set = CardinalPointSet.CreateDefault();
				}
			}

			if (Erelia.Battle.Voxel.MesherUtils.CardinalPointSetCache.TryGetValue(
					set,
					orientation,
					Erelia.Core.VoxelKit.FlipOrientation.PositiveY,
					out CardinalPointSet transformed))
			{
				return transformed.Get(entryPoint);
			}

			return set.Get(entryPoint);
		}
	}
}

