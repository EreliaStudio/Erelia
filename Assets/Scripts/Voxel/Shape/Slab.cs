using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Voxel.ShapeType
{
	[Serializable]
	public class Slab : Erelia.Voxel.Shape
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

		private List<Erelia.Voxel.Face> ConstructTexturedInnerShell()
		{
			var faces = new List<Voxel.Face>();

			Utils.SpriteUv.GetSpriteUvRect(spritePosY, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Voxel.Face posY = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 1f), UV = uvD });
			faces.Add(posY);

			return faces;
		}

		private Dictionary<AxisPlane, Erelia.Voxel.Face> ConstructTexturedOuterShell()
		{
			var faces = new Dictionary<AxisPlane, Voxel.Face>();

			Utils.SpriteUv.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			Vector2 halfUvAnchor = uvAnchor;
			Vector2 uvA = halfUvAnchor;
			Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			Vector2 uvC = halfUvAnchor + halfUvSize;
			Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Voxel.Face posX = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 0f), UV = uvD });
			faces[AxisPlane.PosX] = posX;

			Utils.SpriteUv.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Voxel.Face negX = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });
			faces[AxisPlane.NegX] = negX;

			Utils.SpriteUv.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
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

			Utils.SpriteUv.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Voxel.Face posZ = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			Utils.SpriteUv.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);
			Voxel.Face negZ = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0.5f, 0f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0.5f, 0f), UV = uvD });
			faces[AxisPlane.NegZ] = negZ;

			return faces;
		}

	}
}

