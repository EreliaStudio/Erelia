using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Core.VoxelKit.ShapeType
{
	[Serializable]
	public class CrossPlane : Erelia.Core.VoxelKit.Shape
	{
		[Header("Textures")]
		[SerializeField] private Sprite sprite;

		protected override FaceSet ConstructRenderFaces()
		{
			return new FaceSet(
				inner: ConstructCrossPlanes(),
				outerShell: new Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face>());
		}

		protected override FaceSet ConstructCollisionFaces()
		{
			return new FaceSet(
				inner: new List<Erelia.Core.VoxelKit.Face>(),
				outerShell: ConstructCollisionOuterShell());
		}

		private List<Erelia.Core.VoxelKit.Face> ConstructCrossPlanes()
		{
			var faces = new List<Erelia.Core.VoxelKit.Face>();

			Erelia.Core.VoxelKit.Utils.SpriteUv.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Core.VoxelKit.Face planeA = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
			faces.Add(planeA);

			Erelia.Core.VoxelKit.Face planeABack = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB });
			faces.Add(planeABack);

			Erelia.Core.VoxelKit.Face planeB = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
			faces.Add(planeB);

			Erelia.Core.VoxelKit.Face planeBBack = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB });
			faces.Add(planeBBack);

			return faces;
		}

		private Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face> ConstructCollisionOuterShell()
		{
			var faces = new Dictionary<AxisPlane, Erelia.Core.VoxelKit.Face>();

			Vector2 uvA = new Vector2(0f, 0f);
			Vector2 uvB = new Vector2(1f, 0f);
			Vector2 uvC = new Vector2(1f, 1f);
			Vector2 uvD = new Vector2(0f, 1f);

			faces[AxisPlane.PosX] = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });

			faces[AxisPlane.NegX] = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });

			faces[AxisPlane.PosY] = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });

			faces[AxisPlane.NegY] = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });

			faces[AxisPlane.PosZ] = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });

			faces[AxisPlane.NegZ] = Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvC },
				new Erelia.Core.VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });

			return faces;
		}

	}
}



