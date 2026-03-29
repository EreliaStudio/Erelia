using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class VoxelShape
{
	[SerializeField, HideInInspector] private FaceSet render = new FaceSet();
	[SerializeField, HideInInspector] private FaceSet collision = new FaceSet();
	[SerializeField, HideInInspector] private MaskSet mask = new MaskSet();

	public FaceSet Render => render;

	public FaceSet Collision => collision != null && collision.HasAnyRenderableFaces ? collision : render;

	public List<Face> GetMaskFaces(VoxelFlipOrientation flipOrientation)
	{
		return mask?.GetFaces(flipOrientation);
	}

	protected abstract FaceSet ConstructRenderFaces();

	protected virtual FaceSet ConstructCollisionFaces()
	{
		return ConstructRenderFaces();
	}

	protected abstract MaskSet ConstructMask();

	public virtual void Initialize()
	{
		render = ConstructRenderFaces() ?? new FaceSet();
		collision = ConstructCollisionFaces() ?? new FaceSet();
		mask = ConstructMask() ?? new MaskSet();
	}

	protected static Face CreateRectangle(Vector3 aPosition, Vector2 aUv, Vector3 bPosition, Vector2 bUv, Vector3 cPosition, Vector2 cUv, Vector3 dPosition, Vector2 dUv)
	{
		var face = new Face();
		var polygon = new Polygon();
		polygon.Vertices.Add(new Vertex { Position = aPosition, UV = aUv });
		polygon.Vertices.Add(new Vertex { Position = bPosition, UV = bUv });
		polygon.Vertices.Add(new Vertex { Position = cPosition, UV = cUv });
		polygon.Vertices.Add(new Vertex { Position = dPosition, UV = dUv });
		face.Polygons.Add(polygon);
		return face;
	}

	protected static Face CreateTriangle(Vector3 aPosition, Vector2 aUv, Vector3 bPosition, Vector2 bUv, Vector3 cPosition, Vector2 cUv)
	{
		var face = new Face();
		var polygon = new Polygon();
		polygon.Vertices.Add(new Vertex { Position = aPosition, UV = aUv });
		polygon.Vertices.Add(new Vertex { Position = bPosition, UV = bUv });
		polygon.Vertices.Add(new Vertex { Position = cPosition, UV = cUv });
		face.Polygons.Add(polygon);
		return face;
	}

	protected static void GetSpriteUvRect(Sprite sprite, out Vector2 uvAnchor, out Vector2 uvSize)
	{
		if (sprite == null || sprite.uv == null || sprite.uv.Length == 0)
		{
			uvAnchor = Vector2.zero;
			uvSize = Vector2.one;
			return;
		}

		Vector2 min = sprite.uv[0];
		Vector2 max = sprite.uv[0];

		for (int i = 1; i < sprite.uv.Length; i++)
		{
			Vector2 uv = sprite.uv[i];
			min = Vector2.Min(min, uv);
			max = Vector2.Max(max, uv);
		}

		uvAnchor = min;
		uvSize = max - min;
	}

	[Serializable]
	public class FaceSet
	{
		public List<Face> InnerFaces = new List<Face>();
		public OuterShellFaces OuterShell = new OuterShellFaces();

		public bool HasAnyRenderableFaces
		{
			get
			{
				if (OuterShell != null && OuterShell.HasAnyRenderableFaces)
				{
					return true;
				}

				if (InnerFaces == null)
				{
					return false;
				}

				for (int i = 0; i < InnerFaces.Count; i++)
				{
					if (InnerFaces[i] != null && InnerFaces[i].HasRenderablePolygons)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool TryGetOuterFace(VoxelAxisPlane plane, out Face face)
		{
			if (OuterShell == null)
			{
				face = null;
				return false;
			}

			return OuterShell.TryGetFace(plane, out face);
		}
	}

	[Serializable]
	public class OuterShellFaces
	{
		public Face PosX;
		public Face NegX;
		public Face PosY;
		public Face NegY;
		public Face PosZ;
		public Face NegZ;

		public bool HasAnyRenderableFaces =>
			HasRenderable(PosX) ||
			HasRenderable(NegX) ||
			HasRenderable(PosY) ||
			HasRenderable(NegY) ||
			HasRenderable(PosZ) ||
			HasRenderable(NegZ);

		public bool TryGetFace(VoxelAxisPlane plane, out Face face)
		{
			switch (plane)
			{
				case VoxelAxisPlane.PosX:
					face = PosX;
					return face != null;
				case VoxelAxisPlane.NegX:
					face = NegX;
					return face != null;
				case VoxelAxisPlane.PosY:
					face = PosY;
					return face != null;
				case VoxelAxisPlane.NegY:
					face = NegY;
					return face != null;
				case VoxelAxisPlane.PosZ:
					face = PosZ;
					return face != null;
				case VoxelAxisPlane.NegZ:
					face = NegZ;
					return face != null;
				default:
					face = null;
					return false;
			}
		}

		private static bool HasRenderable(Face face)
		{
			return face != null && face.HasRenderablePolygons;
		}
	}

	[Serializable]
	public class MaskSet
	{
		public List<Face> PositiveYFaces = new List<Face>();
		public List<Face> NegativeYFaces = new List<Face>();

		public List<Face> GetFaces(VoxelFlipOrientation flipOrientation)
		{
			if (flipOrientation == VoxelFlipOrientation.NegativeY && NegativeYFaces != null && NegativeYFaces.Count > 0)
			{
				return NegativeYFaces;
			}

			return PositiveYFaces;
		}
	}

	[Serializable]
	public class Face
	{
		public List<Polygon> Polygons = new List<Polygon>();

		public bool HasRenderablePolygons
		{
			get
			{
				if (Polygons == null)
				{
					return false;
				}

				for (int i = 0; i < Polygons.Count; i++)
				{
					Polygon polygon = Polygons[i];
					if (polygon != null && polygon.Vertices != null && polygon.Vertices.Count >= 3)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void AddPolygon(List<Vertex> vertices)
		{
			if (vertices == null || vertices.Count == 0)
			{
				return;
			}

			var polygon = new Polygon();
			polygon.Vertices.AddRange(vertices);
			Polygons.Add(polygon);
		}
	}

	[Serializable]
	public class Polygon
	{
		public List<Vertex> Vertices = new List<Vertex>();
	}

	[Serializable]
	public class Vertex
	{
		public Vector3 Position;
		public Vector2 UV;
	}
}
