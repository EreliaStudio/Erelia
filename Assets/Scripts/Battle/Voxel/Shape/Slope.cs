using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Battle.Voxel.ShapeType
{
	/// <summary>
	/// Mask shape for slope voxels.
	/// Defines a sloped top face and cardinal points for placement offsets.
	/// </summary>
	[Serializable]
	public class Slope : Erelia.Battle.Voxel.Mask.Shape
	{
		/// <summary>
		/// Constructs the mask faces for a slope voxel.
		/// </summary>
		protected override Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> ConstructMaskFaces()
		{
			// Build a sloped top face or flat top for flipped orientation.
			const float maskOffset = 0.01f;

			var faces = new List<Erelia.Core.VoxelKit.Face>();
			Erelia.Core.VoxelKit.Face slope = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f + maskOffset, 0f), TileUV = new Vector2(0f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f + maskOffset, 0f), TileUV = new Vector2(1f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), TileUV = new Vector2(1f, 1f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), TileUV = new Vector2(0f, 1f) });
			faces.Add(slope);

			var flippedFaces = new List<Erelia.Core.VoxelKit.Face>();
			Erelia.Core.VoxelKit.Face top = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), TileUV = new Vector2(0f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), TileUV = new Vector2(1f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), TileUV = new Vector2(1f, 1f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), TileUV = new Vector2(0f, 1f) });
			flippedFaces.Add(top);

			return new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>>
			{
				[Erelia.Core.VoxelKit.FlipOrientation.PositiveY] = faces,
				[Erelia.Core.VoxelKit.FlipOrientation.NegativeY] = flippedFaces
			};
		}

		/// <summary>
		/// Constructs cardinal points for slope placement offsets.
		/// </summary>
		protected override Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			// Provide points along the slope and fallback to full height when flipped.
			CardinalPointSet nonFlipped = new CardinalPointSet(
				positiveX: new Vector3(1f, 0.5f, 0.5f),
				negativeX: new Vector3(0f, 0.5f, 0.5f),
				positiveZ: new Vector3(0.5f, 1f, 1f),
				negativeZ: new Vector3(0.5f, 0f, 0f),
				stationary: new Vector3(0.5f, 0.5f, 0.5f));

			CardinalPointSet flipped = new CardinalPointSet(
				positiveX: new Vector3(1f, 1f, 0.5f),
				negativeX: new Vector3(0f, 1f, 0.5f),
				positiveZ: new Vector3(0.5f, 1f, 1f),
				negativeZ: new Vector3(0.5f, 1f, 0f),
				stationary: new Vector3(0.5f, 1f, 0.5f));

			return new Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet>
			{
				[Erelia.Core.VoxelKit.FlipOrientation.PositiveY] = nonFlipped,
				[Erelia.Core.VoxelKit.FlipOrientation.NegativeY] = flipped
			};
		}
	}
}


