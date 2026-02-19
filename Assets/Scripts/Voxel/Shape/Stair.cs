using System.Collections.Generic;
using UnityEngine;
using System;

namespace Voxel.ShapeType
{
	[Serializable]
	public class Stair : Voxel.Shape
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

		protected override FaceSet ConstructRenderFaces()
		{
			return new FaceSet(
				inner: ConstructInnerFaces(),
				outerShell: ConstructOuterShellFaces());
		}

		protected override Dictionary<Voxel.FlipOrientation, List<Voxel.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			const float maskXOverhang = 0.01f;
			const float riserZOffset = 0.01f;
			const float uvStep = 1f / 3f;

			var faces = new List<Voxel.Face>();

			float upperY = 1f + maskOffset;
			float lowerY = StepHeight + maskOffset;
			float right = 1f + maskXOverhang;

			Voxel.Face upperTop = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, StepDepth), UV = new Vector2(0f, uvStep * 2f) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, upperY, StepDepth), UV = new Vector2(1f, uvStep * 2f) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, upperY, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(upperTop);

			float riserZ = StepDepth - riserZOffset;
			Voxel.Face upperRiser = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, riserZ), UV = new Vector2(0f, uvStep) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, lowerY, riserZ), UV = new Vector2(1f, uvStep) },
				new Utils.Geometry.Vertex { Position = new Vector3(right, upperY, riserZ), UV = new Vector2(1f, uvStep * 2f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, upperY, riserZ), UV = new Vector2(0f, uvStep * 2f) });
			faces.Add(upperRiser);

			Voxel.Face lowerTop = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, lowerY, StepDepth), UV = new Vector2(1f, uvStep) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, lowerY, StepDepth), UV = new Vector2(0f, uvStep) });
			faces.Add(lowerTop);

			var flippedFaces = new List<Voxel.Face>();
			Voxel.Face top = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			flippedFaces.Add(top);

			return new Dictionary<Voxel.FlipOrientation, List<Voxel.Face>>
			{
				[Voxel.FlipOrientation.PositiveY] = faces,
				[Voxel.FlipOrientation.NegativeY] = flippedFaces
			};
		}

		protected override Dictionary<Voxel.FlipOrientation, Dictionary<Voxel.CardinalPoint, Vector3>> ConstructCardinalPoints()
		{
			var nonFlipped = new Dictionary<Voxel.CardinalPoint, Vector3>
			{
				[Voxel.CardinalPoint.PositiveX] = new Vector3(1f, 0.5f, 0.5f),
				[Voxel.CardinalPoint.NegativeX] = new Vector3(0f, 0.5f, 0.5f),
				[Voxel.CardinalPoint.PositiveZ] = new Vector3(0.5f, 1f, 0.75f),
				[Voxel.CardinalPoint.NegativeZ] = new Vector3(0.5f, 0.5f, 0.5f),
				[Voxel.CardinalPoint.Stationary] = new Vector3(0.5f, 0.5f, 0.5f),
			};

			var flipped = new Dictionary<Voxel.CardinalPoint, Vector3>
			{
				[Voxel.CardinalPoint.PositiveX] = new Vector3(1f, 1f, 0.5f),
				[Voxel.CardinalPoint.NegativeX] = new Vector3(0f, 1f, 0.5f),
				[Voxel.CardinalPoint.PositiveZ] = new Vector3(0.5f, 1f, 1f),
				[Voxel.CardinalPoint.NegativeZ] = new Vector3(0.5f, 1f, 0f),
				[Voxel.CardinalPoint.Stationary] = new Vector3(0.5f, 1f, 0.5f),
			};

			return new Dictionary<Voxel.FlipOrientation, Dictionary<Voxel.CardinalPoint, Vector3>>
			{
				[Voxel.FlipOrientation.PositiveY] = nonFlipped,
				[Voxel.FlipOrientation.NegativeY] = flipped
			};
		}

		private List<Voxel.Face> ConstructInnerFaces()
		{
			var faces = new List<Voxel.Face>();

			Utils.SpriteUv.GetSpriteUvRect(spriteStepTop, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			Vector2 halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);

			Vector2 uvA = halfUvAnchor;
			Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			Vector2 uvC = halfUvAnchor + halfUvSize;
			Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			Voxel.Face stepTop = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvD });
			faces.Add(stepTop);

			Utils.SpriteUv.GetSpriteUvRect(spriteStepRiser, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;

			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			Voxel.Face stepRiser = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvD });
			faces.Add(stepRiser);

			return faces;
		}

		private Dictionary<AxisPlane, Voxel.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<AxisPlane, Voxel.Face>();

			Vector2 uvAnchor = Vector2.zero;
			Vector2 uvSize = Vector2.zero;
			Vector2 halfUvSize = Vector2.zero;
			Vector2 halfUvAnchor = Vector2.zero;
			Vector2 uvA = Vector2.zero;
			Vector2 uvB = Vector2.zero;
			Vector2 uvC = Vector2.zero;
			Vector2 uvD = Vector2.zero;

			Utils.SpriteUv.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
			float u0 = uvAnchor.x;
			float u1 = uvAnchor.x + uvSize.x * 0.5f;
			float u2 = uvAnchor.x + uvSize.x;
			float v0 = uvAnchor.y;
			float v1 = uvAnchor.y + uvSize.y * 0.5f;
			float v2 = uvAnchor.y + uvSize.y;

			var posX = new Voxel.Face();
			posX.AddPolygon(new List<Voxel.Face.Vertex>
			{
				new Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = new Vector2(u2, v0) },
				new Voxel.Face.Vertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
				new Voxel.Face.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
				new Voxel.Face.Vertex { Position = new Vector3(1f, StepHeight, 0f), TileUV = new Vector2(u2, v1) }
			});
			posX.AddPolygon(new List<Voxel.Face.Vertex>
			{
				new Voxel.Face.Vertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
				new Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = new Vector2(u0, v0) },
				new Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = new Vector2(u0, v2) },
				new Voxel.Face.Vertex { Position = new Vector3(1f, 1f, StepDepth), TileUV = new Vector2(u1, v2) }
			});
			faces[AxisPlane.PosX] = posX;

			Utils.SpriteUv.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
			u0 = uvAnchor.x;
			u1 = uvAnchor.x + uvSize.x * 0.5f;
			u2 = uvAnchor.x + uvSize.x;
			v0 = uvAnchor.y;
			v1 = uvAnchor.y + uvSize.y * 0.5f;
			v2 = uvAnchor.y + uvSize.y;

			var negX = new Voxel.Face();
			negX.AddPolygon(new List<Voxel.Face.Vertex>
			{
				new Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = new Vector2(u2, v0) },
				new Voxel.Face.Vertex { Position = new Vector3(0f, StepHeight, 0f), TileUV = new Vector2(u2, v1) },
				new Voxel.Face.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
				new Voxel.Face.Vertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) }
			});
			negX.AddPolygon(new List<Voxel.Face.Vertex>
			{
				new Voxel.Face.Vertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
				new Voxel.Face.Vertex { Position = new Vector3(0f, 1f, StepDepth), TileUV = new Vector2(u1, v2) },
				new Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = new Vector2(u0, v2) },
				new Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = new Vector2(u0, v0) }
			});
			faces[AxisPlane.NegX] = negX;

			Utils.SpriteUv.GetSpriteUvRect(spriteFront, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			Voxel.Face negZ = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvD });
			faces[AxisPlane.NegZ] = negZ;

			Utils.SpriteUv.GetSpriteUvRect(spriteBack, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Voxel.Face posZ = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			Utils.SpriteUv.GetSpriteUvRect(spriteStepTop, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			Voxel.Face posY = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
			faces[AxisPlane.PosY] = posY;

			Utils.SpriteUv.GetSpriteUvRect(spriteBottom, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Voxel.Face negY = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });
			faces[AxisPlane.NegY] = negY;

			return faces;
		}
	}
}
