using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Battle.Voxel.ShapeType
{
	/// <summary>
	/// Mask shape for cross-plane voxels.
	/// Defines the top face and cardinal entry points for placement offsets.
	/// </summary>
	[Serializable]
	public class CrossPlane : Erelia.Battle.Voxel.Mask.Shape
	{
		/// <summary>
		/// Constructs the mask faces for a cross-plane voxel.
		/// </summary>
		protected override Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> ConstructMaskFaces()
		{
			// Build a single top face for the mask.
			const float maskOffset = 0.01f;
			var faces = new List<Erelia.Core.VoxelKit.Face>();
			Erelia.Core.VoxelKit.Face top = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), TileUV = new Vector2(0f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), TileUV = new Vector2(1f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), TileUV = new Vector2(1f, 1f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), TileUV = new Vector2(0f, 1f) });
			faces.Add(top);

			return new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>>
			{
				[Erelia.Core.VoxelKit.FlipOrientation.PositiveY] = faces,
				[Erelia.Core.VoxelKit.FlipOrientation.NegativeY] = faces
			};
		}

		/// <summary>
		/// Constructs cardinal points for cross-plane placement offsets.
		/// </summary>
		protected override Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			// Use base-level points centered on each edge.
			CardinalPointSet set = new CardinalPointSet(
				positiveX: new Vector3(1f, 0f, 0.5f),
				negativeX: new Vector3(0f, 0f, 0.5f),
				positiveZ: new Vector3(0.5f, 0f, 1f),
				negativeZ: new Vector3(0.5f, 0f, 0f),
				stationary: new Vector3(0.5f, 0f, 0.5f));

			return new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>
			{
				[Erelia.Core.VoxelKit.FlipOrientation.PositiveY] = set,
				[Erelia.Core.VoxelKit.FlipOrientation.NegativeY] = set
			};
		}
	}
}


