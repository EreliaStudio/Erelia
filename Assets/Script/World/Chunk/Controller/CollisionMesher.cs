using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World.Chunk.Controller
{
	[Serializable]
	public abstract class CollisionMesher : World.Chunk.Core.Mesher
	{
		[NonSerialized] private int[] visited;
		[NonSerialized] private int visitedStamp = 1;
		[NonSerialized] private readonly Queue<Vector3Int> floodQueue = new Queue<Vector3Int>();
		[NonSerialized] private readonly List<Vector3Int> islandCells = new List<Vector3Int>();
		[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
		[NonSerialized] private readonly List<int> triangles = new List<int>();
		[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();
		[NonSerialized] private readonly Dictionary<Utils.PolygonMerge.PlaneKey, Utils.PolygonMerge.PlaneGroup> planeGroups = new Dictionary<Utils.PolygonMerge.PlaneKey, Utils.PolygonMerge.PlaneGroup>();
		[NonSerialized] private readonly Dictionary<Utils.PolygonMerge.PointKey, Vector3> pointKeyTo3D = new Dictionary<Utils.PolygonMerge.PointKey, Vector3>();
		[NonSerialized] private readonly Dictionary<Utils.PolygonMerge.PointKey, Vector2> pointKeyTo2D = new Dictionary<Utils.PolygonMerge.PointKey, Vector2>();
		[NonSerialized] private readonly Dictionary<Utils.PolygonMerge.EdgeKey, Utils.PolygonMerge.Edge> edges = new Dictionary<Utils.PolygonMerge.EdgeKey, Utils.PolygonMerge.Edge>();
		[NonSerialized] private readonly Dictionary<Utils.PolygonMerge.PointKey, List<int>> adjacency = new Dictionary<Utils.PolygonMerge.PointKey, List<int>>();
		[NonSerialized] private readonly List<Utils.PolygonMerge.Edge> edgeList = new List<Utils.PolygonMerge.Edge>();
		[NonSerialized] private readonly List<Utils.PolygonMerge.PointKey> loopBuffer = new List<Utils.PolygonMerge.PointKey>();
		[NonSerialized] private readonly List<int> polygonIndices = new List<int>();

		protected abstract bool IsAcceptableDefinition(Voxel.Model.Definition definition);

		protected virtual string MeshName => "CollisionMesh";

		public List<Mesh> BuildMeshes(World.Chunk.Model.Cell[,,] cells)
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

		private void FloodFillAcceptable(World.Chunk.Model.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int startX, int startY, int startZ)
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

		private void TryEnqueueAcceptable(World.Chunk.Model.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int x, int y, int z)
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

		private Mesh BuildIslandMesh(World.Chunk.Model.Cell[,,] cells, int sizeX, int sizeY, int sizeZ)
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

			foreach (Utils.PolygonMerge.PlaneGroup group in planeGroups.Values)
			{
				AppendMergedPlane(group);
			}

			if (vertices.Count == 0)
			{
				return mesh;
			}

			mesh.SetVertices(vertices);
			Utils.Geometry.FlipWindingInPlace(triangles);
			mesh.SetTriangles(triangles, 0);
			mesh.SetUVs(0, uvs);
			return mesh;
		}

		private void AddVoxelCollision(World.Chunk.Model.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int x, int y, int z)
		{
			if (!TryGetCell(cells, x, y, z, out World.Chunk.Model.Cell cell))
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

			Voxel.Model.Orientation orientation = cell.Orientation;
			Voxel.Model.FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			foreach (Voxel.View.Shape.AxisPlane plane in Voxel.View.Shape.AxisPlanes)
			{
				TryAddOuterFaceCollision(cells, sizeX, sizeY, sizeZ, shape, orientation, flipOrientation, position, x, y, z, plane, ref anyOuterVisible);
			}

			if (anyOuterVisible)
			{
				IReadOnlyList<Voxel.Model.Face> innerFaces = shape.InnerFaces;
				for (int i = 0; i < innerFaces.Count; i++)
				{
					CollectFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position);
				}
			}
		}

		private void TryAddOuterFaceCollision(
			World.Chunk.Model.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			Voxel.View.Shape shape,
			Voxel.Model.Orientation orientation,
			Voxel.Model.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Voxel.View.Shape.AxisPlane plane,
			ref bool anyOuterVisible)
		{
			Vector3Int offset = Utils.Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			bool hasNeighbor = TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Model.Cell neighborCell);
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

			Voxel.View.Shape.AxisPlane localPlane = Utils.Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			if (!shape.OuterShellFaces.TryGetValue(localPlane, out Voxel.Model.Face face))
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
			Voxel.Model.Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (hasNeighbor && neighborShape != null)
			{
				Voxel.View.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
				Voxel.Model.Orientation neighborOrientation = neighborCell != null ? neighborCell.Orientation : Voxel.Model.Orientation.PositiveX;
				Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell != null ? neighborCell.FlipOrientation : Voxel.Model.FlipOrientation.PositiveY;
				Voxel.View.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
				if (neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.Model.Face otherFace))
				{
					Voxel.Model.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
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

		private bool IsAcceptableAt(World.Chunk.Model.Cell[,,] cells, int sizeX, int sizeY, int sizeZ, int x, int y, int z)
		{
			if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ)
			{
				return false;
			}

			if (!TryGetCell(cells, x, y, z, out World.Chunk.Model.Cell cell))
			{
				return false;
			}

			int id = cell.Id;
			if (id == Voxel.Service.AirID)
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
			World.Chunk.Model.Cell[,,] cells,
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

			if (!TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Model.Cell neighborCell) || neighborCell == null)
			{
				return false;
			}

			Voxel.View.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			Voxel.Model.Orientation neighborOrientation = neighborCell.Orientation;
			Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Voxel.View.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (!neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.Model.Face otherFace))
			{
				return false;
			}

			Voxel.Model.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			if (!Utils.Geometry.IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
			{
				return false;
			}

			Voxel.Model.Face fullFace = Utils.Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
		}

		private void CollectFace(Voxel.Model.Face face, Vector3 offset)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Voxel.Model.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Voxel.Model.Face.Vertex> polygon = facePolygons[p];
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
			Utils.PolygonMerge.PlaneKey key = Utils.PolygonMerge.PlaneKey.From(normal, distance);
			if (!planeGroups.TryGetValue(key, out Utils.PolygonMerge.PlaneGroup group))
			{
				group = new Utils.PolygonMerge.PlaneGroup(normal, distance);
				planeGroups.Add(key, group);
			}

			group.Polygons.Add(polygon);
		}

		private void AppendMergedPlane(Utils.PolygonMerge.PlaneGroup group)
		{
			if (!Utils.Geometry.TryBuildBasis(group.Normal, out Vector3 tangent, out Vector3 bitangent))
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
					Utils.PolygonMerge.PointKey key = Utils.PolygonMerge.PointKey.From(projected);
					if (!pointKeyTo2D.ContainsKey(key))
					{
						pointKeyTo2D[key] = projected;
						pointKeyTo3D[key] = position;
					}

					Vector3 nextPosition = polygon[(i + 1) % polygon.Count];
					Vector2 nextProjected = new Vector2(Vector3.Dot(nextPosition, tangent), Vector3.Dot(nextPosition, bitangent));
					Utils.PolygonMerge.PointKey nextKey = Utils.PolygonMerge.PointKey.From(nextProjected);
					if (!pointKeyTo2D.ContainsKey(nextKey))
					{
						pointKeyTo2D[nextKey] = nextProjected;
						pointKeyTo3D[nextKey] = nextPosition;
					}

					Utils.PolygonMerge.EdgeKey edgeKey = new Utils.PolygonMerge.EdgeKey(key, nextKey);
					Utils.PolygonMerge.EdgeKey reverseKey = new Utils.PolygonMerge.EdgeKey(nextKey, key);
					if (edges.ContainsKey(reverseKey))
					{
						edges.Remove(reverseKey);
					}
					else
					{
						edges[edgeKey] = new Utils.PolygonMerge.Edge(key, nextKey);
					}
				}
			}

			if (edges.Count == 0)
			{
				return;
			}

			foreach (Utils.PolygonMerge.Edge edge in edges.Values)
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

			Utils.PolygonMerge.Edge startEdge = edgeList[startEdgeIndex];
			Utils.PolygonMerge.PointKey start = startEdge.A;
			Utils.PolygonMerge.PointKey current = startEdge.B;
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
				Utils.PolygonMerge.PointKey nextPoint = default;
				for (int i = 0; i < connected.Count; i++)
				{
					int edgeIndex = connected[i];
					if (used[edgeIndex])
					{
						continue;
					}

					Utils.PolygonMerge.Edge edge = edgeList[edgeIndex];
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

		private void AppendTriangulatedLoop(List<Utils.PolygonMerge.PointKey> loop)
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
				Utils.PolygonMerge.PointKey key = loop[i];
				if (!pointKeyTo2D.TryGetValue(key, out Vector2 p2D) || !pointKeyTo3D.TryGetValue(key, out Vector3 p3D))
				{
					return;
				}
				poly2D.Add(p2D);
				poly3D.Add(p3D);
			}

			Utils.Geometry.RemoveCollinear(poly2D, poly3D);
			if (poly2D.Count < 3)
			{
				return;
			}

			if (Utils.Geometry.SignedArea(poly2D) < 0f)
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
					if (!Utils.Geometry.IsConvex(a, b, c))
					{
						continue;
					}

					if (Utils.Geometry.ContainsPointInTriangle(polygon, polygonIndices, prevIndex, currIndex, nextIndex))
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


	}
}
