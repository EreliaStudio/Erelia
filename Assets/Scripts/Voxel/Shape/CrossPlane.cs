using System.Collections.Generic;
using UnityEngine;
using System;

namespace VoxelKit.ShapeType
{
	[Serializable]
	public class CrossPlane : VoxelKit.Shape
	{
		[Header("Textures")]
		[SerializeField] private Sprite sprite;

		protected override FaceSet ConstructRenderFaces()
		{
			return new FaceSet(
				inner: ConstructCrossPlanes(),
				outerShell: new Dictionary<AxisPlane, VoxelKit.Face>());
		}

		protected override FaceSet ConstructCollisionFaces()
		{
			return new FaceSet(
				inner: new List<VoxelKit.Face>(),
				outerShell: ConstructCollisionOuterShell());
		}

		private List<VoxelKit.Face> ConstructCrossPlanes()
		{
			var faces = new List<VoxelKit.Face>();

			VoxelKit.Utils.SpriteUv.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			VoxelKit.Face planeA = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
			faces.Add(planeA);

			VoxelKit.Face planeABack = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB });
			faces.Add(planeABack);

			VoxelKit.Face planeB = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
			faces.Add(planeB);

			VoxelKit.Face planeBBack = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB });
			faces.Add(planeBBack);

			return faces;
		}

		private Dictionary<AxisPlane, VoxelKit.Face> ConstructCollisionOuterShell()
		{
			var faces = new Dictionary<AxisPlane, VoxelKit.Face>();

			Vector2 uvA = new Vector2(0f, 0f);
			Vector2 uvB = new Vector2(1f, 0f);
			Vector2 uvC = new Vector2(1f, 1f);
			Vector2 uvD = new Vector2(0f, 1f);

			faces[AxisPlane.PosX] = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });

			faces[AxisPlane.NegX] = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });

			faces[AxisPlane.PosY] = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });

			faces[AxisPlane.NegY] = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });

			faces[AxisPlane.PosZ] = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });

			faces[AxisPlane.NegZ] = VoxelKit.Utils.Geometry.CreateRectangle(
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvC },
				new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });

			return faces;
		}

	}
}



