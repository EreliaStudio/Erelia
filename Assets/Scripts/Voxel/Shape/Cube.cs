using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Voxel.ShapeType
{
	[Serializable]
	public class Cube : Erelia.Voxel.Shape
	{
		[Header("Textures")]
		[SerializeField] private Sprite spritePosX;
		[SerializeField] private Sprite spriteNegX;
		[SerializeField] private Sprite spritePosY;
		[SerializeField] private Sprite spriteNegY;
		[SerializeField] private Sprite spritePosZ;
		[SerializeField] private Sprite spriteNegZ;

		protected override FaceSet ConstructRenderFaces()
		{
			return new FaceSet(
				inner: new List<Erelia.Voxel.Face>(),
				outerShell: ConstructOuterShell());
		}

		private Dictionary<AxisPlane, Erelia.Voxel.Face> ConstructOuterShell()
		{
			var faces = new Dictionary<AxisPlane, Erelia.Voxel.Face>();

			Utils.SpriteUv.GetSpriteUvRect(spritePosX, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Erelia.Voxel.Face posX = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
			faces[AxisPlane.PosX] = posX;

			Utils.SpriteUv.GetSpriteUvRect(spriteNegX, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Erelia.Voxel.Face negX = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });
			faces[AxisPlane.NegX] = negX;

			Utils.SpriteUv.GetSpriteUvRect(spritePosY, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Erelia.Voxel.Face posY = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
			faces[AxisPlane.PosY] = posY;

			Utils.SpriteUv.GetSpriteUvRect(spriteNegY, out uvAnchor, out uvSize);
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

			Utils.SpriteUv.GetSpriteUvRect(spritePosZ, out uvAnchor, out uvSize);
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

			Utils.SpriteUv.GetSpriteUvRect(spriteNegZ, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);
			Erelia.Voxel.Face negZ = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
			faces[AxisPlane.NegZ] = negZ;

			return faces;
		}

	}
}

