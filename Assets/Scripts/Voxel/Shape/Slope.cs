using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Voxel.ShapeType
{
	[Serializable]
	public class Slope : Erelia.Voxel.Shape
	{
		[Header("Textures")]
		[SerializeField] private Sprite spriteBack;
		[SerializeField] private Sprite spriteBottom;
		[SerializeField] private Sprite spriteSlope;
		[SerializeField] private Sprite spriteSideLeft;
		[SerializeField] private Sprite spriteSideRight;

		protected override FaceSet ConstructRenderFaces()
		{
			return new FaceSet(
				inner: ConstructInnerFaces(),
				outerShell: ConstructOuterShellFaces());
		}

		private List<Erelia.Voxel.Face> ConstructInnerFaces()
		{
			var faces = new List<Erelia.Voxel.Face>();

			Utils.SpriteUv.GetSpriteUvRect(spriteSlope, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Voxel.Face slope = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
			faces.Add(slope);

			return faces;
		}

		private Dictionary<AxisPlane, Erelia.Voxel.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<AxisPlane, Erelia.Voxel.Face>();

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

			Erelia.Voxel.Face posX = Utils.Geometry.CreateTriangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvTriA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvTriB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvTriC });
			faces[AxisPlane.PosX] = posX;

			Utils.SpriteUv.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
			uvTriA = uvAnchor;
			uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvTriC = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Voxel.Face negX = Utils.Geometry.CreateTriangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvTriA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvTriB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvTriC });
			faces[AxisPlane.NegX] = negX;

			Utils.SpriteUv.GetSpriteUvRect(spriteBottom, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Voxel.Face negY = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });
			faces[AxisPlane.NegY] = negY;

			Utils.SpriteUv.GetSpriteUvRect(spriteBack, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Voxel.Face posZ = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			return faces;
		}
	}
}

