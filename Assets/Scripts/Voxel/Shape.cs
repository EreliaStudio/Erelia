using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Voxel
{
	[Serializable]
	public abstract class Shape
	{
		[Serializable]
		public enum AxisPlane
		{
			PosX, NegX, PosY, NegY, PosZ, NegZ
		}

		public static readonly AxisPlane[] AxisPlanes =
		{
			AxisPlane.PosX, AxisPlane.NegX,
			AxisPlane.PosY, AxisPlane.NegY,
			AxisPlane.PosZ, AxisPlane.NegZ
		};

		[Serializable]
		public struct FaceSet
		{
			public List<Erelia.Voxel.Face> Inner;
			public Dictionary<AxisPlane, Erelia.Voxel.Face> OuterShell;

			public FaceSet(
				List<Erelia.Voxel.Face> inner,
				Dictionary<AxisPlane, Erelia.Voxel.Face> outerShell)
			{
				Inner = inner ?? new List<Erelia.Voxel.Face>();
				OuterShell = outerShell ?? new Dictionary<AxisPlane, Erelia.Voxel.Face>();
			}
		}

		[Serializable]
		public struct Channels
		{
			public FaceSet Render;
			public FaceSet Collision;

			public Channels(FaceSet render, FaceSet collision)
			{
				Render = render;
				Collision = collision;
			}
		}

		private Channels channels;
		private Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> maskFaces = new Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>>();
		private Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> cardinalPoints
			= new Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>>();

		public FaceSet RenderFaces => channels.Render;
		public FaceSet CollisionFaces => channels.Collision;

		public IReadOnlyDictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> MaskFaces => maskFaces;
		public IReadOnlyDictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> CardinalPoints => cardinalPoints;

		protected abstract FaceSet ConstructRenderFaces();
		protected virtual FaceSet ConstructCollisionFaces()
		{
			return ConstructRenderFaces();
		}
		protected abstract Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> ConstructMaskFaces();
		protected abstract Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> ConstructCardinalPoints();

		public virtual void Initialize()
		{
			channels = new Channels(
				ConstructRenderFaces(),
				ConstructCollisionFaces());

			maskFaces = ConstructMaskFaces() ?? new Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>>();
			cardinalPoints = ConstructCardinalPoints()
				?? new Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>>();
		}
	}
}


