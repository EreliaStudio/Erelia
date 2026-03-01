using System.Collections.Generic;
using UnityEngine;
using System;

namespace VoxelKit.ShapeType
{
	[Serializable]
	public class Slope : VoxelKit.Shape
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

		private List<VoxelKit.Face> ConstructInnerFaces()
		{
			var faces = new List<VoxelKit.Face>();

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteSlope, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			VoxelKit.Face slope = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
			faces.Add(slope);

			return faces;
		}

		private Dictionary<AxisPlane, VoxelKit.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<AxisPlane, VoxelKit.Face>();

			Vector2 uvAnchor = Vector2.zero;
			Vector2 uvSize = Vector2.zero;

			Vector2 uvA = Vector2.zero;
			Vector2 uvB = Vector2.zero;
			Vector2 uvC = Vector2.zero;
			Vector2 uvD = Vector2.zero;

			Vector2 uvTriA = Vector2.zero;
			Vector2 uvTriB = Vector2.zero;
			Vector2 uvTriC = Vector2.zero;

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
			uvTriA = uvAnchor;
			uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvTriC = uvAnchor + new Vector2(0f, uvSize.y);

			VoxelKit.Face posX = VoxelKit.Utils.Geometry.CreateTriangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvTriA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvTriB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvTriC });
			faces[AxisPlane.PosX] = posX;

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
			uvTriA = uvAnchor;
			uvTriB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvTriC = uvAnchor + new Vector2(0f, uvSize.y);

			VoxelKit.Face negX = VoxelKit.Utils.Geometry.CreateTriangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvTriA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvTriB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvTriC });
			faces[AxisPlane.NegX] = negX;

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteBottom, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			VoxelKit.Face negY = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });
			faces[AxisPlane.NegY] = negY;

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteBack, out uvAnchor, out uvSize);
			uvA = uvAnchor;
			uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			uvC = uvAnchor + uvSize;
			uvD = uvAnchor + new Vector2(0f, uvSize.y);

			VoxelKit.Face posZ = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });
			faces[AxisPlane.PosZ] = posZ;

			return faces;
		}

	}
}



