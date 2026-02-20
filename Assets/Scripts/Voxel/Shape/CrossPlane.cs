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

		protected override Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>> ConstructMaskFaces()
		{
			const float maskOffset = 0.01f;
			var faces = new List<Erelia.Voxel.Face>();
			Erelia.Voxel.Face top = Utils.Geometry.CreateRectangle(
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 0f), UV = new Vector2(0f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 0f), UV = new Vector2(1f, 0f) },
				new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f + maskOffset, 1f), UV = new Vector2(1f, 1f) },
				new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f + maskOffset, 1f), UV = new Vector2(0f, 1f) });
			faces.Add(top);

			return new Dictionary<Erelia.Voxel.FlipOrientation, List<Erelia.Voxel.Face>>
			{
				[Erelia.Voxel.FlipOrientation.PositiveY] = faces,
				[Erelia.Voxel.FlipOrientation.NegativeY] = faces
			};
		}

		protected override Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>> ConstructCardinalPoints()
		{
			var points = new Dictionary<Erelia.Voxel.CardinalPoint, Vector3>
			{
				[Erelia.Voxel.CardinalPoint.PositiveX] = new Vector3(1f, 0f, 0.5f),
				[Erelia.Voxel.CardinalPoint.NegativeX] = new Vector3(0f, 0f, 0.5f),
				[Erelia.Voxel.CardinalPoint.PositiveZ] = new Vector3(0.5f, 0f, 1f),
				[Erelia.Voxel.CardinalPoint.NegativeZ] = new Vector3(0.5f, 0f, 0f),
				[Erelia.Voxel.CardinalPoint.Stationary] = new Vector3(0.5f, 0f, 0.5f),
			};

			return new Dictionary<Erelia.Voxel.FlipOrientation, Dictionary<Erelia.Voxel.CardinalPoint, Vector3>>
			{
				[Erelia.Voxel.FlipOrientation.PositiveY] = points,
				[Erelia.Voxel.FlipOrientation.NegativeY] = points
			};
		}
	}
}

