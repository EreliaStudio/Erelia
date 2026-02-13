using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Voxel.Geometry
{
	[Serializable]
	public abstract class Shape
	{
		[Serializable]
		public enum CollisionMode
		{
			RealMesh,
			CubeEnvelope
		}

		[Serializable]
		public enum AxisPlane
		{
			PosX,
			NegX,
			PosY,
			NegY,
			PosZ,
			NegZ
		}

		public static readonly AxisPlane[] AxisPlanes =
		{
			AxisPlane.PosX,
			AxisPlane.NegX,
			AxisPlane.PosY,
			AxisPlane.NegY,
			AxisPlane.PosZ,
			AxisPlane.NegZ
		};

		[SerializeField] protected List<Core.Voxel.Model.Face> innerFaces = new List<Core.Voxel.Model.Face>();
		[SerializeField] protected Dictionary<AxisPlane, Core.Voxel.Model.Face> outerShellFaces = new Dictionary<AxisPlane, Core.Voxel.Model.Face>();
		[SerializeField] protected Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>> maskFaces = new Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>>();
		[SerializeField] private CollisionMode collisionMode = CollisionMode.RealMesh;

		public IReadOnlyList<Core.Voxel.Model.Face> InnerFaces => innerFaces;
		public IReadOnlyDictionary<AxisPlane, Core.Voxel.Model.Face> OuterShellFaces => outerShellFaces;
		public IReadOnlyDictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>> MaskFaces => maskFaces;
		public CollisionMode Collision => collisionMode;

		protected abstract List<Core.Voxel.Model.Face> ConstructInnerFaces();
		protected abstract Dictionary<AxisPlane, Core.Voxel.Model.Face> ConstructOuterShellFaces();
		protected abstract Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>> ConstructMaskFaces();

		public virtual void Initialize()
		{
			innerFaces = ConstructInnerFaces() ?? new List<Core.Voxel.Model.Face>();
			outerShellFaces = ConstructOuterShellFaces() ?? new Dictionary<AxisPlane, Core.Voxel.Model.Face>();
			maskFaces = ConstructMaskFaces() ?? new Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>>();
		}
	}
}
