using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Core.Utils.Mesher
{
	[Serializable]
	public abstract class CollisionMesher : Utils.Mesher.Mesher
	{
		private const int MaxVerticesPerMesh = 65000;
		private const float MergeEpsilon = 0.001f;

		protected abstract bool IsAcceptableDefinition(Core.Voxel.Model.Definition definition);

		protected virtual string MeshName => "CollisionMesh";

		public List<Mesh> BuildMeshes(Core.Voxel.Model.Cell[,,] cells)
		{
			var meshes = new List<Mesh>();
			if (cells == null)
			{
				return meshes;
			}

			var rectGroups = new Dictionary<RectKey, List<Rect2D>>();
			var polygons = new List<List<Vector3>>();

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						AddVoxel(cells, x, y, z, rectGroups, polygons);
					}
				}
			}

			foreach (var kvp in rectGroups)
			{
				List<Rect2D> merged = MergeRectangles(kvp.Value);
				for (int i = 0; i < merged.Count; i++)
				{
					polygons.Add(RectToPolygon(merged[i]));
				}
			}

			List<List<List<Vector3>>> islands = SplitPolygonsIntoIslands(polygons);
			for (int i = 0; i < islands.Count; i++)
			{
				BuildMeshesFromPolygons(islands[i], meshes, $"{MeshName}_Island{i}");
			}
			return meshes;
		}

		private void AddVoxel(
			Core.Voxel.Model.Cell[,,] cells,
			int x,
			int y,
			int z,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			if (!TryGetCell(cells, x, y, z, out Core.Voxel.Model.Cell cell))
			{
				return;
			}

			if (cell.Id == Core.Voxel.Service.AirID)
			{
				return;
			}

			if (!TryGetDefinition(cell, out Core.Voxel.Model.Definition definition))
			{
				return;
			}

			if (!IsAcceptableDefinition(definition))
			{
				return;
			}

			Core.Voxel.Geometry.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Core.Voxel.Model.Orientation orientation = cell.Orientation;
			Core.Voxel.Model.FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);

			if (shape.Collision == Core.Voxel.Geometry.Shape.CollisionMode.CubeEnvelope)
			{
				for (int i = 0; i < Core.Voxel.Geometry.Shape.AxisPlanes.Length; i++)
				{
					Core.Voxel.Geometry.Shape.AxisPlane plane = Core.Voxel.Geometry.Shape.AxisPlanes[i];
					TryAddCubeEnvelopeFace(cells, position, x, y, z, plane, rectGroups, polygons);
				}

				return;
			}

			bool anyOuterVisible = false;
			for (int i = 0; i < Core.Voxel.Geometry.Shape.AxisPlanes.Length; i++)
			{
				Core.Voxel.Geometry.Shape.AxisPlane plane = Core.Voxel.Geometry.Shape.AxisPlanes[i];
				TryAddOuterFace(
					cells,
					shape,
					orientation,
					flipOrientation,
					position,
					x,
					y,
					z,
					plane,
					ref anyOuterVisible,
					rectGroups,
					polygons);
			}

			if (anyOuterVisible)
			{
				IReadOnlyList<Core.Voxel.Model.Face> innerFaces = shape.InnerFaces;
				if (innerFaces == null)
				{
					return;
				}

				for (int i = 0; i < innerFaces.Count; i++)
				{
					Core.Voxel.Model.Face face = TransformFaceCached(innerFaces[i], orientation, flipOrientation);
					AddFacePolygons(face, position, rectGroups, polygons);
				}
			}
		}

		private void TryAddCubeEnvelopeFace(
			Core.Voxel.Model.Cell[,,] cells,
			Vector3 position,
			int x,
			int y,
			int z,
			Core.Voxel.Geometry.Shape.AxisPlane plane,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			Vector3Int offset = Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			if (IsFullFaceOccludedByNeighbor(cells, neighborX, neighborY, neighborZ, plane))
			{
				return;
			}

			Core.Voxel.Model.Face fullFace = Geometry.GetFullOuterFace(plane);
			AddFacePolygons(fullFace, position, rectGroups, polygons);
		}

		private void TryAddOuterFace(
			Core.Voxel.Model.Cell[,,] cells,
			Core.Voxel.Geometry.Shape shape,
			Core.Voxel.Model.Orientation orientation,
			Core.Voxel.Model.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Core.Voxel.Geometry.Shape.AxisPlane plane,
			ref bool anyOuterVisible,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			Vector3Int offset = Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			Core.Voxel.Geometry.Shape.AxisPlane localPlane = Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			IReadOnlyDictionary<Core.Voxel.Geometry.Shape.AxisPlane, Core.Voxel.Model.Face> outerShellFaces = shape.OuterShellFaces;
			if (outerShellFaces == null || !outerShellFaces.TryGetValue(localPlane, out Core.Voxel.Model.Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullFaceOccludedByNeighbor(cells, neighborX, neighborY, neighborZ, plane))
				{
					anyOuterVisible = true;
				}
				return;
			}

			Core.Voxel.Model.Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (rotatedFace == null)
			{
				return;
			}

			if (IsFaceOccludedByNeighbor(rotatedFace, cells, neighborX, neighborY, neighborZ, plane))
			{
				return;
			}

			AddFacePolygons(rotatedFace, position, rectGroups, polygons);
			anyOuterVisible = true;
		}

		private bool IsFaceOccludedByNeighbor(
			Core.Voxel.Model.Face face,
			Core.Voxel.Model.Cell[,,] cells,
			int neighborX,
			int neighborY,
			int neighborZ,
			Core.Voxel.Geometry.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFace(cells, neighborX, neighborY, neighborZ, plane, out Core.Voxel.Model.Face neighborFace))
			{
				return false;
			}

			return neighborFace != null && face.IsOccludedBy(neighborFace);
		}

		private bool IsFullFaceOccludedByNeighbor(
			Core.Voxel.Model.Cell[,,] cells,
			int neighborX,
			int neighborY,
			int neighborZ,
			Core.Voxel.Geometry.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFace(cells, neighborX, neighborY, neighborZ, plane, out Core.Voxel.Model.Face neighborFace))
			{
				return false;
			}

			if (neighborFace == null)
			{
				return false;
			}

			Core.Voxel.Geometry.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
			if (!Geometry.IsFaceCoplanarWithPlane(neighborFace, oppositePlane))
			{
				return false;
			}

			Core.Voxel.Model.Face fullFace = Geometry.GetFullOuterFace(oppositePlane);
			return fullFace != null && fullFace.IsOccludedBy(neighborFace);
		}

		private bool TryGetOccludingNeighborFace(
			Core.Voxel.Model.Cell[,,] cells,
			int neighborX,
			int neighborY,
			int neighborZ,
			Core.Voxel.Geometry.Shape.AxisPlane plane,
			out Core.Voxel.Model.Face neighborFace)
		{
			neighborFace = null;

			if (!TryGetCell(cells, neighborX, neighborY, neighborZ, out Core.Voxel.Model.Cell neighborCell))
			{
				return false;
			}

			if (neighborCell == null || neighborCell.Id == Core.Voxel.Service.AirID)
			{
				return false;
			}

			if (!TryGetDefinition(neighborCell, out Core.Voxel.Model.Definition neighborDefinition))
			{
				return false;
			}

			if (!IsAcceptableDefinition(neighborDefinition))
			{
				return false;
			}

			Core.Voxel.Geometry.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			Core.Voxel.Geometry.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
			if (neighborShape.Collision == Core.Voxel.Geometry.Shape.CollisionMode.CubeEnvelope)
			{
				neighborFace = Geometry.GetFullOuterFace(oppositePlane);
				return neighborFace != null;
			}

			Core.Voxel.Model.Orientation neighborOrientation = neighborCell.Orientation;
			Core.Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Core.Voxel.Geometry.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			IReadOnlyDictionary<Core.Voxel.Geometry.Shape.AxisPlane, Core.Voxel.Model.Face> neighborOuterFaces = neighborShape.OuterShellFaces;
			if (neighborOuterFaces == null || !neighborOuterFaces.TryGetValue(neighborLocalPlane, out Core.Voxel.Model.Face otherFace))
			{
				return false;
			}

			neighborFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			return neighborFace != null;
		}

		private void AddFacePolygons(
			Core.Voxel.Model.Face face,
			Vector3 positionOffset,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Core.Voxel.Model.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Core.Voxel.Model.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				var worldVerts = new List<Vector3>(faceVertices.Count);
				for (int i = 0; i < faceVertices.Count; i++)
				{
					worldVerts.Add(positionOffset + faceVertices[i].Position);
				}

				if (TryExtractRectOnPlane(worldVerts, out Rect2D rect))
				{
					var key = new RectKey(rect.PlaneNormal, rect.PlaneD);
					if (!rectGroups.TryGetValue(key, out List<Rect2D> list))
					{
						list = new List<Rect2D>();
						rectGroups.Add(key, list);
					}
					list.Add(rect);
				}
				else
				{
					polygons.Add(worldVerts);
				}
			}
		}

		private bool TryExtractRectOnPlane(List<Vector3> worldVerts, out Rect2D rect)
		{
			rect = default;
			if (worldVerts == null || worldVerts.Count != 4)
			{
				return false;
			}

			Vector3 normal = Vector3.Cross(worldVerts[1] - worldVerts[0], worldVerts[2] - worldVerts[0]);
			if (normal.sqrMagnitude < Geometry.NormalEpsilon)
			{
				return false;
			}

			Vector3 canonicalNormal = CanonicalizeNormal(normal.normalized);
			if (!Geometry.TryBuildBasis(canonicalNormal, out Vector3 tangent, out Vector3 bitangent))
			{
				return false;
			}

			float planeD = Vector3.Dot(canonicalNormal, worldVerts[0]);
			for (int i = 1; i < worldVerts.Count; i++)
			{
				float d = Vector3.Dot(canonicalNormal, worldVerts[i]);
				if (Mathf.Abs(d - planeD) > MergeEpsilon)
				{
					return false;
				}
			}

			GetPlaneUV(tangent, bitangent, worldVerts[0], out float u0, out float v0);
			float minU = u0;
			float maxU = u0;
			float minV = v0;
			float maxV = v0;

			for (int i = 1; i < worldVerts.Count; i++)
			{
				GetPlaneUV(tangent, bitangent, worldVerts[i], out float u, out float v);
				minU = Mathf.Min(minU, u);
				maxU = Mathf.Max(maxU, u);
				minV = Mathf.Min(minV, v);
				maxV = Mathf.Max(maxV, v);
			}

			if (Mathf.Abs(maxU - minU) < MergeEpsilon || Mathf.Abs(maxV - minV) < MergeEpsilon)
			{
				return false;
			}

			for (int i = 0; i < worldVerts.Count; i++)
			{
				GetPlaneUV(tangent, bitangent, worldVerts[i], out float u, out float v);
				if (!Approximately(u, minU) && !Approximately(u, maxU))
				{
					return false;
				}

				if (!Approximately(v, minV) && !Approximately(v, maxV))
				{
					return false;
				}
			}

			rect = new Rect2D(canonicalNormal, planeD, tangent, bitangent, minU, maxU, minV, maxV);
			return true;
		}

		private static Vector3 CanonicalizeNormal(Vector3 normal)
		{
			Vector3 n = normal.normalized;
			float ax = Mathf.Abs(n.x);
			float ay = Mathf.Abs(n.y);
			float az = Mathf.Abs(n.z);
			if (ax >= ay && ax >= az)
			{
				return n.x < 0f ? -n : n;
			}
			if (ay >= az)
			{
				return n.y < 0f ? -n : n;
			}
			return n.z < 0f ? -n : n;
		}

		private static void GetPlaneUV(Vector3 tangent, Vector3 bitangent, Vector3 position, out float u, out float v)
		{
			u = Vector3.Dot(position, tangent);
			v = Vector3.Dot(position, bitangent);
		}

		private static bool Approximately(float a, float b)
		{
			return Mathf.Abs(a - b) <= MergeEpsilon;
		}

		private static List<Rect2D> MergeRectangles(List<Rect2D> rects)
		{
			if (rects == null || rects.Count <= 1)
			{
				return rects ?? new List<Rect2D>();
			}

			var working = new List<Rect2D>(rects);
			bool changed = true;
			while (changed)
			{
				changed = false;
				for (int i = 0; i < working.Count; i++)
				{
					for (int j = i + 1; j < working.Count; j++)
					{
						if (TryMerge(working[i], working[j], out Rect2D merged))
						{
							working[i] = merged;
							working.RemoveAt(j);
							changed = true;
							goto NextIteration;
						}
					}
				}
NextIteration:
				;
			}

			return working;
		}

		private static bool TryMerge(Rect2D a, Rect2D b, out Rect2D merged)
		{
			merged = a;
			if (!Approximately(Vector3.Dot(a.PlaneNormal, b.PlaneNormal), 1f) || !Approximately(a.PlaneD, b.PlaneD))
			{
				return false;
			}

			bool sameV = Approximately(a.MinV, b.MinV) && Approximately(a.MaxV, b.MaxV);
			bool sameU = Approximately(a.MinU, b.MinU) && Approximately(a.MaxU, b.MaxU);

			if (sameV && (Approximately(a.MaxU, b.MinU) || Approximately(b.MaxU, a.MinU)))
			{
				float minU = Mathf.Min(a.MinU, b.MinU);
				float maxU = Mathf.Max(a.MaxU, b.MaxU);
				merged = new Rect2D(a.PlaneNormal, a.PlaneD, a.Tangent, a.Bitangent, minU, maxU, a.MinV, a.MaxV);
				return true;
			}

			if (sameU && (Approximately(a.MaxV, b.MinV) || Approximately(b.MaxV, a.MinV)))
			{
				float minV = Mathf.Min(a.MinV, b.MinV);
				float maxV = Mathf.Max(a.MaxV, b.MaxV);
				merged = new Rect2D(a.PlaneNormal, a.PlaneD, a.Tangent, a.Bitangent, a.MinU, a.MaxU, minV, maxV);
				return true;
			}

			return false;
		}

		private static List<Vector3> RectToPolygon(Rect2D rect)
		{
			Vector3 origin = rect.PlaneNormal * rect.PlaneD;
			Vector3 uMin = rect.Tangent * rect.MinU;
			Vector3 uMax = rect.Tangent * rect.MaxU;
			Vector3 vMin = rect.Bitangent * rect.MinV;
			Vector3 vMax = rect.Bitangent * rect.MaxV;

			return new List<Vector3>
			{
				origin + uMin + vMin,
				origin + uMax + vMin,
				origin + uMax + vMax,
				origin + uMin + vMax
			};
		}

		private void BuildMeshesFromPolygons(List<List<Vector3>> polygons, List<Mesh> meshes, string namePrefix)
		{
			if (polygons == null || meshes == null)
			{
				return;
			}

			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			int meshIndex = 0;

			for (int p = 0; p < polygons.Count; p++)
			{
				List<Vector3> polygon = polygons[p];
				if (polygon == null || polygon.Count < 3)
				{
					continue;
				}

				if (vertices.Count + polygon.Count > MaxVerticesPerMesh)
				{
					AddMesh(vertices, triangles, meshes, ref meshIndex, namePrefix);
				}

				int start = vertices.Count;
				for (int i = 0; i < polygon.Count; i++)
				{
					vertices.Add(polygon[i]);
				}

				for (int i = 1; i < polygon.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}

			if (vertices.Count > 0)
			{
				AddMesh(vertices, triangles, meshes, ref meshIndex, namePrefix);
			}
		}

		private void AddMesh(List<Vector3> vertices, List<int> triangles, List<Mesh> meshes, ref int meshIndex, string namePrefix)
		{
			var mesh = new Mesh
			{
				name = meshIndex == 0 ? namePrefix : $"{namePrefix}_{meshIndex}"
			};
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			meshes.Add(mesh);
			meshIndex++;
			vertices.Clear();
			triangles.Clear();
		}

		private static List<List<List<Vector3>>> SplitPolygonsIntoIslands(List<List<Vector3>> polygons)
		{
			var islands = new List<List<List<Vector3>>>();
			if (polygons == null || polygons.Count == 0)
			{
				return islands;
			}

			var validIndices = new List<int>(polygons.Count);
			for (int i = 0; i < polygons.Count; i++)
			{
				List<Vector3> polygon = polygons[i];
				if (polygon != null && polygon.Count >= 3)
				{
					validIndices.Add(i);
				}
			}

			if (validIndices.Count == 0)
			{
				return islands;
			}

			var vertexToPolygons = new Dictionary<VertexKey, List<int>>();
			var polygonVertexKeys = new Dictionary<int, VertexKey[]>();

			for (int i = 0; i < validIndices.Count; i++)
			{
				int polygonIndex = validIndices[i];
				List<Vector3> polygon = polygons[polygonIndex];
				var keys = new VertexKey[polygon.Count];
				for (int v = 0; v < polygon.Count; v++)
				{
					VertexKey key = new VertexKey(polygon[v]);
					keys[v] = key;
					if (!vertexToPolygons.TryGetValue(key, out List<int> polyList))
					{
						polyList = new List<int>();
						vertexToPolygons.Add(key, polyList);
					}
					polyList.Add(polygonIndex);
				}
				polygonVertexKeys.Add(polygonIndex, keys);
			}

			var visited = new HashSet<int>();
			var queue = new Queue<int>();

			for (int i = 0; i < validIndices.Count; i++)
			{
				int startIndex = validIndices[i];
				if (visited.Contains(startIndex))
				{
					continue;
				}

				var islandPolygons = new List<List<Vector3>>();
				queue.Enqueue(startIndex);
				visited.Add(startIndex);

				while (queue.Count > 0)
				{
					int current = queue.Dequeue();
					islandPolygons.Add(polygons[current]);

					if (!polygonVertexKeys.TryGetValue(current, out VertexKey[] keys))
					{
						continue;
					}

					for (int k = 0; k < keys.Length; k++)
					{
						if (!vertexToPolygons.TryGetValue(keys[k], out List<int> neighbors))
						{
							continue;
						}

						for (int n = 0; n < neighbors.Count; n++)
						{
							int neighborIndex = neighbors[n];
							if (visited.Add(neighborIndex))
							{
								queue.Enqueue(neighborIndex);
							}
						}
					}
				}

				if (islandPolygons.Count > 0)
				{
					islands.Add(islandPolygons);
				}
			}

			return islands;
		}

		private readonly struct RectKey : IEquatable<RectKey>
		{
			public readonly int NormalX;
			public readonly int NormalY;
			public readonly int NormalZ;
			public readonly int DKey;

			public RectKey(Vector3 normal, float planeD)
			{
				NormalX = Mathf.RoundToInt(normal.x / MergeEpsilon);
				NormalY = Mathf.RoundToInt(normal.y / MergeEpsilon);
				NormalZ = Mathf.RoundToInt(normal.z / MergeEpsilon);
				DKey = Mathf.RoundToInt(planeD / MergeEpsilon);
			}

			public bool Equals(RectKey other)
			{
				return NormalX == other.NormalX
					&& NormalY == other.NormalY
					&& NormalZ == other.NormalZ
					&& DKey == other.DKey;
			}

			public override bool Equals(object obj)
			{
				return obj is RectKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = NormalX;
					hash = (hash * 397) ^ NormalY;
					hash = (hash * 397) ^ NormalZ;
					hash = (hash * 397) ^ DKey;
					return hash;
				}
			}
		}

		private readonly struct VertexKey : IEquatable<VertexKey>
		{
			public readonly int X;
			public readonly int Y;
			public readonly int Z;

			public VertexKey(Vector3 position)
			{
				X = Mathf.RoundToInt(position.x / MergeEpsilon);
				Y = Mathf.RoundToInt(position.y / MergeEpsilon);
				Z = Mathf.RoundToInt(position.z / MergeEpsilon);
			}

			public bool Equals(VertexKey other)
			{
				return X == other.X && Y == other.Y && Z == other.Z;
			}

			public override bool Equals(object obj)
			{
				return obj is VertexKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = X;
					hash = (hash * 397) ^ Y;
					hash = (hash * 397) ^ Z;
					return hash;
				}
			}
		}

		private readonly struct Rect2D
		{
			public readonly Vector3 PlaneNormal;
			public readonly float PlaneD;
			public readonly Vector3 Tangent;
			public readonly Vector3 Bitangent;
			public readonly float MinU;
			public readonly float MaxU;
			public readonly float MinV;
			public readonly float MaxV;

			public Rect2D(Vector3 planeNormal, float planeD, Vector3 tangent, Vector3 bitangent, float minU, float maxU, float minV, float maxV)
			{
				PlaneNormal = planeNormal;
				PlaneD = planeD;
				Tangent = tangent;
				Bitangent = bitangent;
				MinU = minU;
				MaxU = maxU;
				MinV = minV;
				MaxV = maxV;
			}
		}
	}
}
