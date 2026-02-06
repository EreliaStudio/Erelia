using System.Collections.Generic;
using UnityEngine;
using System;

namespace Voxel.View
{
	[Serializable]
	public abstract class Shape
	{
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

		[SerializeField] protected List<Voxel.View.Face> innerFaces = new List<Voxel.View.Face>();
		[SerializeField] protected Dictionary<AxisPlane, Voxel.View.Face> outerShellFaces = new Dictionary<AxisPlane, Voxel.View.Face>();
		[SerializeField] protected List<Voxel.View.Face> maskFaces = new List<Voxel.View.Face>();
		[SerializeField] protected List<Voxel.View.Face> flippedMaskFaces = new List<Voxel.View.Face>();

		public IReadOnlyList<Voxel.View.Face> InnerFaces => innerFaces;
		public IReadOnlyDictionary<AxisPlane, Voxel.View.Face> OuterShellFaces => outerShellFaces;
		public IReadOnlyList<Voxel.View.Face> MaskFaces => maskFaces;
		public IReadOnlyList<Voxel.View.Face> FlippedMaskFaces => flippedMaskFaces;

		protected abstract List<Voxel.View.Face> ConstructInnerFaces();
		protected abstract Dictionary<AxisPlane, Voxel.View.Face> ConstructOuterShellFaces();
		protected abstract List<Voxel.View.Face> ConstructMaskFaces();
		protected abstract List<Voxel.View.Face> ConstructFlippedMaskFaces();

		protected virtual void OnEnable()
		{
			innerFaces = ConstructInnerFaces() ?? new List<Voxel.View.Face>();
			outerShellFaces = ConstructOuterShellFaces() ?? new Dictionary<AxisPlane, Voxel.View.Face>();
			maskFaces = ConstructMaskFaces() ?? new List<Voxel.View.Face>();
			flippedMaskFaces = ConstructFlippedMaskFaces() ?? new List<Voxel.View.Face>();
		}
	}
}
