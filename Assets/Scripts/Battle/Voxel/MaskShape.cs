using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel
{
	/// <summary>
	/// Base class for voxel mask shapes used to render battle overlays.
	/// Builds face lists and cardinal point sets for different orientations.
	/// </summary>
	[System.Serializable]
	public abstract class MaskShape
	{
		private Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> maskFaces
			= new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>>();
		private Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> cardinalPoints
			= new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>();

		public IReadOnlyDictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> MaskFaces => maskFaces;
		public IReadOnlyDictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> CardinalPoints => cardinalPoints;

		/// <summary>
		/// Constructs the mask faces for each flip orientation.
		/// </summary>
		protected abstract Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> ConstructMaskFaces();
		/// <summary>
		/// Constructs the cardinal point sets for each flip orientation.
		/// </summary>
		protected virtual Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			// Provide default cardinal points for both flip orientations.
			CardinalPointSet set = CardinalPointSet.CreateDefault();
			return new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>
			{
				[Erelia.Core.VoxelKit.FlipOrientation.PositiveY] = set,
				[Erelia.Core.VoxelKit.FlipOrientation.NegativeY] = set
			};
		}

		/// <summary>
		/// Initializes cached faces and cardinal point sets.
		/// </summary>
		public void Initialize()
		{
			// Build face and cardinal point caches.
			maskFaces = ConstructMaskFaces() ?? new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>>();
			cardinalPoints = ConstructCardinalPoints() ?? new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>();
		}

		/// <summary>
		/// Resolves a cardinal point offset for a given orientation and flip.
		/// </summary>
		public Vector3 GetCardinalPoint(
			Erelia.Battle.Voxel.CardinalPoint entryPoint,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			// Ensure cardinal points are initialized and return a transformed point.
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

