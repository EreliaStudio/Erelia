using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelKit
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
			public List<VoxelKit.Face> Inner;
			public Dictionary<AxisPlane, VoxelKit.Face> OuterShell;

			public FaceSet(
				List<VoxelKit.Face> inner,
				Dictionary<AxisPlane, VoxelKit.Face> outerShell)
			{
				Inner = inner ?? new List<VoxelKit.Face>();
				OuterShell = outerShell ?? new Dictionary<AxisPlane, VoxelKit.Face>();
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

		public FaceSet RenderFaces => channels.Render;
		public FaceSet CollisionFaces => channels.Collision;

		protected abstract FaceSet ConstructRenderFaces();
		protected virtual FaceSet ConstructCollisionFaces()
		{
			return ConstructRenderFaces();
		}
		public virtual void Initialize()
		{
			channels = new Channels(
				ConstructRenderFaces(),
				ConstructCollisionFaces());
		}
	}
}



