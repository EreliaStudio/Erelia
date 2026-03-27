using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Voxel
{
	[Serializable]
	public abstract class Shape
	{
		[Serializable]
		public struct FaceSet
		{
			public List<Erelia.Core.Voxel.Face> Inner;

			public Dictionary<AxisPlane, Erelia.Core.Voxel.Face> OuterShell;

			public FaceSet(
				List<Erelia.Core.Voxel.Face> inner,
				Dictionary<AxisPlane, Erelia.Core.Voxel.Face> outerShell)
			{
				Inner = inner ?? new List<Erelia.Core.Voxel.Face>();
				OuterShell = outerShell ?? new Dictionary<AxisPlane, Erelia.Core.Voxel.Face>();
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
