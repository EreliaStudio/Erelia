using System.Collections.Generic;
using UnityEngine;
using System;

namespace Voxel.View
{
	[Serializable]
	public class Slope : Voxel.View.Shape
	{
		[SerializeField] private Sprite spriteBack;
		[SerializeField] private Sprite spriteBottom;
		[SerializeField] private Sprite spriteSlope;
		[SerializeField] private Sprite spriteSideLeft;
		[SerializeField] private Sprite spriteSideRight;

		protected override List<Voxel.View.Face> ConstructInnerFaces()
		{
			var faces = new List<Voxel.View.Face>();

			Utils.SpriteUv.GetSpriteUvRect(spriteSlope, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Voxel.View.Face slope = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
			faces.Add(slope);

			return faces;
		}

		protected override List<Voxel.View.Face> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<Voxel.View.Face>();
			Voxel.View.Face slope = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(slope);
			return faces;
		}

		protected override List<Voxel.View.Face> ConstructFlippedMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<Voxel.View.Face>();
			Voxel.View.Face top = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(top);
			return faces;
		}

		protected override Dictionary<OuterShellPlane, Voxel.View.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<OuterShellPlane, Voxel.View.Face>();
			Vector2 uvAnchor = Vector2.zero;
			Vector2 uvSize = Vector2.zero;
			Vector2 uvA = Vector2.zero;
			Vector2 uvB = Vector2.zero;
			Vector2 uvC = Vector2.zero;
			Vector2 uvD = Vector2.zero;
			Vector2 uvTriA = Vector2.zero;
			Vector2 uvTriB = Vector2.zero;
			Vector2 uvTriC = Vector2.zero;

			Utils.SpriteUv.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
			uvTriA = uvAnchor;
			uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvTriC = uvAnchor + new Vector2(0f, uvSize.y);
			Voxel.View.Face posX = Utils.Geometry.CreateTriangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvTriA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvTriB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvTriC });
			faces[OuterShellPlane.PosX] = posX;

			Utils.SpriteUv.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
			uvTriA = uvAnchor;
			uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvTriC = uvAnchor + new Vector2(0f, uvSize.y);
			Voxel.View.Face negX = Utils.Geometry.CreateTriangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvTriA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvTriB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvTriC });
			faces[OuterShellPlane.NegX] = negX;

			Utils.SpriteUv.GetSpriteUvRect(spriteBottom, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Voxel.View.Face negY = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });
			faces[OuterShellPlane.NegY] = negY;

			Utils.SpriteUv.GetSpriteUvRect(spriteBack, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Voxel.View.Face posZ = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
			faces[OuterShellPlane.PosZ] = posZ;

			return faces;
		}

	}
}
