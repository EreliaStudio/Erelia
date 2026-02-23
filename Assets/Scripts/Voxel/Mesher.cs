using System;
using System.Collections.Generic;
using UnityEngine;
namespace VoxelKit
{
	static class Mesher
	{
		public static readonly Func<VoxelKit.Definition, bool> AnyVoxelPredicate = (definition) => true;
		public static readonly Func<VoxelKit.Definition, bool> OnlyObstacleVoxelPredicate = (definition) => definition.Data.Traversal == VoxelKit.Traversal.Obstacle;
		public static readonly Func<VoxelKit.Definition, bool> OnlyWalkableVoxelPredicate = (definition) => definition.Data.Traversal == VoxelKit.Traversal.Walkable;

		private const float MergeEpsilon = 0.001f;

		static public UnityEngine.Mesh BuildRenderMesh(
			VoxelKit.Cell[,,] cells,
			VoxelKit.Registry registry,
			Func<VoxelKit.Definition, bool> predicate)
		{
			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var uvs = new List<Vector2>();

			if (cells == null || registry == null)
			{
				return new UnityEngine.Mesh();
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						if (!TryGetDefinition(cells[x, y, z], registry, predicate, out VoxelKit.Definition definition, out VoxelKit.Cell cell))
						{
							continue;
						}

						VoxelKit.Shape shape = definition.Shape;
						if (shape == null)
						{
							continue;
						}

						VoxelKit.Shape.FaceSet faceSet = shape.RenderFaces;
						Vector3 offset = new Vector3(x, y, z);

						bool anyOuterVisible = false;
						for (int i = 0; i < VoxelKit.Shape.AxisPlanes.Length; i++)
						{
							VoxelKit.Shape.AxisPlane worldPlane = VoxelKit.Shape.AxisPlanes[i];
							VoxelKit.Shape.AxisPlane localPlane = VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

							if (faceSet.OuterShell != null
								&& faceSet.OuterShell.TryGetValue(localPlane, out VoxelKit.Face localFace)
								&& localFace != null
								&& localFace.HasRenderablePolygons)
							{
								if (!IsFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, cell, localFace, worldPlane, registry, predicate, useCollision: false))
								{
									VoxelKit.Face transformed = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
									AddFace(transformed, offset, vertices, triangles, uvs);
									anyOuterVisible = true;
								}
							}
							else
							{
								if (!IsFullFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, worldPlane, registry, predicate, useCollision: false))
								{
									anyOuterVisible = true;
								}
							}
						}

						if (anyOuterVisible && faceSet.Inner != null)
						{
							for (int i = 0; i < faceSet.Inner.Count; i++)
							{
								VoxelKit.Face innerFace = TransformFaceCached(faceSet.Inner[i], cell.Orientation, cell.FlipOrientation);
								AddFace(innerFace, offset, vertices, triangles, uvs);
							}
						}
					}
				}
			}

			var result = new UnityEngine.Mesh();
			if (vertices.Count >= 65535)
			{
				result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			result.SetVertices(vertices);
			result.SetTriangles(triangles, 0);
			result.SetUVs(0, uvs);
			result.RecalculateNormals();
			result.RecalculateBounds();
			return result;
		}
	
		static public UnityEngine.Mesh BuildCollisionMesh(
			VoxelKit.Cell[,,] cells,
			VoxelKit.Registry registry,
			Func<VoxelKit.Definition, bool> predicate)
		{
			if (cells == null || registry == null)
			{
				return new UnityEngine.Mesh();
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
						if (!TryGetDefinition(cells[x, y, z], registry, predicate, out VoxelKit.Definition definition, out VoxelKit.Cell cell))
						{
							continue;
						}

						VoxelKit.Shape shape = definition.Shape;
						if (shape == null)
						{
							continue;
						}

						VoxelKit.Shape.FaceSet faceSet = shape.CollisionFaces;
						Vector3 offset = new Vector3(x, y, z);

						bool anyOuterVisible = false;
						for (int i = 0; i < VoxelKit.Shape.AxisPlanes.Length; i++)
						{
							VoxelKit.Shape.AxisPlane worldPlane = VoxelKit.Shape.AxisPlanes[i];
							VoxelKit.Shape.AxisPlane localPlane = VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

							if (faceSet.OuterShell != null
								&& faceSet.OuterShell.TryGetValue(localPlane, out VoxelKit.Face localFace)
								&& localFace != null
								&& localFace.HasRenderablePolygons)
							{
								if (!IsFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, cell, localFace, worldPlane, registry, predicate, useCollision: true))
								{
									VoxelKit.Face transformed = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
									AddFacePolygons(transformed, offset, rectGroups, polygons);
									anyOuterVisible = true;
								}
							}
							else
							{
								if (!IsFullFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, worldPlane, registry, predicate, useCollision: true))
								{
									anyOuterVisible = true;
								}
							}
						}

						if (anyOuterVisible && faceSet.Inner != null)
						{
							for (int i = 0; i < faceSet.Inner.Count; i++)
							{
								VoxelKit.Face innerFace = TransformFaceCached(faceSet.Inner[i], cell.Orientation, cell.FlipOrientation);
								AddFacePolygons(innerFace, offset, rectGroups, polygons);
							}
						}
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

			var result = new UnityEngine.Mesh();
			var vertices = new List<Vector3>();
			var triangles = new List<int>();

			for (int p = 0; p < polygons.Count; p++)
			{
				List<Vector3> polygon = polygons[p];
				if (polygon == null || polygon.Count < 3)
				{
					continue;
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

			if (vertices.Count >= 65535)
			{
				result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			result.SetVertices(vertices);
			result.SetTriangles(triangles, 0);
			result.RecalculateNormals();
			result.RecalculateBounds();
			return result;
		}

		private static bool TryGetDefinition(
			VoxelKit.Cell cell,
			VoxelKit.Registry registry,
			Func<VoxelKit.Definition, bool> predicate,
			out VoxelKit.Definition definition,
			out VoxelKit.Cell resolvedCell)
		{
			definition = null;
			resolvedCell = cell;
			if (cell == null || cell.Id < 0 || registry == null)
			{
				return false;
			}

			if (!registry.TryGet(cell.Id, out definition))
			{
				return false;
			}

			if (predicate != null && !predicate(definition))
			{
				return false;
			}

			return true;
		}

		private static VoxelKit.Face TransformFaceCached(
			VoxelKit.Face face,
			VoxelKit.Orientation orientation,
			VoxelKit.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (VoxelKit.Mesherutils.FaceByOrientationCache.TryGetValue(face, orientation, flipOrientation, out VoxelKit.Face output))
			{
				return output;
			}

			return face;
		}

		private static void AddFace(
			VoxelKit.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<VoxelKit.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<VoxelKit.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					VoxelKit.Face.Vertex vertex = faceVertices[i];
					vertices.Add(positionOffset + vertex.Position);
					uvs.Add(vertex.TileUV);
				}

				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

		private static bool IsFaceOccludedByNeighbor(
			VoxelKit.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			int x,
			int y,
			int z,
			VoxelKit.Cell cell,
			VoxelKit.Face localFace,
			VoxelKit.Shape.AxisPlane worldPlane,
			VoxelKit.Registry registry,
			Func<VoxelKit.Definition, bool> predicate,
			bool useCollision)
		{
			Vector3Int offset = VoxelKit.Utils.Geometry.PlaneToOffset(worldPlane);
			int nx = x + offset.x;
			int ny = y + offset.y;
			int nz = z + offset.z;

			if (nx < 0 || ny < 0 || nz < 0 || nx >= sizeX || ny >= sizeY || nz >= sizeZ)
			{
				return false;
			}

			if (!TryGetDefinition(cells[nx, ny, nz], registry, predicate, out VoxelKit.Definition neighborDefinition, out VoxelKit.Cell neighborCell))
			{
				return false;
			}

			VoxelKit.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			VoxelKit.Shape.FaceSet neighborFaceSet = useCollision ? neighborShape.CollisionFaces : neighborShape.RenderFaces;

			VoxelKit.Shape.AxisPlane oppositePlane = VoxelKit.Utils.Geometry.GetOppositePlane(worldPlane);
			VoxelKit.Shape.AxisPlane neighborLocalPlane = VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (neighborFaceSet.OuterShell == null
				|| !neighborFaceSet.OuterShell.TryGetValue(neighborLocalPlane, out VoxelKit.Face neighborLocalFace)
				|| neighborLocalFace == null
				|| !neighborLocalFace.HasRenderablePolygons)
			{
				return false;
			}

			VoxelKit.Face faceWorld = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
			VoxelKit.Face neighborWorld = TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (faceWorld == null || neighborWorld == null)
			{
				return false;
			}

			if (VoxelKit.Mesherutils.FaceVsFaceOcclusionCache.TryGetValue(faceWorld, neighborWorld, out bool occluded))
			{
				return occluded;
			}

			return false;
		}

		private static bool IsFullFaceOccludedByNeighbor(
			VoxelKit.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			int x,
			int y,
			int z,
			VoxelKit.Shape.AxisPlane worldPlane,
			VoxelKit.Registry registry,
			Func<VoxelKit.Definition, bool> predicate,
			bool useCollision)
		{
			Vector3Int offset = VoxelKit.Utils.Geometry.PlaneToOffset(worldPlane);
			int nx = x + offset.x;
			int ny = y + offset.y;
			int nz = z + offset.z;

			if (nx < 0 || ny < 0 || nz < 0 || nx >= sizeX || ny >= sizeY || nz >= sizeZ)
			{
				return false;
			}

			if (!TryGetDefinition(cells[nx, ny, nz], registry, predicate, out VoxelKit.Definition neighborDefinition, out VoxelKit.Cell neighborCell))
			{
				return false;
			}

			VoxelKit.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			VoxelKit.Shape.FaceSet neighborFaceSet = useCollision ? neighborShape.CollisionFaces : neighborShape.RenderFaces;

			VoxelKit.Shape.AxisPlane oppositePlane = VoxelKit.Utils.Geometry.GetOppositePlane(worldPlane);
			VoxelKit.Shape.AxisPlane neighborLocalPlane = VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (neighborFaceSet.OuterShell == null
				|| !neighborFaceSet.OuterShell.TryGetValue(neighborLocalPlane, out VoxelKit.Face neighborLocalFace)
				|| neighborLocalFace == null
				|| !neighborLocalFace.HasRenderablePolygons)
			{
				return false;
			}

			VoxelKit.Face neighborWorld = TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);
			if (neighborWorld == null)
			{
				return false;
			}

			VoxelKit.Face fullFace = VoxelKit.Utils.Geometry.GetFullOuterFace(oppositePlane);
			if (fullFace == null)
			{
				return false;
			}

			if (VoxelKit.Mesherutils.FaceVsFaceOcclusionCache.TryGetValue(fullFace, neighborWorld, out bool occluded))
			{
				return occluded;
			}

			return VoxelKit.Utils.Geometry.IsFullFace(neighborWorld, oppositePlane);
		}

		private static void AddFacePolygons(
			VoxelKit.Face face,
			Vector3 positionOffset,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<VoxelKit.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<VoxelKit.Face.Vertex> faceVertices = facePolygons[p];
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

		private static bool TryExtractRectOnPlane(List<Vector3> worldVerts, out Rect2D rect)
		{
			rect = default;
			if (worldVerts == null || worldVerts.Count != 4)
			{
				return false;
			}

			Vector3 normal = Vector3.Cross(worldVerts[1] - worldVerts[0], worldVerts[2] - worldVerts[0]);
			if (normal.sqrMagnitude < VoxelKit.Utils.Geometry.NormalEpsilon)
			{
				return false;
			}

			Vector3 planeNormal = normal.normalized;
			if (!VoxelKit.Utils.Geometry.TryBuildBasis(planeNormal, out Vector3 tangent, out Vector3 bitangent))
			{
				return false;
			}

			float planeD = Vector3.Dot(planeNormal, worldVerts[0]);
			for (int i = 1; i < worldVerts.Count; i++)
			{
				float d = Vector3.Dot(planeNormal, worldVerts[i]);
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

			rect = new Rect2D(planeNormal, planeD, tangent, bitangent, minU, maxU, minV, maxV);
			return true;
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


