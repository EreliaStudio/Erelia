using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Voxel.Geometry
{
	[Serializable]
	public class CrossPlane : Core.Voxel.Geometry.Shape
	{
		[SerializeField] private Sprite sprite;

		protected override List<Core.Voxel.Model.Face> ConstructInnerFaces()
		{
			var faces = new List<Core.Voxel.Model.Face>();

			Core.Utils.SpriteUv.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Core.Voxel.Model.Face planeA = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
			faces.Add(planeA);
			Core.Voxel.Model.Face planeABack = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB });
			faces.Add(planeABack);

			Core.Voxel.Model.Face planeB = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
			faces.Add(planeB);
			Core.Voxel.Model.Face planeBBack = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB });
			faces.Add(planeBBack);

			return faces;
		}

		protected override Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<Core.Voxel.Model.Face>();
			Core.Voxel.Model.Face top = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(top);
			return new Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>>
			{
				[Core.Voxel.Model.FlipOrientation.PositiveY] = faces,
				[Core.Voxel.Model.FlipOrientation.NegativeY] = new List<Core.Voxel.Model.Face>(faces)
			};
		}

		protected override Dictionary<AxisPlane, Core.Voxel.Model.Face> ConstructOuterShellFaces()
		{
			return new Dictionary<AxisPlane, Core.Voxel.Model.Face>();
		}

		protected override Core.Voxel.Model.CardinalPointSet ConstructCardinalPoints()
		{
			return new Core.Voxel.Model.CardinalPointSet(
				positiveX: new Vector3(1f, 1f, 0.5f),
				negativeX: new Vector3(0f, 1f, 0.5f),
				positiveZ: new Vector3(0.5f, 1f, 1f),
				negativeZ: new Vector3(0.5f, 1f, 0f),
				stationary: new Vector3(0.5f, 1f, 0.5f));
		}
	}
}
