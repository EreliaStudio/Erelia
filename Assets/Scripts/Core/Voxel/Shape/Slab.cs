using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Voxel.ShapeType
{
	[Serializable]
	public class Slab : Erelia.Core.Voxel.Shape
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
				inner: ConstructTexturedInnerShell(),
				outerShell: ConstructTexturedOuterShell());
		}

		private List<Erelia.Core.Voxel.Face> ConstructTexturedInnerShell()
		{
			var faces = new List<Erelia.Core.Voxel.Face>();

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spritePosY, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Erelia.Core.Voxel.Face posY = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0.5f, 0f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0.5f, 0f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0.5f, 1f), TileUV = uvD });
			faces.Add(posY);

			return faces;
		}

		private Dictionary<AxisPlane, Erelia.Core.Voxel.Face> ConstructTexturedOuterShell()
		{
			var faces = new Dictionary<AxisPlane, Erelia.Core.Voxel.Face>();

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			Vector2 halfUvAnchor = uvAnchor;
			Vector2 uvA = halfUvAnchor;
			Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			Vector2 uvC = halfUvAnchor + halfUvSize;
			Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.Voxel.Face posX = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0.5f, 0f), TileUV = uvD });
			faces[AxisPlane.PosX] = posX;

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.Voxel.Face negX = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0.5f, 0f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvD });
			faces[AxisPlane.NegX] = negX;

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
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

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.Voxel.Face posZ = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0.5f, 1f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.Voxel.Face negZ = Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvB },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0.5f, 0f), TileUV = uvC },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0.5f, 0f), TileUV = uvD });
			faces[AxisPlane.NegZ] = negZ;

			return faces;
		}

	}
}




