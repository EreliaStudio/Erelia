using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class VoxelMesher
{
	private const float NormalEpsilon = 0.001f;
	private const float PointEpsilon = 0.001f;

	private static bool TryGetVoxelDefinition(VoxelCell cell, VoxelRegistry voxelRegistry, out VoxelDefinition voxelDefinition, out VoxelCell resolvedCell)
	{
		voxelDefinition = null;
		resolvedCell = cell;

		if (cell == null || cell.Id < 0 || voxelRegistry == null)
		{
			return false;
		}

		return voxelRegistry.TryGetVoxel(cell.Id, out voxelDefinition) && voxelDefinition != null;
	}

	private static bool IsFaceSetEmpty(VoxelShape.FaceSet faceSet)
	{
		return faceSet == null || !faceSet.HasAnyRenderableFaces;
	}

	private static bool IsFaceOccludedByNeighbor(
		VoxelCell[,,] cells,
		int sizeX,
		int sizeY,
		int sizeZ,
		int x,
		int y,
		int z,
		VoxelCell cell,
		VoxelShape.Face localFace,
		VoxelAxisPlane worldPlane,
		VoxelRegistry voxelRegistry,
		bool useCollision)
	{
		Vector3Int offset = PlaneToOffset(worldPlane);
		int nx = x + offset.x;
		int ny = y + offset.y;
		int nz = z + offset.z;

		if (nx < 0 || ny < 0 || nz < 0 || nx >= sizeX || ny >= sizeY || nz >= sizeZ)
		{
			return false;
		}

		if (!TryGetVoxelDefinition(cells[nx, ny, nz], voxelRegistry, out VoxelDefinition neighborVoxelDefinition, out VoxelCell neighborCell) || neighborVoxelDefinition?.Shape == null)
		{
			return false;
		}

		VoxelShape.FaceSet neighborFaceSet = useCollision ? neighborVoxelDefinition.Shape.Collision : neighborVoxelDefinition.Shape.Render;
		if (useCollision && (neighborFaceSet == null || !neighborFaceSet.HasAnyRenderableFaces))
		{
			neighborFaceSet = neighborVoxelDefinition.Shape.Render;
		}
		VoxelAxisPlane oppositePlane = GetOppositePlane(worldPlane);
		VoxelAxisPlane neighborLocalPlane = MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

		if (neighborFaceSet == null ||
			!neighborFaceSet.TryGetOuterFace(neighborLocalPlane, out VoxelShape.Face neighborLocalFace) ||
			neighborLocalFace == null ||
			!neighborLocalFace.HasRenderablePolygons)
		{
			return false;
		}

		VoxelShape.Face faceWorld = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
		VoxelShape.Face neighborWorld = TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);

		return faceWorld != null &&
			neighborWorld != null &&
			TryGetFaceOcclusion(faceWorld, neighborWorld, out bool occluded) &&
			occluded;
	}

	private static bool IsFullFaceOccludedByNeighbor(
		VoxelCell[,,] cells,
		int sizeX,
		int sizeY,
		int sizeZ,
		int x,
		int y,
		int z,
		VoxelAxisPlane worldPlane,
		VoxelRegistry voxelRegistry,
		bool useCollision)
	{
		Vector3Int offset = PlaneToOffset(worldPlane);
		int nx = x + offset.x;
		int ny = y + offset.y;
		int nz = z + offset.z;

		if (nx < 0 || ny < 0 || nz < 0 || nx >= sizeX || ny >= sizeY || nz >= sizeZ)
		{
			return false;
		}

		if (!TryGetVoxelDefinition(cells[nx, ny, nz], voxelRegistry, out VoxelDefinition neighborVoxelDefinition, out VoxelCell neighborCell) || neighborVoxelDefinition?.Shape == null)
		{
			return false;
		}

		VoxelShape.FaceSet neighborFaceSet = useCollision ? neighborVoxelDefinition.Shape.Collision : neighborVoxelDefinition.Shape.Render;
		if (useCollision && (neighborFaceSet == null || !neighborFaceSet.HasAnyRenderableFaces))
		{
			neighborFaceSet = neighborVoxelDefinition.Shape.Render;
		}
		VoxelAxisPlane oppositePlane = GetOppositePlane(worldPlane);
		VoxelAxisPlane neighborLocalPlane = MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

		if (neighborFaceSet == null ||
			!neighborFaceSet.TryGetOuterFace(neighborLocalPlane, out VoxelShape.Face neighborLocalFace) ||
			neighborLocalFace == null ||
			!neighborLocalFace.HasRenderablePolygons)
		{
			return false;
		}

		VoxelShape.Face neighborWorld = TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);
		VoxelShape.Face fullFace = FullOuterFaces[(int)oppositePlane];

		if (neighborWorld == null || fullFace == null)
		{
			return false;
		}

		if (TryGetFaceOcclusion(fullFace, neighborWorld, out bool occluded))
		{
			return occluded;
		}

		return IsFullFace(neighborWorld, oppositePlane);
	}

	private static VoxelShape.Face TransformFaceCached(VoxelShape.Face face, VoxelOrientation orientation, VoxelFlipOrientation flipOrientation)
	{
		if (face == null)
		{
			return null;
		}

		var key = new FaceTransformKey(face, orientation, flipOrientation);
		if (FaceTransformCache.TryGetValue(key, out VoxelShape.Face cached))
		{
			return cached;
		}

		VoxelShape.Face transformed = TransformFace(face, orientation, flipOrientation);
		FaceTransformCache[key] = transformed;
		return transformed;
	}

	private static bool TryGetFaceOcclusion(VoxelShape.Face face, VoxelShape.Face occluder, out bool isOccluded)
	{
		isOccluded = false;

		if (face == null || occluder == null)
		{
			return false;
		}

		var key = new FaceOcclusionKey(face, occluder);
		if (FaceOcclusionCache.TryGetValue(key, out bool cached))
		{
			isOccluded = cached;
			return true;
		}

		isOccluded = IsFaceOccludedByFace(face, occluder);
		FaceOcclusionCache[key] = isOccluded;
		return true;
	}

	private static void AddFace(
		VoxelShape.Face face,
		Vector3 positionOffset,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs,
		bool remapUv,
		Vector2 uvAnchor,
		Vector2 uvSize)
	{
		if (face == null || face.Polygons == null || face.Polygons.Count == 0)
		{
			return;
		}

		for (int polygonIndex = 0; polygonIndex < face.Polygons.Count; polygonIndex++)
		{
			VoxelShape.Polygon polygon = face.Polygons[polygonIndex];
			if (polygon?.Vertices == null || polygon.Vertices.Count < 3)
			{
				continue;
			}

			int start = vertices.Count;
			for (int vertexIndex = 0; vertexIndex < polygon.Vertices.Count; vertexIndex++)
			{
				VoxelShape.Vertex vertex = polygon.Vertices[vertexIndex];
				vertices.Add(positionOffset + vertex.Position);
				uvs.Add(remapUv ? uvAnchor + Vector2.Scale(vertex.UV, uvSize) : vertex.UV);
			}

			for (int triangleIndex = 1; triangleIndex < polygon.Vertices.Count - 1; triangleIndex++)
			{
				triangles.Add(start);
				triangles.Add(start + triangleIndex + 1);
				triangles.Add(start + triangleIndex);
			}
		}
	}

	private static void AddFaceTrianglesOnly(VoxelShape.Face face, Vector3 positionOffset, List<Vector3> vertices, List<int> triangles)
	{
		if (face == null || face.Polygons == null || face.Polygons.Count == 0)
		{
			return;
		}

		for (int polygonIndex = 0; polygonIndex < face.Polygons.Count; polygonIndex++)
		{
			VoxelShape.Polygon polygon = face.Polygons[polygonIndex];
			if (polygon?.Vertices == null || polygon.Vertices.Count < 3)
			{
				continue;
			}

			int start = vertices.Count;
			for (int vertexIndex = 0; vertexIndex < polygon.Vertices.Count; vertexIndex++)
			{
				vertices.Add(positionOffset + polygon.Vertices[vertexIndex].Position);
			}

			for (int triangleIndex = 1; triangleIndex < polygon.Vertices.Count - 1; triangleIndex++)
			{
				triangles.Add(start);
				triangles.Add(start + triangleIndex + 1);
				triangles.Add(start + triangleIndex);
			}
		}
	}

	private static VoxelShape.Face TransformFace(VoxelShape.Face face, VoxelOrientation orientation, VoxelFlipOrientation flipOrientation)
	{
		if (face == null || face.Polygons == null || face.Polygons.Count == 0)
		{
			return face;
		}

		var transformedFace = new VoxelShape.Face();

		for (int polygonIndex = 0; polygonIndex < face.Polygons.Count; polygonIndex++)
		{
			VoxelShape.Polygon sourcePolygon = face.Polygons[polygonIndex];
			if (sourcePolygon?.Vertices == null || sourcePolygon.Vertices.Count == 0)
			{
				continue;
			}

			var transformedPolygon = new VoxelShape.Polygon();
			for (int vertexIndex = 0; vertexIndex < sourcePolygon.Vertices.Count; vertexIndex++)
			{
				VoxelShape.Vertex sourceVertex = sourcePolygon.Vertices[vertexIndex];
				transformedPolygon.Vertices.Add(new VoxelShape.Vertex
				{
					Position = TransformPoint(sourceVertex.Position, orientation, flipOrientation),
					UV = sourceVertex.UV
				});
			}

			if (flipOrientation == VoxelFlipOrientation.NegativeY)
			{
				transformedPolygon.Vertices.Reverse();
			}

			transformedFace.Polygons.Add(transformedPolygon);
		}

		return transformedFace;
	}

	private static Vector3 TransformPoint(Vector3 point, VoxelOrientation orientation, VoxelFlipOrientation flipOrientation)
	{
		Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
		Vector3 local = point;
		int steps = (int)orientation;

		if (steps != 0)
		{
			Vector3 offset = local - pivot;
			Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
			local = rotation * offset + pivot;
		}

		if (flipOrientation == VoxelFlipOrientation.NegativeY)
		{
			local.y = 1f - local.y;
		}

		return local;
	}

	private static bool IsFaceOccludedByFace(VoxelShape.Face face, VoxelShape.Face other)
	{
		if (face == null || other == null || !face.HasRenderablePolygons || !other.HasRenderablePolygons)
		{
			return false;
		}

		Vector3 normal = Vector3.zero;
		for (int polygonIndex = 0; polygonIndex < face.Polygons.Count; polygonIndex++)
		{
			VoxelShape.Polygon polygon = face.Polygons[polygonIndex];
			if (polygon?.Vertices == null || polygon.Vertices.Count < 3)
			{
				continue;
			}

			normal = GetNormal(polygon.Vertices);
			if (normal.sqrMagnitude >= NormalEpsilon)
			{
				break;
			}
		}

		if (normal.sqrMagnitude < NormalEpsilon)
		{
			return false;
		}

		for (int polygonIndex = 0; polygonIndex < face.Polygons.Count; polygonIndex++)
		{
			VoxelShape.Polygon polygon = face.Polygons[polygonIndex];
			if (polygon?.Vertices == null || polygon.Vertices.Count < 3)
			{
				continue;
			}

			if (!IsPolygonContainedInUnion(polygon.Vertices, other.Polygons, normal))
			{
				return false;
			}
		}

		return true;
	}

	private static Vector3 GetNormal(List<VoxelShape.Vertex> vertices)
	{
		return Vector3.Cross(vertices[1].Position - vertices[0].Position, vertices[2].Position - vertices[0].Position);
	}

	private static bool IsPolygonContainedInUnion(List<VoxelShape.Vertex> polygon, List<VoxelShape.Polygon> containers, Vector3 normal)
	{
		if (polygon == null || polygon.Count < 3 || containers == null || !TryBuildBasis(normal, out Vector3 tangent, out Vector3 bitangent))
		{
			return false;
		}

		var polygon2D = new List<Vector2>(polygon.Count);
		for (int i = 0; i < polygon.Count; i++)
		{
			Vector3 p = polygon[i].Position;
			polygon2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
		}

		var container2Ds = new List<List<Vector2>>();
		for (int i = 0; i < containers.Count; i++)
		{
			VoxelShape.Polygon container = containers[i];
			if (container?.Vertices == null || container.Vertices.Count < 3)
			{
				continue;
			}

			var container2D = new List<Vector2>(container.Vertices.Count);
			for (int j = 0; j < container.Vertices.Count; j++)
			{
				Vector3 p = container.Vertices[j].Position;
				container2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
			}

			container2Ds.Add(container2D);
		}

		return container2Ds.Count > 0 && AreSamplePointsContained(polygon2D, container2Ds);
	}

	private static bool AreSamplePointsContained(List<Vector2> polygon, List<List<Vector2>> containers)
	{
		Vector2 centroid = Vector2.zero;
		for (int i = 0; i < polygon.Count; i++)
		{
			centroid += polygon[i];
		}

		centroid /= polygon.Count;
		if (!IsPointInUnion(centroid, containers))
		{
			return false;
		}

		for (int i = 0; i < polygon.Count; i++)
		{
			Vector2 a = polygon[i];
			Vector2 b = polygon[(i + 1) % polygon.Count];
			Vector2 midpoint = (a + b) * 0.5f;

			if (!IsPointInUnion(a, containers) || !IsPointInUnion(midpoint, containers))
			{
				return false;
			}
		}

		return true;
	}

	private static bool IsPointInUnion(Vector2 point, List<List<Vector2>> polygons)
	{
		for (int i = 0; i < polygons.Count; i++)
		{
			if (IsPointInPolygon(point, polygons[i]))
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryBuildBasis(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
	{
		if (normal.sqrMagnitude < NormalEpsilon)
		{
			tangent = Vector3.zero;
			bitangent = Vector3.zero;
			return false;
		}

		Vector3 n = normal.normalized;
		Vector3 up = Mathf.Abs(n.y) < 0.99f ? Vector3.up : Vector3.right;
		tangent = Vector3.Cross(up, n).normalized;
		bitangent = Vector3.Cross(n, tangent);
		return true;
	}

	private static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
	{
		bool inside = false;

		for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
		{
			Vector2 pi = polygon[i];
			Vector2 pj = polygon[j];

			if (IsPointOnSegment(point, pj, pi))
			{
				return true;
			}

			bool intersect = (pi.y > point.y) != (pj.y > point.y) &&
				point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y + 0.000001f) + pi.x;

			if (intersect)
			{
				inside = !inside;
			}
		}

		return inside;
	}

	private static bool IsPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
	{
		float cross = (point.y - a.y) * (b.x - a.x) - (point.x - a.x) * (b.y - a.y);
		if (Mathf.Abs(cross) > 0.0001f)
		{
			return false;
		}

		float dot = (point.x - a.x) * (b.x - a.x) + (point.y - a.y) * (b.y - a.y);
		if (dot < -0.0001f)
		{
			return false;
		}

		return dot <= (b - a).sqrMagnitude + 0.0001f;
	}

	private static bool IsFullFace(VoxelShape.Face face, VoxelAxisPlane plane)
	{
		if (face == null || !face.HasRenderablePolygons || !IsFaceCoplanarWithPlane(face, plane))
		{
			return false;
		}

		VoxelShape.Face fullFace = FullOuterFaces[(int)plane];
		return fullFace != null &&
			IsFaceOccludedByFace(fullFace, face) &&
			IsFaceOccludedByFace(face, fullFace);
	}

	private static bool IsFaceCoplanarWithPlane(VoxelShape.Face face, VoxelAxisPlane plane)
	{
		if (face == null || face.Polygons == null || face.Polygons.Count == 0)
		{
			return false;
		}

		float target = 0f;
		int axis = 0;

		switch (plane)
		{
			case VoxelAxisPlane.PosX: axis = 0; target = 1f; break;
			case VoxelAxisPlane.NegX: axis = 0; target = 0f; break;
			case VoxelAxisPlane.PosY: axis = 1; target = 1f; break;
			case VoxelAxisPlane.NegY: axis = 1; target = 0f; break;
			case VoxelAxisPlane.PosZ: axis = 2; target = 1f; break;
			case VoxelAxisPlane.NegZ: axis = 2; target = 0f; break;
		}

		for (int polygonIndex = 0; polygonIndex < face.Polygons.Count; polygonIndex++)
		{
			VoxelShape.Polygon polygon = face.Polygons[polygonIndex];
			if (polygon?.Vertices == null)
			{
				continue;
			}

			for (int vertexIndex = 0; vertexIndex < polygon.Vertices.Count; vertexIndex++)
			{
				Vector3 position = polygon.Vertices[vertexIndex].Position;
				float value = axis == 0 ? position.x : axis == 1 ? position.y : position.z;
				if (Mathf.Abs(value - target) > PointEpsilon)
				{
					return false;
				}
			}
		}

		return true;
	}

	private static VoxelAxisPlane MapWorldPlaneToLocal(VoxelAxisPlane plane, VoxelOrientation orientation, VoxelFlipOrientation flipOrientation)
	{
		VoxelAxisPlane rotated = RotatePlane(plane, -(int)orientation);
		return flipOrientation == VoxelFlipOrientation.NegativeY ? FlipPlaneY(rotated) : rotated;
	}

	private static VoxelAxisPlane RotatePlane(VoxelAxisPlane plane, int steps)
	{
		int normalized = ((steps % 4) + 4) % 4;
		if (normalized == 0)
		{
			return plane;
		}

		Vector3 normal = PlaneToNormal(plane);
		Quaternion rotation = Quaternion.AngleAxis(-normalized * 90f, Vector3.up);
		Vector3 rotatedNormal = rotation * normal;
		return TryFromNormal(rotatedNormal, out VoxelAxisPlane rotatedPlane) ? rotatedPlane : plane;
	}

	private static bool TryFromNormal(Vector3 normal, out VoxelAxisPlane plane)
	{
		if (normal.sqrMagnitude < NormalEpsilon)
		{
			plane = VoxelAxisPlane.PosX;
			return false;
		}

		Vector3 n = normal.normalized;
		float ax = Mathf.Abs(n.x);
		float ay = Mathf.Abs(n.y);
		float az = Mathf.Abs(n.z);

		if (ax >= 1f - NormalEpsilon && ay <= NormalEpsilon && az <= NormalEpsilon)
		{
			plane = n.x >= 0f ? VoxelAxisPlane.PosX : VoxelAxisPlane.NegX;
			return true;
		}

		if (ay >= 1f - NormalEpsilon && ax <= NormalEpsilon && az <= NormalEpsilon)
		{
			plane = n.y >= 0f ? VoxelAxisPlane.PosY : VoxelAxisPlane.NegY;
			return true;
		}

		if (az >= 1f - NormalEpsilon && ax <= NormalEpsilon && ay <= NormalEpsilon)
		{
			plane = n.z >= 0f ? VoxelAxisPlane.PosZ : VoxelAxisPlane.NegZ;
			return true;
		}

		plane = VoxelAxisPlane.PosX;
		return false;
	}

	private static Vector3 PlaneToNormal(VoxelAxisPlane plane)
	{
		switch (plane)
		{
			case VoxelAxisPlane.PosX: return Vector3.right;
			case VoxelAxisPlane.NegX: return Vector3.left;
			case VoxelAxisPlane.PosY: return Vector3.up;
			case VoxelAxisPlane.NegY: return Vector3.down;
			case VoxelAxisPlane.PosZ: return Vector3.forward;
			case VoxelAxisPlane.NegZ: return Vector3.back;
			default: return Vector3.zero;
		}
	}

	private static Vector3Int PlaneToOffset(VoxelAxisPlane plane)
	{
		switch (plane)
		{
			case VoxelAxisPlane.PosX: return new Vector3Int(1, 0, 0);
			case VoxelAxisPlane.NegX: return new Vector3Int(-1, 0, 0);
			case VoxelAxisPlane.PosY: return new Vector3Int(0, 1, 0);
			case VoxelAxisPlane.NegY: return new Vector3Int(0, -1, 0);
			case VoxelAxisPlane.PosZ: return new Vector3Int(0, 0, 1);
			case VoxelAxisPlane.NegZ: return new Vector3Int(0, 0, -1);
			default: return Vector3Int.zero;
		}
	}

	private static VoxelAxisPlane GetOppositePlane(VoxelAxisPlane plane)
	{
		switch (plane)
		{
			case VoxelAxisPlane.PosX: return VoxelAxisPlane.NegX;
			case VoxelAxisPlane.NegX: return VoxelAxisPlane.PosX;
			case VoxelAxisPlane.PosY: return VoxelAxisPlane.NegY;
			case VoxelAxisPlane.NegY: return VoxelAxisPlane.PosY;
			case VoxelAxisPlane.PosZ: return VoxelAxisPlane.NegZ;
			case VoxelAxisPlane.NegZ: return VoxelAxisPlane.PosZ;
			default: return plane;
		}
	}

	private static VoxelAxisPlane FlipPlaneY(VoxelAxisPlane plane)
	{
		switch (plane)
		{
			case VoxelAxisPlane.PosY: return VoxelAxisPlane.NegY;
			case VoxelAxisPlane.NegY: return VoxelAxisPlane.PosY;
			default: return plane;
		}
	}

	private static void GetSpriteUvRect(Sprite sprite, out Vector2 uvAnchor, out Vector2 uvSize)
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

	private static VoxelShape.Face CreateRectangleFace(
		Vector3 aPos,
		Vector2 aUv,
		Vector3 bPos,
		Vector2 bUv,
		Vector3 cPos,
		Vector2 cUv,
		Vector3 dPos,
		Vector2 dUv)
	{
		var face = new VoxelShape.Face();
		var polygon = new VoxelShape.Polygon();
		polygon.Vertices.Add(new VoxelShape.Vertex { Position = aPos, UV = aUv });
		polygon.Vertices.Add(new VoxelShape.Vertex { Position = bPos, UV = bUv });
		polygon.Vertices.Add(new VoxelShape.Vertex { Position = cPos, UV = cUv });
		polygon.Vertices.Add(new VoxelShape.Vertex { Position = dPos, UV = dUv });
		face.Polygons.Add(polygon);
		return face;
	}

	private readonly struct FaceTransformKey : IEquatable<FaceTransformKey>
	{
		private readonly VoxelShape.Face face;
		private readonly VoxelOrientation orientation;
		private readonly VoxelFlipOrientation flipOrientation;

		public FaceTransformKey(VoxelShape.Face face, VoxelOrientation orientation, VoxelFlipOrientation flipOrientation)
		{
			this.face = face;
			this.orientation = orientation;
			this.flipOrientation = flipOrientation;
		}

		public bool Equals(FaceTransformKey other)
		{
			return ReferenceEquals(face, other.face) && orientation == other.orientation && flipOrientation == other.flipOrientation;
		}

		public override bool Equals(object obj)
		{
			return obj is FaceTransformKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			int hash = face != null ? face.GetHashCode() : 0;
			unchecked
			{
				hash = (hash * 397) ^ (int)orientation;
				hash = (hash * 397) ^ (int)flipOrientation;
			}

			return hash;
		}
	}

	private readonly struct FaceOcclusionKey : IEquatable<FaceOcclusionKey>
	{
		private readonly VoxelShape.Face face;
		private readonly VoxelShape.Face occluder;

		public FaceOcclusionKey(VoxelShape.Face face, VoxelShape.Face occluder)
		{
			this.face = face;
			this.occluder = occluder;
		}

		public bool Equals(FaceOcclusionKey other)
		{
			return ReferenceEquals(face, other.face) && ReferenceEquals(occluder, other.occluder);
		}

		public override bool Equals(object obj)
		{
			return obj is FaceOcclusionKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			int hash = face != null ? face.GetHashCode() : 0;
			unchecked
			{
				hash = (hash * 397) ^ (occluder != null ? occluder.GetHashCode() : 0);
			}

			return hash;
		}
	}
}
