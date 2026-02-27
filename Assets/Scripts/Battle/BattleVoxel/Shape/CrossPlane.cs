using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.BattleVoxel.ShapeType
{
	[Serializable]
	public class CrossPlane : Erelia.BattleVoxel.MaskShape
	{
		protected override Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<VoxelKit.Face>();
			VoxelKit.Face top = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(top);

			return new Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>>
			{
				[VoxelKit.FlipOrientation.PositiveY] = faces,
				[VoxelKit.FlipOrientation.NegativeY] = faces
			};
		}

		protected override Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>> ConstructCardinalPoints()
		{
			var points = new Dictionary<VoxelKit.CardinalPoint, Vector3>
			{
				[VoxelKit.CardinalPoint.PositiveX] = new Vector3(1f, 0f, 0.5f),
				[VoxelKit.CardinalPoint.NegativeX] = new Vector3(0f, 0f, 0.5f),
				[VoxelKit.CardinalPoint.PositiveZ] = new Vector3(0.5f, 0f, 1f),
				[VoxelKit.CardinalPoint.NegativeZ] = new Vector3(0.5f, 0f, 0f),
				[VoxelKit.CardinalPoint.Stationary] = new Vector3(0.5f, 0f, 0.5f),
			};

			return new Dictionary<VoxelKit.FlipOrientation, Dictionary<VoxelKit.CardinalPoint, Vector3>>
			{
				[VoxelKit.FlipOrientation.PositiveY] = points,
				[VoxelKit.FlipOrientation.NegativeY] = points
			};
		}
	}
}


