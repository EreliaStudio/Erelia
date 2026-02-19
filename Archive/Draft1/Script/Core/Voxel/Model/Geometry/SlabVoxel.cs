using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Voxel.Geometry
{
	[Serializable]
	public class Slab : Core.Voxel.Geometry.Shape
	{
		[SerializeField] private Sprite spritePosX;
		[SerializeField] private Sprite spriteNegX;
		[SerializeField] private Sprite spritePosY;
		[SerializeField] private Sprite spriteNegY;
		[SerializeField] private Sprite spritePosZ;
		[SerializeField] private Sprite spriteNegZ;

		protected override List<Core.Voxel.Model.Face> ConstructInnerFaces()
		{
			var faces = new List<Core.Voxel.Model.Face>();

			Core.Utils.SpriteUv.GetSpriteUvRect(spritePosY, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Core.Voxel.Model.Face posY = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 0f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 1f), UV = uvD });
			faces.Add(posY);

			return faces;
		}

		protected override Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			float height = 0.5f + maskOffset;
			var faces = new List<Core.Voxel.Model.Face>();
			Core.Voxel.Model.Face top = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, height, 0f), UV = new Vector2(0f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, height, 0f), UV = new Vector2(1f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, height, 1f), UV = new Vector2(1f, 1f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, height, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(top);
			const float flippedHeight = 1f + maskOffset;
			var flippedFaces = new List<Core.Voxel.Model.Face>();
			Core.Voxel.Model.Face flippedTop = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, flippedHeight, 0f), UV = new Vector2(0f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, flippedHeight, 0f), UV = new Vector2(1f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, flippedHeight, 1f), UV = new Vector2(1f, 1f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, flippedHeight, 1f), UV = new Vector2(0f, 1f) });
			flippedFaces.Add(flippedTop);

			return new Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>>
			{
				[Core.Voxel.Model.FlipOrientation.PositiveY] = faces,
				[Core.Voxel.Model.FlipOrientation.NegativeY] = flippedFaces
			};
		}

		protected override Dictionary<AxisPlane, Core.Voxel.Model.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<AxisPlane, Core.Voxel.Model.Face>();

			Core.Utils.SpriteUv.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			Vector2 halfUvAnchor = uvAnchor;
			Vector2 uvA = halfUvAnchor;
			Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			Vector2 uvC = halfUvAnchor + halfUvSize;
			Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face posX = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 0f), UV = uvD });
			faces[AxisPlane.PosX] = posX;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face negX = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 0f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });
			faces[AxisPlane.NegX] = negX;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Core.Voxel.Model.Face negY = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });
			faces[AxisPlane.NegY] = negY;

			Core.Utils.SpriteUv.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face posZ = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 1f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face negZ = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 0f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 0f), UV = uvD });
			faces[AxisPlane.NegZ] = negZ;

			return faces;
		}

		protected override Core.Voxel.Model.CardinalPointSet ConstructCardinalPoints()
		{
			return new Core.Voxel.Model.CardinalPointSet(
				positiveX: new Vector3(1f, 0.5f, 0.5f),
				negativeX: new Vector3(0f, 0.5f, 0.5f),
				positiveZ: new Vector3(0.5f, 0.5f, 1f),
				negativeZ: new Vector3(0.5f, 0.5f, 0f),
				stationary: new Vector3(0.5f, 0.5f, 0.5f));
		}
	}
}
