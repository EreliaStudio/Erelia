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

		protected override Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			const float maskXOverhang = 0.01f;
			const float riserZOffset = 0.01f;
			const float uvStep = 1f / 3f;

			var faces = new List<Erelia.Voxel.Face>();

			float upperY = 1f + maskOffset;
			float lowerY = StepHeight + maskOffset;
			float right = 1f + maskXOverhang;

			Erelia.Voxel.Face upperTop = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, StepDepth), UV = new Vector2(0f, uvStep * 2f) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, upperY, StepDepth), UV = new Vector2(1f, uvStep * 2f) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, upperY, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(upperTop);

			float riserZ = StepDepth - riserZOffset;
			Erelia.Voxel.Face upperRiser = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, riserZ), UV = new Vector2(0f, uvStep) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, lowerY, riserZ), UV = new Vector2(1f, uvStep) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, upperY, riserZ), UV = new Vector2(1f, uvStep * 2f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, riserZ), UV = new Vector2(0f, uvStep * 2f) });
			faces.Add(upperRiser);

			Erelia.Voxel.Face lowerTop = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, StepDepth), UV = new Vector2(1f, uvStep) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, StepDepth), UV = new Vector2(0f, uvStep) });
			faces.Add(lowerTop);

			var flippedFaces = new List<Erelia.Voxel.Face>();
			Erelia.Voxel.Face top = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			flippedFaces.Add(top);

			return new Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>>
			{
				[Erelia.Voxel.FlipOrientation.PositiveY] = faces,
				[Erelia.Voxel.FlipOrientation.NegativeY] = flippedFaces
			};
		}

		protected override Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> ConstructCardinalPoints()
		{
			var nonFlipped = new Dictionary<Erelia.Voxel.CardinalPoint, Vector3>
			{
				[Erelia.Voxel.CardinalPoint.PositiveX] = new Vector3(1f, 0.5f, 0.5f),
				[Erelia.Voxel.CardinalPoint.NegativeX] = new Vector3(0f, 0.5f, 0.5f),
				[Erelia.Voxel.CardinalPoint.PositiveZ] = new Vector3(0.5f, 1f, 0.75f),
				[Erelia.Voxel.CardinalPoint.NegativeZ] = new Vector3(0.5f, 0.5f, 0.5f),
				[Erelia.Voxel.CardinalPoint.Stationary] = new Vector3(0.5f, 0.5f, 0.5f),
			};

			var flipped = new Dictionary<Erelia.Voxel.CardinalPoint, Vector3>
			{
				[Erelia.Voxel.CardinalPoint.PositiveX] = new Vector3(1f, 1f, 0.5f),
				[Erelia.Voxel.CardinalPoint.NegativeX] = new Vector3(0f, 1f, 0.5f),
				[Erelia.Voxel.CardinalPoint.PositiveZ] = new Vector3(0.5f, 1f, 1f),
				[Erelia.Voxel.CardinalPoint.NegativeZ] = new Vector3(0.5f, 1f, 0f),
				[Erelia.Voxel.CardinalPoint.Stationary] = new Vector3(0.5f, 1f, 0.5f),
			};

			return new Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>>
			{
				[Erelia.Voxel.FlipOrientation.PositiveY] = nonFlipped,
				[Erelia.Voxel.FlipOrientation.NegativeY] = flipped
			};
		}
	}
}
