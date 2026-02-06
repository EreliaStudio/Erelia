using System.Collections.Generic;
using UnityEngine;
using System;

namespace Voxel.View
{
	[Serializable]
	public abstract class Shape
	{
		[Serializable]
		public enum OuterShellPlane
		{
			PosX,
			NegX,
			PosY,
			NegY,
			PosZ,
			NegZ
		}

		protected List<Voxel.View.Face> innerFaces = new List<Voxel.View.Face>();
		protected Dictionary<OuterShellPlane, Voxel.View.Face> outerShellByPlane = new Dictionary<OuterShellPlane, Voxel.View.Face>();
		protected List<Voxel.View.Face> maskFaces = new List<Voxel.View.Face>();
		protected List<Voxel.View.Face> flippedMaskFaces = new List<Voxel.View.Face>();

		[NonSerialized] private bool isBuilt = false;

		public IReadOnlyList<Voxel.View.Face> InnerFaces
		{
			get
			{
				EnsureBuilt();
				return innerFaces;
			}
		}

		public IReadOnlyDictionary<OuterShellPlane, Voxel.View.Face> OuterShellFaces
		{
			get
			{
				EnsureBuilt();
				return outerShellByPlane;
			}
		}

		public IReadOnlyList<Voxel.View.Face> MaskFaces
		{
			get
			{
				EnsureBuilt();
				return maskFaces;
			}
		}

		public IReadOnlyList<Voxel.View.Face> FlippedMaskFaces
		{
			get
			{
				EnsureBuilt();
				return flippedMaskFaces;
			}
		}

		protected abstract List<Voxel.View.Face> ConstructInnerFaces();
		protected abstract Dictionary<OuterShellPlane, Voxel.View.Face> ConstructOuterShellFaces();
		protected abstract List<Voxel.View.Face> ConstructMaskFaces();
		protected abstract List<Voxel.View.Face> ConstructFlippedMaskFaces();

		public void EnsureBuilt()
		{
			if (isBuilt)
			{
				return;
			}

			RebuildRuntimeFaces();
		}

		public void Invalidate()
		{
			isBuilt = false;
		}

		protected void RebuildRuntimeFaces()
		{
			innerFaces = ConstructInnerFaces();
			outerShellByPlane = ConstructOuterShellFaces();
			maskFaces = ConstructMaskFaces();
			flippedMaskFaces = ConstructFlippedMaskFaces();
			isBuilt = true;
		}
	}
}
