using System.Collections.Generic;
using UnityEngine;
using System;

namespace Erelia.Voxel.ShapeType
{
	[Serializable]
	public class CrossPlane : Erelia.Voxel.Shape
	{
		[Header("Textures")]
		[SerializeField] private Sprite sprite;

		protected override FaceSet ConstructRenderFaces()
		{
			return new FaceSet(
				inner: ConstructCrossPlanes(),
				outerShell: new Dictionary<AxisPlane, Erelia.Voxel.Face>());
		}

		protected override FaceSet ConstructCollisionFaces()
		{
			return new FaceSet(
				inner: new List<Erelia.Voxel.Face>(),
				outerShell: ConstructCollisionOuterShell());
		}

		private List<Erelia.Voxel.Face> ConstructCrossPlanes()
		{
			var faces = new List<Erelia.Voxel.Face>();

			Utils.SpriteUv.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector2 uvA = uvAnchor;
			Vector2 uvB = uvAnchor + new Vector2(uvSize.x, 0f);
			Vector2 uvC = uvAnchor + uvSize;
			Vector2 uvD = uvAnchor + new Vector2(0f, uvSize.y);

			Erelia.Voxel.Face planeA = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });
			faces.Add(planeA);

			Erelia.Voxel.Face planeABack = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB });
			faces.Add(planeABack);

			Erelia.Voxel.Face planeB = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });
			faces.Add(planeB);

			Erelia.Voxel.Face planeBBack = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB });
			faces.Add(planeBBack);

			return faces;
		}

		private Dictionary<AxisPlane, Erelia.Voxel.Face> ConstructCollisionOuterShell()
		{
			var faces = new Dictionary<AxisPlane, Erelia.Voxel.Face>();

			Vector2 uvA = new Vector2(0f, 0f);
			Vector2 uvB = new Vector2(1f, 0f);
			Vector2 uvC = new Vector2(1f, 1f);
			Vector2 uvD = new Vector2(0f, 1f);

			faces[AxisPlane.PosX] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvD });

			faces[AxisPlane.NegX] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvD });

			faces[AxisPlane.PosY] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvD });

			faces[AxisPlane.NegY] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvD });

			faces[AxisPlane.PosZ] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = uvD });

			faces[AxisPlane.NegZ] = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = uvA },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = uvB },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = uvC },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = uvD });

			return faces;
		}

	}
}

