using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel
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
			public List<Voxel.Face> Inner;
			public Dictionary<AxisPlane, Voxel.Face> OuterShell;

			public FaceSet(
				List<Voxel.Face> inner,
				Dictionary<AxisPlane, Voxel.Face> outerShell)
			{
				Inner = inner ?? new List<Voxel.Face>();
				OuterShell = outerShell ?? new Dictionary<AxisPlane, Voxel.Face>();
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
		private Dictionary<Voxel.FlipOrientation, List<Voxel.Face>> maskFaces = new Dictionary<Voxel.FlipOrientation, List<Voxel.Face>>();
		private Dictionary<Voxel.FlipOrientation, Dictionary<Voxel.CardinalPoint, Vector3>> cardinalPoints
			= new Dictionary<Voxel.FlipOrientation, Dictionary<Voxel.CardinalPoint, Vector3>>();

		public FaceSet RenderFaces => channels.Render;
		public FaceSet CollisionFaces => channels.Collision;

		public IReadOnlyDictionary<Voxel.FlipOrientation, List<Voxel.Face>> MaskFaces => maskFaces;
		public IReadOnlyDictionary<Voxel.FlipOrientation, Dictionary<Voxel.CardinalPoint, Vector3>> CardinalPoints => cardinalPoints;

		protected abstract FaceSet ConstructRenderFaces();
		protected virtual FaceSet ConstructCollisionFaces()
		{
			return ConstructRenderFaces();
		}
		protected abstract Dictionary<Voxel.FlipOrientation, List<Voxel.Face>> ConstructMaskFaces();
		protected abstract Dictionary<Voxel.FlipOrientation, Dictionary<Voxel.CardinalPoint, Vector3>> ConstructCardinalPoints();

		public virtual void Initialize()
		{
			channels = new Channels(
				ConstructRenderFaces(),
				ConstructCollisionFaces());

			maskFaces = ConstructMaskFaces() ?? new Dictionary<Voxel.FlipOrientation, List<Voxel.Face>>();
			cardinalPoints = ConstructCardinalPoints()
				?? new Dictionary<Voxel.FlipOrientation, Dictionary<Voxel.CardinalPoint, Vector3>>();
		}
	}
}
