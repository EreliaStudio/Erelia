using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World.Chunk.Controller
{
	[Serializable]
	public abstract class CollisionMesher : World.Chunk.Model.Mesher
	{
		[NonSerialized] private int[] visited;
		[NonSerialized] private int visitedStamp = 1;
		[NonSerialized] private readonly Queue<Vector3Int> floodQueue = new Queue<Vector3Int>();
		[NonSerialized] private readonly List<Vector3Int> islandCells = new List<Vector3Int>();
		[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
		[NonSerialized] private readonly List<int> triangles = new List<int>();
		[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();
		[NonSerialized] private readonly Dictionary<PlaneKey, PlaneGroup> planeGroups = new Dictionary<PlaneKey, PlaneGroup>();
		[NonSerialized] private readonly Dictionary<PointKey, Vector3> pointKeyTo3D = new Dictionary<PointKey, Vector3>();
		[NonSerialized] private readonly Dictionary<PointKey, Vector2> pointKeyTo2D = new Dictionary<PointKey, Vector2>();
		[NonSerialized] private readonly Dictionary<EdgeKey, Edge> edges = new Dictionary<EdgeKey, Edge>();
		[NonSerialized] private readonly Dictionary<PointKey, List<int>> adjacency = new Dictionary<PointKey, List<int>>();
		[NonSerialized] private readonly List<Edge> edgeList = new List<Edge>();
		[NonSerialized] private readonly List<PointKey> loopBuffer = new List<PointKey>();
		[NonSerialized] private readonly List<int> polygonIndices = new List<int>();

		protected abstract bool IsAcceptableDefinition(Voxel.Model.Definition definition);

		protected virtual string MeshName => "CollisionMesh";

		public List<Mesh> BuildMeshes(World.Chunk.Cell[,,] cells)
		{
			var meshes = new List<Mesh>();
			if (cells == null)
			{
				return meshes;
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				return meshes;
			}

			BeginVisitedPass(sizeX, sizeY, sizeZ);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						int index = GetIndex(x, y, z, sizeY, sizeZ);
						if (visited[index] == visitedStamp || !IsAcceptableAt(cells, sizeX, sizeY, sizeZ, x, y, z))
						{
							continue;
						}

						FloodFillAcceptable(cells, sizeX, sizeY, sizeZ, x, y, z);
						Mesh islandMesh = BuildIslandMesh(cells, sizeX, sizeY, sizeZ);
						if (islandMesh != null && islandMesh.vertexCount > 0)
						{
							islandMesh.name = MeshName;
							meshes.Add(islandMesh);
						}
					}
				}
			}

			return meshes;
		}

		private void FloodFillAcceptable(World.Chunk.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int startX, int startY, int startZ)
		{
			islandCells.Clear();
			floodQueue.Clear();
			floodQueue.Enqueue(new Vector3Int(startX, startY, startZ));
			visited[GetIndex(startX, startY, startZ, sizeY, sizeZ)] = visitedStamp;

			while (floodQueue.Count > 0)
			{
				Vector3Int current = floodQueue.Dequeue();
				islandCells.Add(current);

				TryEnqueueAcceptable(cells, sizeX, sizeY, sizeZ, current.x + 1, current.y, current.z);
				TryEnqueueAcceptable(cells, sizeX, sizeY, sizeZ, current.x - 1, current.y, current.z);
				TryEnqueueAcceptable(cells, sizeX, sizeY, sizeZ, current.x, current.y + 1, current.z);
				TryEnqueueAcceptable(cells, sizeX, sizeY, sizeZ, current.x, current.y - 1, current.z);
				TryEnqueueAcceptable(cells, sizeX, sizeY, sizeZ, current.x, current.y, current.z + 1);
				TryEnqueueAcceptable(cells, sizeX, sizeY, sizeZ, current.x, current.y, current.z - 1);
			}
		}

		private void TryEnqueueAcceptable(World.Chunk.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int x, int y, int z)
		{
			if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ)
			{
				return;
			}

			int index = GetIndex(x, y, z, sizeY, sizeZ);
			if (visited[index] == visitedStamp || !IsAcceptableAt(cells, sizeX, sizeY, sizeZ, x, y, z))
			{
				return;
			}

			visited[index] = visitedStamp;
			floodQueue.Enqueue(new Vector3Int(x, y, z));
		}

		private Mesh BuildIslandMesh(World.Chunk.Cell[,,] cells, int sizeX, int sizeY, int sizeZ)
		{
			var mesh = new Mesh();
			vertices.Clear();
			triangles.Clear();
			uvs.Clear();
			planeGroups.Clear();

			for (int i = 0; i < islandCells.Count; i++)
			{
				Vector3Int cell = islandCells[i];
				AddVoxelCollision(cells, sizeX, sizeY, sizeZ, cell.x, cell.y, cell.z);
			}

			foreach (PlaneGroup group in planeGroups.Values)
			{
				AppendMergedPlane(group);
			}

			if (vertices.Count == 0)
			{
				return mesh;
			}

			mesh.SetVertices(vertices);
			FlipWindingInPlace(triangles);
			mesh.SetTriangles(triangles, 0);
			mesh.SetUVs(0, uvs);
			return mesh;
		}

		private static void FlipWindingInPlace(List<int> indices)
		{
			if (indices == null)
			{
				return;
			}

			for (int i = 0; i + 2 < indices.Count; i += 3)
			{
				int temp = indices[i + 1];
				indices[i + 1] = indices[i + 2];
				indices[i + 2] = temp;
			}
		}

		private void AddVoxelCollision(World.Chunk.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int x, int y, int z)
		{
			if (!TryGetCell(cells, x, y, z, out World.Chunk.Cell cell))
			{
				return;
			}

			if (!TryGetDefinition(cell, out Voxel.Model.Definition definition))
			{
				return;
			}

			if (!IsAcceptableDefinition(definition))
			{
				return;
			}

			Voxel.View.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Orientation orientation = cell.Orientation;
			FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			foreach (Voxel.View.Shape.AxisPlane plane in Voxel.View.Shape.AxisPlanes)
			{
				TryAddOuterFaceCollision(cells, sizeX, sizeY, sizeZ, shape, orientation, flipOrientation, position, x, y, z, plane, ref anyOuterVisible);
			}

			if (anyOuterVisible)
			{
				IReadOnlyList<Voxel.View.Face> innerFaces = shape.InnerFaces;
				for (int i = 0; i < innerFaces.Count; i++)
				{
					CollectFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position);
				}
			}
		}

		private void TryAddOuterFaceCollision(
			World.Chunk.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			Voxel.View.Shape shape,
			Orientation orientation,
			FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Voxel.View.Shape.AxisPlane plane,
			ref bool anyOuterVisible)
		{
			Vector3Int offset = Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			bool hasNeighbor = TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Cell neighborCell);
			Voxel.Model.Definition neighborDefinition = null;
			if (hasNeighbor && !TryGetDefinition(neighborCell, out neighborDefinition))
			{
				hasNeighbor = false;
			}
			if (hasNeighbor && !IsAcceptableDefinition(neighborDefinition))
			{
				hasNeighbor = false;
			}

			Voxel.View.Shape neighborShape = hasNeighbor ? neighborDefinition.Shape : null;

			Voxel.View.Shape.AxisPlane localPlane = Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			if (!shape.OuterShellFaces.TryGetValue(localPlane, out Voxel.View.Face face))
			{
				if (!IsFullyOccludedByNeighbor(cells, neighborShape, neighborX, neighborY, neighborZ, plane, hasNeighbor))
				{
					anyOuterVisible = true;
				}
				return;
			}

			if (face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullyOccludedByNeighbor(cells, neighborShape, neighborX, neighborY, neighborZ, plane, hasNeighbor))
				{
					anyOuterVisible = true;
				}
				return;
			}

			bool isOccluded = false;
			Voxel.View.Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (hasNeighbor && neighborShape != null)
			{
				Voxel.View.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
				Orientation neighborOrientation = neighborCell != null ? neighborCell.Orientation : Orientation.PositiveX;
				FlipOrientation neighborFlipOrientation = neighborCell != null ? neighborCell.FlipOrientation : FlipOrientation.PositiveY;
				Voxel.View.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
				if (neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.View.Face otherFace))
				{
					Voxel.View.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
					if (rotatedFace.IsOccludedBy(rotatedOtherFace))
					{
						isOccluded = true;
					}
				}
			}

			if (isOccluded)
			{
				return;
			}

			CollectFace(rotatedFace, position);
			anyOuterVisible = true;
		}

		private void BeginVisitedPass(int sizeX, int sizeY, int sizeZ)
		{
			int size = sizeX * sizeY * sizeZ;
			if (visited == null || visited.Length != size)
			{
				visited = new int[size];
				visitedStamp = 1;
				return;
			}

			visitedStamp++;
			if (visitedStamp == int.MaxValue)
			{
				Array.Clear(visited, 0, visited.Length);
				visitedStamp = 1;
			}
		}

		private int GetIndex(int x, int y, int z, int sizeY, int sizeZ)
		{
			return (x * sizeY + y) * sizeZ + z;
		}

		private bool IsAcceptableAt(World.Chunk.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int x, int y, int z)
		{
			if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ)
			{
				return false;
			}

			if (!TryGetCell(cells, x, y, z, out World.Chunk.Cell cell))
			{
				return false;
			}

			int id = cell.Id;
			if (ServiceLocator.Instance.VoxelService.AirID == id)
			{
				return false;
			}

			if (!TryGetDefinition(cell, out Voxel.Model.Definition definition))
			{
				return false;
			}

			return IsAcceptableDefinition(definition);
		}

		private bool IsFullyOccludedByNeighbor(
			World.Chunk.Cell[,,] cells,
			Voxel.View.Shape neighborShape,
			int neighborX,
			int neighborY,
			int neighborZ,
			Voxel.View.Shape.AxisPlane plane,
			bool hasNeighbor)
		{
			if (!hasNeighbor || neighborShape == null)
			{
				return false;
			}

			if (!TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Cell neighborCell) || neighborCell == null)
			{
				return false;
			}

			Voxel.View.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
			Orientation neighborOrientation = neighborCell.Orientation;
			FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Voxel.View.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (!neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.View.Face otherFace))
			{
				return false;
			}

			Voxel.View.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			if (!Geometry.IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
			{
				return false;
			}

			Voxel.View.Face fullFace = Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
		}

		private void CollectFace(Voxel.View.Face face, Vector3 offset)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Voxel.View.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Voxel.View.Face.Vertex> polygon = facePolygons[p];
				if (polygon == null || polygon.Count < 3)
				{
					continue;
				}

				var points = new List<Vector3>(polygon.Count);
				for (int i = 0; i < polygon.Count; i++)
				{
					points.Add(offset + polygon[i].Position);
				}

				AddPolygon(points);
			}
		}

		private void AddPolygon(List<Vector3> polygon)
		{
			if (polygon == null || polygon.Count < 3)
			{
				return;
			}

			Vector3 a = polygon[0];
			Vector3 b = polygon[1];
			Vector3 c = polygon[2];
			Vector3 normal = Vector3.Cross(b - a, c - a);
			if (normal.sqrMagnitude < 0.000001f)
			{
				return;
			}

			normal.Normalize();
			float distance = Vector3.Dot(normal, a);
			PlaneKey key = PlaneKey.From(normal, distance);
			if (!planeGroups.TryGetValue(key, out PlaneGroup group))
			{
				group = new PlaneGroup(normal, distance);
				planeGroups.Add(key, group);
			}

			group.Polygons.Add(polygon);
		}

		private void AppendMergedPlane(PlaneGroup group)
		{
			if (!TryBuildBasis(group.Normal, out Vector3 tangent, out Vector3 bitangent))
			{
				return;
			}

			pointKeyTo3D.Clear();
			pointKeyTo2D.Clear();
			edges.Clear();
			edgeList.Clear();
			adjacency.Clear();

			for (int p = 0; p < group.Polygons.Count; p++)
			{
				List<Vector3> polygon = group.Polygons[p];
				if (polygon == null || polygon.Count < 3)
				{
					continue;
				}

				for (int i = 0; i < polygon.Count; i++)
				{
					Vector3 position = polygon[i];
					Vector2 projected = new Vector2(Vector3.Dot(position, tangent), Vector3.Dot(position, bitangent));
					PointKey key = PointKey.From(projected);
					if (!pointKeyTo2D.ContainsKey(key))
					{
						pointKeyTo2D[key] = projected;
						pointKeyTo3D[key] = position;
					}

					Vector3 nextPosition = polygon[(i + 1) % polygon.Count];
					Vector2 nextProjected = new Vector2(Vector3.Dot(nextPosition, tangent), Vector3.Dot(nextPosition, bitangent));
					PointKey nextKey = PointKey.From(nextProjected);
					if (!pointKeyTo2D.ContainsKey(nextKey))
					{
						pointKeyTo2D[nextKey] = nextProjected;
						pointKeyTo3D[nextKey] = nextPosition;
					}

					EdgeKey edgeKey = new EdgeKey(key, nextKey);
					EdgeKey reverseKey = new EdgeKey(nextKey, key);
					if (edges.ContainsKey(reverseKey))
					{
						edges.Remove(reverseKey);
					}
					else
					{
						edges[edgeKey] = new Edge(key, nextKey);
					}
				}
			}

			if (edges.Count == 0)
			{
				return;
			}

			foreach (Edge edge in edges.Values)
			{
				int index = edgeList.Count;
				edgeList.Add(edge);
				if (!adjacency.TryGetValue(edge.A, out List<int> listA))
				{
					listA = new List<int>(2);
					adjacency.Add(edge.A, listA);
				}
				listA.Add(index);
				if (!adjacency.TryGetValue(edge.B, out List<int> listB))
				{
					listB = new List<int>(2);
					adjacency.Add(edge.B, listB);
				}
				listB.Add(index);
			}

			var used = new bool[edgeList.Count];
			for (int i = 0; i < edgeList.Count; i++)
			{
				if (used[i])
				{
					continue;
				}

				BuildLoop(i, used);
			}
		}

		private void BuildLoop(int startEdgeIndex, bool[] used)
		{
			loopBuffer.Clear();

			Edge startEdge = edgeList[startEdgeIndex];
			PointKey start = startEdge.A;
			PointKey current = startEdge.B;
			loopBuffer.Add(start);
			loopBuffer.Add(current);
			used[startEdgeIndex] = true;

			int guard = 0;
			while (!current.Equals(start) && guard++ < 10000)
			{
				if (!adjacency.TryGetValue(current, out List<int> connected))
				{
					break;
				}

				int nextEdgeIndex = -1;
				PointKey nextPoint = default;
				for (int i = 0; i < connected.Count; i++)
				{
					int edgeIndex = connected[i];
					if (used[edgeIndex])
					{
						continue;
					}

					Edge edge = edgeList[edgeIndex];
					nextPoint = edge.A.Equals(current) ? edge.B : edge.A;
					nextEdgeIndex = edgeIndex;
					break;
				}

				if (nextEdgeIndex == -1)
				{
					break;
				}

				used[nextEdgeIndex] = true;
				current = nextPoint;
				if (!current.Equals(start))
				{
					loopBuffer.Add(current);
				}
			}

			if (loopBuffer.Count >= 3)
			{
				AppendTriangulatedLoop(loopBuffer);
			}
		}

		private void AppendTriangulatedLoop(List<PointKey> loop)
		{
			int count = loop.Count;
			if (count < 3)
			{
				return;
			}

			var poly2D = new List<Vector2>(count);
			var poly3D = new List<Vector3>(count);
			for (int i = 0; i < count; i++)
			{
				PointKey key = loop[i];
				if (!pointKeyTo2D.TryGetValue(key, out Vector2 p2D) || !pointKeyTo3D.TryGetValue(key, out Vector3 p3D))
				{
					return;
				}
				poly2D.Add(p2D);
				poly3D.Add(p3D);
			}

			RemoveCollinear(poly2D, poly3D);
			if (poly2D.Count < 3)
			{
				return;
			}

			if (SignedArea(poly2D) < 0f)
			{
				poly2D.Reverse();
				poly3D.Reverse();
			}

			int startIndex = vertices.Count;
			for (int i = 0; i < poly3D.Count; i++)
			{
				vertices.Add(poly3D[i]);
				uvs.Add(Vector2.zero);
			}

			TriangulatePolygon(poly2D, startIndex);
		}

		private void TriangulatePolygon(List<Vector2> polygon, int startIndex)
		{
			polygonIndices.Clear();
			for (int i = 0; i < polygon.Count; i++)
			{
				polygonIndices.Add(i);
			}

			int guard = 0;
			while (polygonIndices.Count > 2 && guard++ < 5000)
			{
				bool earFound = false;
				int count = polygonIndices.Count;
				for (int i = 0; i < count; i++)
				{
					int prevIndex = polygonIndices[(i - 1 + count) % count];
					int currIndex = polygonIndices[i];
					int nextIndex = polygonIndices[(i + 1) % count];

					Vector2 a = polygon[prevIndex];
					Vector2 b = polygon[currIndex];
					Vector2 c = polygon[nextIndex];
					if (!IsConvex(a, b, c))
					{
						continue;
					}

					if (ContainsPointInTriangle(polygon, polygonIndices, prevIndex, currIndex, nextIndex))
					{
						continue;
					}

					triangles.Add(startIndex + prevIndex);
					triangles.Add(startIndex + currIndex);
					triangles.Add(startIndex + nextIndex);
					polygonIndices.RemoveAt(i);
					earFound = true;
					break;
				}

				if (!earFound)
				{
					FallbackFanTriangulation(polygon, startIndex);
					return;
				}
			}
		}

		private void FallbackFanTriangulation(List<Vector2> polygon, int startIndex)
		{
			for (int i = 1; i < polygon.Count - 1; i++)
			{
				triangles.Add(startIndex);
				triangles.Add(startIndex + i);
				triangles.Add(startIndex + i + 1);
			}
		}

		private static bool ContainsPointInTriangle(List<Vector2> polygon, List<int> indices, int aIndex, int bIndex, int cIndex)
		{
			Vector2 a = polygon[aIndex];
			Vector2 b = polygon[bIndex];
			Vector2 c = polygon[cIndex];
			for (int i = 0; i < indices.Count; i++)
			{
				int idx = indices[i];
				if (idx == aIndex || idx == bIndex || idx == cIndex)
				{
					continue;
				}

				if (IsPointInTriangle(polygon[idx], a, b, c))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			float area = Cross(b - a, c - a);
			float area1 = Cross(b - a, p - a);
			float area2 = Cross(c - b, p - b);
			float area3 = Cross(a - c, p - c);
			bool hasNeg = (area1 < 0f) || (area2 < 0f) || (area3 < 0f);
			bool hasPos = (area1 > 0f) || (area2 > 0f) || (area3 > 0f);
			if (area < 0f)
			{
				hasNeg = (area1 > 0f) || (area2 > 0f) || (area3 > 0f);
				hasPos = (area1 < 0f) || (area2 < 0f) || (area3 < 0f);
			}
			return !(hasNeg && hasPos);
		}

		private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
		{
			return Cross(b - a, c - b) > 0f;
		}

		private static float Cross(Vector2 a, Vector2 b)
		{
			return a.x * b.y - a.y * b.x;
		}

		private static float SignedArea(List<Vector2> polygon)
		{
			float area = 0f;
			for (int i = 0; i < polygon.Count; i++)
			{
				Vector2 a = polygon[i];
				Vector2 b = polygon[(i + 1) % polygon.Count];
				area += a.x * b.y - b.x * a.y;
			}
			return area * 0.5f;
		}

		private static void RemoveCollinear(List<Vector2> poly2D, List<Vector3> poly3D)
		{
			int i = 0;
			while (i < poly2D.Count && poly2D.Count >= 3)
			{
				int prev = (i - 1 + poly2D.Count) % poly2D.Count;
				int next = (i + 1) % poly2D.Count;
				Vector2 a = poly2D[prev];
				Vector2 b = poly2D[i];
				Vector2 c = poly2D[next];
				if (Mathf.Abs(Cross(b - a, c - b)) < 0.0001f)
				{
					poly2D.RemoveAt(i);
					poly3D.RemoveAt(i);
					if (i > 0)
					{
						i--;
					}
					continue;
				}
				i++;
			}
		}

		private static bool TryBuildBasis(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
		{
			if (normal.sqrMagnitude < 0.000001f)
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

		private readonly struct PlaneKey : IEquatable<PlaneKey>
		{
			private const float Scale = 10000f;
			private readonly int nx;
			private readonly int ny;
			private readonly int nz;
			private readonly int d;

			private PlaneKey(int nx, int ny, int nz, int d)
			{
				this.nx = nx;
				this.ny = ny;
				this.nz = nz;
				this.d = d;
			}

			public static PlaneKey From(Vector3 normal, float distance)
			{
				int nx = Mathf.RoundToInt(normal.x * Scale);
				int ny = Mathf.RoundToInt(normal.y * Scale);
				int nz = Mathf.RoundToInt(normal.z * Scale);
				int d = Mathf.RoundToInt(distance * Scale);
				return new PlaneKey(nx, ny, nz, d);
			}

			public bool Equals(PlaneKey other)
			{
				return nx == other.nx && ny == other.ny && nz == other.nz && d == other.d;
			}

			public override bool Equals(object obj)
			{
				return obj is PlaneKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = nx;
				unchecked
				{
					hash = (hash * 397) ^ ny;
					hash = (hash * 397) ^ nz;
					hash = (hash * 397) ^ d;
				}
				return hash;
			}
		}

		private sealed class PlaneGroup
		{
			public readonly Vector3 Normal;
			public readonly float Distance;
			public readonly List<List<Vector3>> Polygons = new List<List<Vector3>>();

			public PlaneGroup(Vector3 normal, float distance)
			{
				Normal = normal;
				Distance = distance;
			}
		}

		private readonly struct PointKey : IEquatable<PointKey>
		{
			private const float Scale = 10000f;
			private readonly int x;
			private readonly int y;

			private PointKey(int x, int y)
			{
				this.x = x;
				this.y = y;
			}

			public static PointKey From(Vector2 value)
			{
				int x = Mathf.RoundToInt(value.x * Scale);
				int y = Mathf.RoundToInt(value.y * Scale);
				return new PointKey(x, y);
			}

			public bool Equals(PointKey other)
			{
				return x == other.x && y == other.y;
			}

			public override bool Equals(object obj)
			{
				return obj is PointKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = x;
				unchecked
				{
					hash = (hash * 397) ^ y;
				}
				return hash;
			}
		}

		private readonly struct EdgeKey : IEquatable<EdgeKey>
		{
			private readonly PointKey a;
			private readonly PointKey b;

			public EdgeKey(PointKey a, PointKey b)
			{
				this.a = a;
				this.b = b;
			}

			public bool Equals(EdgeKey other)
			{
				return a.Equals(other.a) && b.Equals(other.b);
			}

			public override bool Equals(object obj)
			{
				return obj is EdgeKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = a.GetHashCode();
				unchecked
				{
					hash = (hash * 397) ^ b.GetHashCode();
				}
				return hash;
			}
		}

		private readonly struct Edge
		{
			public readonly PointKey A;
			public readonly PointKey B;

			public Edge(PointKey a, PointKey b)
			{
				A = a;
				B = b;
			}
		}
	
		public static List<Mesh> Build(World.Chunk.Cell[,,] cells)
		{
			return new CollisionMesher().BuildMeshes(cells);
		}
	}
}
