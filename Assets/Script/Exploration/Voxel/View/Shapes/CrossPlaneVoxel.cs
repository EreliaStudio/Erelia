using System.Collections.Generic;
using UnityEngine;
using System;

namespace Voxel.View
{
	[Serializable]
	public class CrossPlane : Voxel.View.Shape
	{
		[SerializeField] private Sprite sprite;

		protected override List<Voxel.Model.Face> ConstructInnerFaces()
		{
			var faces = new List<Voxel.Model.Face>();

			Utils.SpriteUv.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Voxel.Model.Face planeA = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
			faces.Add(planeA);
			Voxel.Model.Face planeABack = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB });
			faces.Add(planeABack);

			Voxel.Model.Face planeB = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
			faces.Add(planeB);
			Voxel.Model.Face planeBBack = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB });
			faces.Add(planeBBack);

			return faces;
		}

		protected override Dictionary<Voxel.Model.FlipOrientation, List<Voxel.Model.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<Voxel.Model.Face>();
			Voxel.Model.Face top = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(top);
			return new Dictionary<Voxel.Model.FlipOrientation, List<Voxel.Model.Face>>
			{
				[Voxel.Model.FlipOrientation.PositiveY] = faces,
				[Voxel.Model.FlipOrientation.NegativeY] = new List<Voxel.Model.Face>(faces)
			};
		}

		protected override Dictionary<AxisPlane, Voxel.Model.Face> ConstructOuterShellFaces()
		{
			return new Dictionary<AxisPlane, Voxel.Model.Face>();
		}
	}
}
