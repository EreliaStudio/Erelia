using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel.Mask
{
	[System.Serializable]
	public abstract class Shape
	{
		private Dictionary<Erelia.Core.Voxel.FlipOrientation, List<Erelia.Core.Voxel.Face>> maskFaces
			= new Dictionary<Erelia.Core.Voxel.FlipOrientation, List<Erelia.Core.Voxel.Face>>();
		private Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet> cardinalPoints
			= new Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet>();

		public IReadOnlyDictionary<Erelia.Core.Voxel.FlipOrientation, List<Erelia.Core.Voxel.Face>> MaskFaces => maskFaces;
		public IReadOnlyDictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet> CardinalPoints => cardinalPoints;

		protected abstract Dictionary<Erelia.Core.Voxel.FlipOrientation, List<Erelia.Core.Voxel.Face>> ConstructMaskFaces();
		protected virtual Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			CardinalPointSet set = CardinalPointSet.CreateDefault();
			return new Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet>
			{
				[Erelia.Core.Voxel.FlipOrientation.PositiveY] = set,
				[Erelia.Core.Voxel.FlipOrientation.NegativeY] = set
			};
		}

		public void Initialize()
		{
			maskFaces = ConstructMaskFaces() ?? new Dictionary<Erelia.Core.Voxel.FlipOrientation, List<Erelia.Core.Voxel.Face>>();
			cardinalPoints = ConstructCardinalPoints() ?? new Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet>();
		}

		public Vector3 GetCardinalPoint(
			Erelia.Battle.Voxel.CardinalPoint entryPoint,
			Erelia.Core.Voxel.Orientation orientation,
			Erelia.Core.Voxel.FlipOrientation flipOrientation)
		{
			if (cardinalPoints == null || cardinalPoints.Count == 0)
			{
				cardinalPoints = ConstructCardinalPoints() ?? new Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet>();
			}

			if (!cardinalPoints.TryGetValue(flipOrientation, out CardinalPointSet set) || set == null)
			{
				if (!cardinalPoints.TryGetValue(Erelia.Core.Voxel.FlipOrientation.PositiveY, out set))
				{
					set = CardinalPointSet.CreateDefault();
				}
			}

			if (Erelia.Battle.Voxel.MesherUtils.CardinalPointSetByOrientationCache.TryGetValue(
					set,
					orientation,
					out CardinalPointSet transformed))
			{
				return transformed.Get(entryPoint);
			}

			return set.Get(entryPoint);
		}
	}
}


