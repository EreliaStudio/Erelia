using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.BattleVoxel.ShapeType
{
	[Serializable]
	public class Cube : Erelia.BattleVoxel.MaskShape
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

		protected override Dictionary<VoxelKit.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			CardinalPointSet set = new CardinalPointSet(
				positiveX: new Vector3(1f, 1f, 0.5f),
				negativeX: new Vector3(0f, 1f, 0.5f),
				positiveZ: new Vector3(0.5f, 1f, 1f),
				negativeZ: new Vector3(0.5f, 1f, 0f),
				stationary: new Vector3(0.5f, 1f, 0.5f));

			return new Dictionary<VoxelKit.FlipOrientation, CardinalPointSet>
			{
				[VoxelKit.FlipOrientation.PositiveY] = set,
				[VoxelKit.FlipOrientation.NegativeY] = set
			};
		}
	}
}


