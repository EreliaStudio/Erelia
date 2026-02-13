using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Voxel.Geometry
{
	[Serializable]
	public class Stair : Core.Voxel.Geometry.Shape
	{
		[SerializeField] private Sprite spriteFront;
		[SerializeField] private Sprite spriteBack;
		[SerializeField] private Sprite spriteBottom;
		[SerializeField] private Sprite spriteTop;
		[SerializeField] private Sprite spriteSideLeft;
		[SerializeField] private Sprite spriteSideRight;
		[SerializeField] private Sprite spriteStepTop;
		[SerializeField] private Sprite spriteStepRiser;

		private const float StepHeight = 0.5f;
		private const float StepDepth = 0.5f;

		protected override List<Core.Voxel.Model.Face> ConstructInnerFaces()
		{
			var faces = new List<Core.Voxel.Model.Face>();

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteStepTop, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			Vector2 halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
			Vector2 uvA = halfUvAnchor;
			Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			Vector2 uvC = halfUvAnchor + halfUvSize;
			Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face stepTop = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvD });
			faces.Add(stepTop);

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteStepRiser, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face stepRiser = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvD });
			faces.Add(stepRiser);

			return faces;
		}

		protected override Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			const float maskXOverhang = 0.01f;
			const float riserZOffset = 0.01f;
			const float uvStep = 1f / 3f;
			var faces = new List<Core.Voxel.Model.Face>();

			float upperY = 1f + maskOffset;
			float lowerY = StepHeight + maskOffset;
			float right = 1f + maskXOverhang;

			Core.Voxel.Model.Face upperTop = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, StepDepth), UV = new Vector2(0f, uvStep * 2f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(right, upperY, StepDepth), UV = new Vector2(1f, uvStep * 2f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(right, upperY, 1f), UV = new Vector2(1f, 1f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(upperTop);

			float riserZ = StepDepth - riserZOffset;
			Core.Voxel.Model.Face upperRiser = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, riserZ), UV = new Vector2(0f, uvStep) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(right, lowerY, riserZ), UV = new Vector2(1f, uvStep) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(right, upperY, riserZ), UV = new Vector2(1f, uvStep * 2f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, riserZ), UV = new Vector2(0f, uvStep * 2f) });
			faces.Add(upperRiser);

			Core.Voxel.Model.Face lowerTop = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, 0f), UV = new Vector2(0f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, 0f), UV = new Vector2(1f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, StepDepth), UV = new Vector2(1f, uvStep) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, StepDepth), UV = new Vector2(0f, uvStep) });
			faces.Add(lowerTop);

			var flippedFaces = new List<Core.Voxel.Model.Face>();
			Core.Voxel.Model.Face top = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			flippedFaces.Add(top);

			return new Dictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>>
			{
				[Core.Voxel.Model.FlipOrientation.PositiveY] = faces,
				[Core.Voxel.Model.FlipOrientation.NegativeY] = flippedFaces
			};
		}

		protected override Dictionary<AxisPlane, Core.Voxel.Model.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<AxisPlane, Core.Voxel.Model.Face>();
			Vector2 uvAnchor = Vector2.zero;
			Vector2 uvSize = Vector2.zero;
			Vector2 halfUvSize = Vector2.zero;
			Vector2 halfUvAnchor = Vector2.zero;
			Vector2 uvA = Vector2.zero;
			Vector2 uvB = Vector2.zero;
			Vector2 uvC = Vector2.zero;
			Vector2 uvD = Vector2.zero;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
			float u0 = uvAnchor.x;
			float u1 = uvAnchor.x + uvSize.x * 0.5f;
			float u2 = uvAnchor.x + uvSize.x;
			float v0 = uvAnchor.y;
			float v1 = uvAnchor.y + uvSize.y * 0.5f;
			float v2 = uvAnchor.y + uvSize.y;
			var posX = new Core.Voxel.Model.Face();
			posX.AddPolygon(new List<Core.Voxel.Model.Face.Vertex>
		{
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = new Vector2(u2, v0) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, StepHeight, 0f), TileUV = new Vector2(u2, v1) }
		});
			posX.AddPolygon(new List<Core.Voxel.Model.Face.Vertex>
		{
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = new Vector2(u0, v0) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = new Vector2(u0, v2) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(1f, 1f, StepDepth), TileUV = new Vector2(u1, v2) }
		});
			faces[AxisPlane.PosX] = posX;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
			u0 = uvAnchor.x;
			u1 = uvAnchor.x + uvSize.x * 0.5f;
			u2 = uvAnchor.x + uvSize.x;
			v0 = uvAnchor.y;
			v1 = uvAnchor.y + uvSize.y * 0.5f;
			v2 = uvAnchor.y + uvSize.y;
			var negX = new Core.Voxel.Model.Face();
			negX.AddPolygon(new List<Core.Voxel.Model.Face.Vertex>
		{
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = new Vector2(u2, v0) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, StepHeight, 0f), TileUV = new Vector2(u2, v1) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) }
		});
			negX.AddPolygon(new List<Core.Voxel.Model.Face.Vertex>
		{
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, 1f, StepDepth), TileUV = new Vector2(u1, v2) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = new Vector2(u0, v2) },
			new Core.Voxel.Model.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = new Vector2(u0, v0) }
		});
			faces[AxisPlane.NegX] = negX;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteFront, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face negZ = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvD });
			faces[AxisPlane.NegZ] = negZ;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteBack, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Core.Voxel.Model.Face posZ = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteStepTop, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Core.Voxel.Model.Face posY = Core.Utils.Geometry.CreateRectangle(
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvA },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvB },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Core.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
			faces[AxisPlane.PosY] = posY;

			Core.Utils.SpriteUv.GetSpriteUvRect(spriteBottom, out uvAnchor, out uvSize);
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

			return faces;
		}
	}
}
