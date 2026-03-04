using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit.ShapeType
{
	[Serializable]
	public class Slab : Erelia.Core.VoxelKit.Shape
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

		private List<Erelia.Core.VoxelKit.Face> ConstructTexturedInnerShell()
		{
			var faces = new List<Erelia.Core.VoxelKit.Face>();

			Erelia.Core.VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spritePosY, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Erelia.Core.VoxelKit.Face posY = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0.5f, 0f), TileUV = uvA },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0.5f, 0f), TileUV = uvB },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0.5f, 1f), TileUV = uvD });
			faces.Add(posY);

			return faces;
		}

		private Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face> ConstructTexturedOuterShell()
		{
			var faces = new Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face>();

			Erelia.Core.VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			Vector2 halfUvAnchor = uvAnchor;
			Vector2 uvA = halfUvAnchor;
			Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			Vector2 uvC = halfUvAnchor + halfUvSize;
			Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.VoxelKit.Face posX = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvB },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0.5f, 0f), TileUV = uvD });
			faces[AxisPlane.PosX] = posX;

			Erelia.Core.VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.VoxelKit.Face negX = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0.5f, 0f), TileUV = uvB },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvD });
			faces[AxisPlane.NegX] = negX;

			Erelia.Core.VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Erelia.Core.VoxelKit.Face negY = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvB },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvC },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvD });
			faces[AxisPlane.NegY] = negY;

			Erelia.Core.VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.VoxelKit.Face posZ = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = uvA },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0.5f, 1f), TileUV = uvB },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0.5f, 1f), TileUV = uvC },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			Erelia.Core.VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Erelia.Core.VoxelKit.Face negZ = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = uvA },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = uvB },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0.5f, 0f), TileUV = uvC },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0.5f, 0f), TileUV = uvD });
			faces[AxisPlane.NegZ] = negZ;

			return faces;
		}

	}
}



