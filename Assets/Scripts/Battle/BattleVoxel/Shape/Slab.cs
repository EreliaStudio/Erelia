using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.BattleVoxel.ShapeType
{
	[Serializable]
	public class Slab : Erelia.BattleVoxel.MaskShape
	{
		private const float Height = 0.5f;

		protected override Dictionary<VoxelKit.FlipOrientation, List<VoxelKit.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;

			var faces = new List<VoxelKit.Face>
			{
				VoxelKit.Utils.Geometry.CreateRectangle(
					new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, Height + maskOffset, 0f), UV = new Vector2(0f, 0f) },
					new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, Height + maskOffset, 0f), UV = new Vector2(1f, 0f) },
					new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, Height + maskOffset, 1f), UV = new Vector2(1f, 1f) },
					new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, Height + maskOffset, 1f), UV = new Vector2(0f, 1f) })
			};

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

		protected override Dictionary<VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
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

			return new Dictionary<VoxelKit.FlipOrientation, CardinalPointSet>
			{
				[VoxelKit.FlipOrientation.PositiveY] = nonFlipped,
				[VoxelKit.FlipOrientation.NegativeY] = flipped
			};
		}
	}
}


