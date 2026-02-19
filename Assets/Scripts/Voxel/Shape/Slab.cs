using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel.ShapeType
{
	[Serializable]
	public class Slab : Voxel.Shape
	{
		[Header("Textures")]
		[SerializeField] private Sprite spritePosX;
		[SerializeField] private Sprite spriteNegX;
		[SerializeField] private Sprite spritePosY;
		[SerializeField] private Sprite spriteNegY;
		[SerializeField] private Sprite spritePosZ;
		[SerializeField] private Sprite spriteNegZ;

		private const float Height = 0.5f;

		protected override FaceSet ConstructRenderFaces()
		{
			return new FaceSet(
				inner: new List<Voxel.Face>(),
				outerShell: ConstructTexturedOuterShell());
		}

		private Dictionary<AxisPlane, Voxel.Face> ConstructTexturedOuterShell()
		{
			var faces = new Dictionary<AxisPlane, Voxel.Face>();

			Utils.SpriteUv.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			faces[AxisPlane.PosX] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, Height, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, Height, 0f), UV = uvD });

			Utils.SpriteUv.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			faces[AxisPlane.NegX] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, Height, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, Height, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });

			Utils.SpriteUv.GetSpriteUvRect(spritePosY, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			faces[AxisPlane.PosY] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, Height, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, Height, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, Height, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, Height, 1f), UV = uvD });

			Utils.SpriteUv.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			faces[AxisPlane.NegY] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });

			Utils.SpriteUv.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			faces[AxisPlane.PosZ] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, Height, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, Height, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });

			Utils.SpriteUv.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			faces[AxisPlane.NegZ] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, Height, 0f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, Height, 0f), UV = uvD });

			return faces;
		}

		protected override Dictionary<Voxel.FlipOrientation, List<Voxel.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;

			var faces = new List<Voxel.Face>
			{
				Utils.Geometry.CreateRectangle(
					new Utils.Geometry.Vertex { Position = new Vector3(0f, Height + maskOffset, 0f), UV = new Vector2(0f, 0f) },
					new Utils.Geometry.Vertex { Position = new Vector3(1f, Height + maskOffset, 0f), UV = new Vector2(1f, 0f) },
					new Utils.Geometry.Vertex { Position = new Vector3(1f, Height + maskOffset, 1f), UV = new Vector2(1f, 1f) },
					new Utils.Geometry.Vertex { Position = new Vector3(0f, Height + maskOffset, 1f), UV = new Vector2(0f, 1f) })
			};

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
				[Voxel.CardinalPoint.PositiveX] = new Vector3(1f, Height, 0.5f),
				[Voxel.CardinalPoint.NegativeX] = new Vector3(0f, Height, 0.5f),
				[Voxel.CardinalPoint.PositiveZ] = new Vector3(0.5f, Height, 1f),
				[Voxel.CardinalPoint.NegativeZ] = new Vector3(0.5f, Height, 0f),
				[Voxel.CardinalPoint.Stationary] = new Vector3(0.5f, Height, 0.5f),
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
	}
}
