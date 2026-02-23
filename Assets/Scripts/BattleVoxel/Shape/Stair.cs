using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.BattleVoxel.ShapeType
{
	[Serializable]
	public class Stair : Erelia.BattleVoxel.MaskShape
	{
		private const float StepHeight = 0.5f;
		private const float StepDepth = 0.5f;

		protected override Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			const float maskXOverhang = 0.01f;
			const float riserZOffset = 0.01f;
			const float uvStep = 1f / 3f;

			var faces = new List<VoxelKit.Face>();

			float upperY = 1f + maskOffset;
			float lowerY = StepHeight + maskOffset;
			float right = 1f + maskXOverhang;

			VoxelKit.Face upperTop = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, StepDepth), UV = new Vector2(0f, uvStep * 2f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(right, upperY, StepDepth), UV = new Vector2(1f, uvStep * 2f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(right, upperY, 1f), UV = new Vector2(1f, 1f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(upperTop);

			float riserZ = StepDepth - riserZOffset;
			VoxelKit.Face upperRiser = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, riserZ), UV = new Vector2(0f, uvStep) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(right, lowerY, riserZ), UV = new Vector2(1f, uvStep) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(right, upperY, riserZ), UV = new Vector2(1f, uvStep * 2f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, riserZ), UV = new Vector2(0f, uvStep * 2f) });
			faces.Add(upperRiser);

			VoxelKit.Face lowerTop = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, 0f), UV = new Vector2(0f, 0f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, 0f), UV = new Vector2(1f, 0f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, StepDepth), UV = new Vector2(1f, uvStep) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, StepDepth), UV = new Vector2(0f, uvStep) });
			faces.Add(lowerTop);

			var flippedFaces = new List<VoxelKit.Face>();
			VoxelKit.Face top = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			flippedFaces.Add(top);

			return new Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>>
			{
				[VoxelKit.FlipOrientation.PositiveY] = faces,
				[VoxelKit.FlipOrientation.NegativeY] = flippedFaces
			};
		}

		protected override Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>> ConstructCardinalPoints()
		{
			var nonFlipped = new Dictionary<VoxelKit.CardinalPoint, Vector3>
			{
				[VoxelKit.CardinalPoint.PositiveX] = new Vector3(1f, 0.5f, 0.5f),
				[VoxelKit.CardinalPoint.NegativeX] = new Vector3(0f, 0.5f, 0.5f),
				[VoxelKit.CardinalPoint.PositiveZ] = new Vector3(0.5f, 1f, 0.75f),
				[VoxelKit.CardinalPoint.NegativeZ] = new Vector3(0.5f, 0.5f, 0.5f),
				[VoxelKit.CardinalPoint.Stationary] = new Vector3(0.5f, 0.5f, 0.5f),
			};

			var flipped = new Dictionary<VoxelKit.CardinalPoint, Vector3>
			{
				[VoxelKit.CardinalPoint.PositiveX] = new Vector3(1f, 1f, 0.5f),
				[VoxelKit.CardinalPoint.NegativeX] = new Vector3(0f, 1f, 0.5f),
				[VoxelKit.CardinalPoint.PositiveZ] = new Vector3(0.5f, 1f, 1f),
				[VoxelKit.CardinalPoint.NegativeZ] = new Vector3(0.5f, 1f, 0f),
				[VoxelKit.CardinalPoint.Stationary] = new Vector3(0.5f, 1f, 0.5f),
			};

			return new Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>>
			{
				[VoxelKit.FlipOrientation.PositiveY] = nonFlipped,
				[VoxelKit.FlipOrientation.NegativeY] = flipped
			};
		}
	}
}


