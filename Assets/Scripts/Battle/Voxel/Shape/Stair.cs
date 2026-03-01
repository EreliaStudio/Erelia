using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Battle.Voxel.ShapeType
{
	[Serializable]
	public class Stair : Erelia.Battle.Voxel.MaskShape
	{
		private const float StepHeight = 0.5f;
		private const float StepDepth = 0.5f;

		protected override Dictionary<Erelia.Core.VoxelKit.FlipOrientation, List<Erelia.Core.VoxelKit.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			const float maskXOverhang = 0.01f;
			const float riserZOffset = 0.01f;
			const float uvStep = 1f / 3f;

			var faces = new List<Erelia.Core.VoxelKit.Face>();

			float upperY = 1f + maskOffset;
			float lowerY = StepHeight + maskOffset;
			float right = 1f + maskXOverhang;

			Erelia.Core.VoxelKit.Face upperTop = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, upperY, StepDepth), TileUV = new Vector2(0f, uvStep * 2f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(right, upperY, StepDepth), TileUV = new Vector2(1f, uvStep * 2f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(right, upperY, 1f), TileUV = new Vector2(1f, 1f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, upperY, 1f), TileUV = new Vector2(0f, 1f) });
			faces.Add(upperTop);

			float riserZ = StepDepth - riserZOffset;
			Erelia.Core.VoxelKit.Face upperRiser = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, lowerY, riserZ), TileUV = new Vector2(0f, uvStep) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(right, lowerY, riserZ), TileUV = new Vector2(1f, uvStep) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(right, upperY, riserZ), TileUV = new Vector2(1f, uvStep * 2f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, upperY, riserZ), TileUV = new Vector2(0f, uvStep * 2f) });
			faces.Add(upperRiser);

			Erelia.Core.VoxelKit.Face lowerTop = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, lowerY, 0f), TileUV = new Vector2(0f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, lowerY, 0f), TileUV = new Vector2(1f, 0f) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, lowerY, StepDepth), TileUV = new Vector2(1f, uvStep) },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, lowerY, StepDepth), TileUV = new Vector2(0f, uvStep) });
			faces.Add(lowerTop);

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
				positiveX: new Vector3(1f, 0.5f, 0.5f),
				negativeX: new Vector3(0f, 0.5f, 0.5f),
				positiveZ: new Vector3(0.5f, 1f, 0.75f),
				negativeZ: new Vector3(0.5f, 0.5f, 0.5f),
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


