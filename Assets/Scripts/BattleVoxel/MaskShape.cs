using System.Collections.Generic;
using UnityEngine;

namespace Erelia.BattleVoxel
{
	[System.Serializable]
	public abstract class MaskShape
	{
		private Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> maskFaces
			= new Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>>();
		private Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> cardinalPoints
			= new Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>>();

		public IReadOnlyDictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> MaskFaces => maskFaces;

		public IReadOnlyDictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> CardinalPoints => cardinalPoints;

		protected abstract Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> ConstructMaskFaces();
		protected abstract Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> ConstructCardinalPoints();

		public void Initialize()
		{
			maskFaces = ConstructMaskFaces() ?? new Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>>();
			cardinalPoints = ConstructCardinalPoints()
				?? new Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>>();
		}
	}
}
