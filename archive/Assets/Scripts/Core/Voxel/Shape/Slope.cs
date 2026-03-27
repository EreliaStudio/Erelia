using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Core.Voxel.ShapeType
{
	[Serializable]
	public class Slope : Erelia.Core.Voxel.Shape
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

		private List<Erelia.Core.Voxel.Face> ConstructInnerFaces()
		{
			var faces = new List<Erelia.Core.Voxel.Face>();

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteSlope, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Core.Voxel.Face slope = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = uvD });
			faces.Add(slope);

			return faces;
		}

		private Dictionary<AxisPlane, Erelia.Core.Voxel.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<AxisPlane, Erelia.Core.Voxel.Face>();

			Vector2 uvAnchor = Vector2.zero;
			Vector2 uvSize = Vector2.zero;

			Vector2 uvA = Vector2.zero;
			Vector2 uvB = Vector2.zero;
			Vector2 uvC = Vector2.zero;
			Vector2 uvD = Vector2.zero;

			Vector2 uvTriA = Vector2.zero;
			Vector2 uvTriB = Vector2.zero;
			Vector2 uvTriC = Vector2.zero;

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
			uvTriA = uvAnchor;
			uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvTriC = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Core.Voxel.Face posX = Erelia.Core.Voxel.Utils.Geometry.CreateTriangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvTriA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvTriB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = uvTriC });
			faces[AxisPlane.PosX] = posX;

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
			uvTriA = uvAnchor;
			uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvTriC = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Core.Voxel.Face negX = Erelia.Core.Voxel.Utils.Geometry.CreateTriangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvTriA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = uvTriB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvTriC });
			faces[AxisPlane.NegX] = negX;

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteBottom, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Core.Voxel.Face negY = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvD });
			faces[AxisPlane.NegY] = negY;

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteBack, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Core.Voxel.Face posZ = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			return faces;
		}

	}
}




