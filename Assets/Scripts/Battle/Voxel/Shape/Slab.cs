using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel.ShapeType
{
	[Serializable]
	public class Slab : Erelia.Battle.Voxel.MaskShape
	{
		private const float Height = 0.5f;

		protected override Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;

			var faces = new List<Erelia.Core.VoxelKit.Face>
			{
				Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
					new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, Height + maskOffset, 0f), TileUV = new Vector2(0f, 0f) },
					new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, Height + maskOffset, 0f), TileUV = new Vector2(1f, 0f) },
					new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, Height + maskOffset, 1f), TileUV = new Vector2(1f, 1f) },
					new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, Height + maskOffset, 1f), TileUV = new Vector2(0f, 1f) })
			};

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

		protected override Dictionary<Erelia.Core.VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			CardinalPointSet nonFlipped = new CardinalPointSet(
				positiveX: new Vector3(1f, Height, 0.5f),
				negativeX: new Vector3(0f, Height, 0.5f),
				positiveZ: new Vector3(0.5f, Height, 1f),
				negativeZ: new Vector3(0.5f, Height, 0f),
				stationary: new Vector3(0.5f, Height, 0.5f));

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


