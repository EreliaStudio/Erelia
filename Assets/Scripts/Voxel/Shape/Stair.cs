using System.Collections.Generic;
using UnityEngine;
using System;

namespace VoxelKit.ShapeType
{
	[Serializable]
	public class Stair : VoxelKit.Shape
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

		private List<VoxelKit.Face> ConstructInnerFaces()
		{
			var faces = new List<VoxelKit.Face>();

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteStepTop, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			Vector2 halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);

			Vector2 uvA = halfUvAnchor;
			Vector2 uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			Vector2 uvC = halfUvAnchor + halfUvSize;
			Vector2 uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			VoxelKit.Face stepTop = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvD });
			faces.Add(stepTop);

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteStepRiser, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;

			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			VoxelKit.Face stepRiser = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvD });
			faces.Add(stepRiser);

			return faces;
		}

		private Dictionary<AxisPlane, VoxelKit.Face> ConstructOuterShellFaces()
		{
			var faces = new Dictionary<AxisPlane, VoxelKit.Face>();

			Vector2 uvAnchor = Vector2.zero;
			Vector2 uvSize = Vector2.zero;
			Vector2 halfUvSize = Vector2.zero;
			Vector2 halfUvAnchor = Vector2.zero;
			Vector2 uvA = Vector2.zero;
			Vector2 uvB = Vector2.zero;
			Vector2 uvC = Vector2.zero;
			Vector2 uvD = Vector2.zero;

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteSideRight, out uvAnchor, out uvSize);
			float u0 = uvAnchor.x;
			float u1 = uvAnchor.x + uvSize.x * 0.5f;
			float u2 = uvAnchor.x + uvSize.x;
			float v0 = uvAnchor.y;
			float v1 = uvAnchor.y + uvSize.y * 0.5f;
			float v2 = uvAnchor.y + uvSize.y;

			var posX = new VoxelKit.Face();
			posX.AddPolygon(new List<VoxelKit.Face.Vertex>
			{
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = new Vector2(u2, v0) },
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, StepHeight, 0f), TileUV = new Vector2(u2, v1) }
			});
			posX.AddPolygon(new List<VoxelKit.Face.Vertex>
			{
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = new Vector2(u0, v0) },
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = new Vector2(u0, v2) },
				new VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, StepDepth), TileUV = new Vector2(u1, v2) }
			});
			faces[AxisPlane.PosX] = posX;

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteSideLeft, out uvAnchor, out uvSize);
			u0 = uvAnchor.x;
			u1 = uvAnchor.x + uvSize.x * 0.5f;
			u2 = uvAnchor.x + uvSize.x;
			v0 = uvAnchor.y;
			v1 = uvAnchor.y + uvSize.y * 0.5f;
			v2 = uvAnchor.y + uvSize.y;

			var negX = new VoxelKit.Face();
			negX.AddPolygon(new List<VoxelKit.Face.Vertex>
			{
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = new Vector2(u2, v0) },
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, StepHeight, 0f), TileUV = new Vector2(u2, v1) },
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, StepHeight, StepDepth), TileUV = new Vector2(u1, v1) },
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) }
			});
			negX.AddPolygon(new List<VoxelKit.Face.Vertex>
			{
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, StepDepth), TileUV = new Vector2(u1, v0) },
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, StepDepth), TileUV = new Vector2(u1, v2) },
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = new Vector2(u0, v2) },
				new VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = new Vector2(u0, v0) }
			});
			faces[AxisPlane.NegX] = negX;

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteFront, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor;
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			VoxelKit.Face negZ = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, StepHeight, 0f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, StepHeight, 0f), UV = uvD });
			faces[AxisPlane.NegZ] = negZ;

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

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(spriteStepTop, out uvAnchor, out uvSize);
			halfUvSize = new Vector2(uvSize.x, uvSize.y * 0.5f);
			halfUvAnchor = uvAnchor + new Vector2(0f, uvSize.y * 0.5f);
			uvA = halfUvAnchor;
			uvB = halfUvAnchor + new Vector2(halfUvSize.x, 0f);
			uvC = halfUvAnchor + halfUvSize;
			uvD = halfUvAnchor + new Vector2(0f, halfUvSize.y);

			VoxelKit.Face posY = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, StepDepth), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, StepDepth), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });
			faces[AxisPlane.PosY] = posY;

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

			return faces;
		}
	}
}



