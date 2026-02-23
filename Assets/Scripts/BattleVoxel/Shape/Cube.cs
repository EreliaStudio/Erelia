using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.BattleVoxel.ShapeType
{
	[Serializable]
	public class Cube : Erelia.BattleVoxel.MaskShape
	{
		protected override Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<Erelia.Voxel.Face>();
			Erelia.Voxel.Face top = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(top);

			return new Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>>
			{
				[Erelia.Voxel.FlipOrientation.PositiveY] = faces,
				[Erelia.Voxel.FlipOrientation.NegativeY] = faces
			};
		}

		protected override Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> ConstructCardinalPoints()
		{
			var points = new Dictionary<Erelia.Voxel.CardinalPoint, Vector3>
			{
				[Erelia.Voxel.CardinalPoint.PositiveX] = new Vector3(1f, 1f, 0.5f),
				[Erelia.Voxel.CardinalPoint.NegativeX] = new Vector3(0f, 1f, 0.5f),
				[Erelia.Voxel.CardinalPoint.PositiveZ] = new Vector3(0.5f, 1f, 1f),
				[Erelia.Voxel.CardinalPoint.NegativeZ] = new Vector3(0.5f, 1f, 0f),
				[Erelia.Voxel.CardinalPoint.Stationary] = new Vector3(0.5f, 1f, 0.5f),
			};

			return new Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>>
			{
				[Erelia.Voxel.FlipOrientation.PositiveY] = points,
				[Erelia.Voxel.FlipOrientation.NegativeY] = points
			};
		}
	}
}
