using System.Collections.Generic;
using UnityEngine;
using System;

namespace Voxel.View
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

		[SerializeField] protected List<Voxel.Model.Face> innerFaces = new List<Voxel.Model.Face>();
		[SerializeField] protected Dictionary<AxisPlane, Voxel.Model.Face> outerShellFaces = new Dictionary<AxisPlane, Voxel.Model.Face>();
		[SerializeField] protected List<Voxel.Model.Face> maskFaces = new List<Voxel.Model.Face>();
		[SerializeField] protected List<Voxel.Model.Face> flippedMaskFaces = new List<Voxel.Model.Face>();
		[SerializeField] private CollisionMode collisionMode = CollisionMode.RealMesh;

		public IReadOnlyList<Voxel.Model.Face> InnerFaces => innerFaces;
		public IReadOnlyDictionary<AxisPlane, Voxel.Model.Face> OuterShellFaces => outerShellFaces;
		public IReadOnlyList<Voxel.Model.Face> MaskFaces => maskFaces;
		public IReadOnlyList<Voxel.Model.Face> FlippedMaskFaces => flippedMaskFaces;
		public CollisionMode Collision => collisionMode;

		protected abstract List<Voxel.Model.Face> ConstructInnerFaces();
		protected abstract Dictionary<AxisPlane, Voxel.Model.Face> ConstructOuterShellFaces();
		protected abstract List<Voxel.Model.Face> ConstructMaskFaces();
		protected abstract List<Voxel.Model.Face> ConstructFlippedMaskFaces();

		public virtual void Initialize()
		{
			innerFaces = ConstructInnerFaces() ?? new List<Voxel.Model.Face>();
			outerShellFaces = ConstructOuterShellFaces() ?? new Dictionary<AxisPlane, Voxel.Model.Face>();
			maskFaces = ConstructMaskFaces() ?? new List<Voxel.Model.Face>();
			flippedMaskFaces = ConstructFlippedMaskFaces() ?? new List<Voxel.Model.Face>();
		}
	}
}
