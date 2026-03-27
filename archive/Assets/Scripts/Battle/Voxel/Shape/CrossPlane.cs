using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Battle.Voxel.ShapeType
{
	[Serializable]
	public class CrossPlane : Erelia.Battle.Voxel.Mask.Shape
	{
		protected override Dictionary<Erelia.Core.Voxel.FlipOrientation, List<Erelia.Core.Voxel.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<Erelia.Core.Voxel.Face>();
			Erelia.Core.Voxel.Face top = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), TileUV = new Vector2(0f, 0f) },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), TileUV = new Vector2(1f, 0f) },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), TileUV = new Vector2(1f, 1f) },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), TileUV = new Vector2(0f, 1f) });
			faces.Add(top);

			return new Dictionary<Erelia.Core.Voxel.FlipOrientation, List<Erelia.Core.Voxel.Face>>
			{
				[Erelia.Core.Voxel.FlipOrientation.PositiveY] = faces,
				[Erelia.Core.Voxel.FlipOrientation.NegativeY] = faces
			};
		}

		protected override Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet> ConstructCardinalPoints()
		{
			CardinalPointSet set = new CardinalPointSet(
				positiveX: new Vector3(1f, 0f, 0.5f),
				negativeX: new Vector3(0f, 0f, 0.5f),
				positiveZ: new Vector3(0.5f, 0f, 1f),
				negativeZ: new Vector3(0.5f, 0f, 0f),
				stationary: new Vector3(0.5f, 0f, 0.5f));

			return new Dictionary<Erelia.Core.Voxel.FlipOrientation, CardinalPointSet>
			{
				[Erelia.Core.Voxel.FlipOrientation.PositiveY] = set,
				[Erelia.Core.Voxel.FlipOrientation.NegativeY] = set
			};
		}
	}
}


