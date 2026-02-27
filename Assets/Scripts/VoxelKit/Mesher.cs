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

			var planeGroups = new Dictionary<RectKey, PlaneGroup>();
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
									AddFacePolygons(transformed, offset, planeGroups, polygons);
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
								AddFacePolygons(innerFace, offset, planeGroups, polygons);
							}
						}
					}
				}
			}

			foreach (var kvp in planeGroups)
			{
				PlaneGroup group = kvp.Value;
				if (group == null || group.Polygons == null || group.Polygons.Count == 0)
				{
					continue;
				}

				List<List<Vector3>> merged = MergeCoplanarPolygons(
					group.Polygons,
					group.Normal,
					group.PlaneD,
					group.Tangent,
					group.Bitangent);

				for (int i = 0; i < merged.Count; i++)
				{
					polygons.Add(merged[i]);
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

				AddTriangulatedPolygon(polygon, start, triangles);
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
			Dictionary<RectKey, PlaneGroup> planeGroups,
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

				if (TryExtractPlane(worldVerts, out Vector3 planeNormal, out float planeD, out Vector3 tangent, out Vector3 bitangent))
				{
					NormalizePlane(ref planeNormal, ref planeD);
					if (!VoxelKit.Utils.Geometry.TryBuildBasis(planeNormal, out tangent, out bitangent))
					{
						polygons.Add(worldVerts);
						continue;
					}

					var key = new RectKey(planeNormal, planeD);
					if (!planeGroups.TryGetValue(key, out PlaneGroup group))
					{
						group = new PlaneGroup(planeNormal, planeD, tangent, bitangent);
						planeGroups.Add(key, group);
					}

					if (Vector3.Dot(planeNormal, group.Normal) < 0f)
					{
						worldVerts.Reverse();
					}

					group.Polygons.Add(worldVerts);
				}
				else
				{
					polygons.Add(worldVerts);
				}
			}
		}

		private static bool TryExtractPlane(
			List<Vector3> worldVerts,
			out Vector3 planeNormal,
			out float planeD,
			out Vector3 tangent,
			out Vector3 bitangent)
		{
			planeNormal = Vector3.zero;
			planeD = 0f;
			tangent = Vector3.zero;
			bitangent = Vector3.zero;

			if (worldVerts == null || worldVerts.Count < 3)
			{
				return false;
			}

			Vector3 normal = Vector3.zero;
			for (int i = 0; i + 2 < worldVerts.Count; i++)
			{
				Vector3 candidate = Vector3.Cross(worldVerts[i + 1] - worldVerts[i], worldVerts[i + 2] - worldVerts[i]);
				if (candidate.sqrMagnitude >= VoxelKit.Utils.Geometry.NormalEpsilon)
				{
					normal = candidate;
					break;
				}
			}

			if (normal.sqrMagnitude < VoxelKit.Utils.Geometry.NormalEpsilon)
			{
				return false;
			}

			planeNormal = normal.normalized;
			if (!VoxelKit.Utils.Geometry.TryBuildBasis(planeNormal, out tangent, out bitangent))
			{
				return false;
			}

			planeD = Vector3.Dot(planeNormal, worldVerts[0]);
			for (int i = 1; i < worldVerts.Count; i++)
			{
				float d = Vector3.Dot(planeNormal, worldVerts[i]);
				if (Mathf.Abs(d - planeD) > MergeEpsilon)
				{
					return false;
				}
			}

			return true;
		}

		private static void NormalizePlane(ref Vector3 normal, ref float planeD)
		{
			if (normal.x < -MergeEpsilon
				|| (Mathf.Abs(normal.x) <= MergeEpsilon && normal.y < -MergeEpsilon)
				|| (Mathf.Abs(normal.x) <= MergeEpsilon && Mathf.Abs(normal.y) <= MergeEpsilon && normal.z < -MergeEpsilon))
			{
				normal = -normal;
				planeD = -planeD;
			}
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

		private static List<List<Vector3>> MergeCoplanarPolygons(
			List<List<Vector3>> polygons,
			Vector3 planeNormal,
			float planeD,
			Vector3 tangent,
			Vector3 bitangent)
		{
			if (polygons == null || polygons.Count <= 1)
			{
				return polygons ?? new List<List<Vector3>>();
			}

			var edgeCounts = new Dictionary<UndirectedEdgeKey, int>();
			var directedEdges = new List<DirectedEdge>();
			var pointLookup = new Dictionary<Quantized2D, Vector2>();

			for (int p = 0; p < polygons.Count; p++)
			{
				List<Vector3> poly = polygons[p];
				if (poly == null || poly.Count < 3)
				{
					continue;
				}

				var poly2D = new List<Vector2>(poly.Count);
				for (int i = 0; i < poly.Count; i++)
				{
					GetPlaneUV(tangent, bitangent, poly[i], out float u, out float v);
					poly2D.Add(new Vector2(u, v));
				}

				if (VoxelKit.Utils.Geometry.SignedArea(poly2D) < 0f)
				{
					poly2D.Reverse();
				}

				for (int i = 0; i < poly2D.Count; i++)
				{
					Vector2 a = poly2D[i];
					Vector2 b = poly2D[(i + 1) % poly2D.Count];
					Quantized2D qa = Quantized2D.From(a, MergeEpsilon);
					Quantized2D qb = Quantized2D.From(b, MergeEpsilon);

					if (!pointLookup.ContainsKey(qa))
					{
						pointLookup.Add(qa, a);
					}
					if (!pointLookup.ContainsKey(qb))
					{
						pointLookup.Add(qb, b);
					}

					var key = new UndirectedEdgeKey(qa, qb);
					edgeCounts.TryGetValue(key, out int count);
					edgeCounts[key] = count + 1;

					directedEdges.Add(new DirectedEdge(qa, qb));
				}
			}

			var adjacency = new Dictionary<Quantized2D, List<DirectedEdge>>();
			var unused = new HashSet<DirectedEdge>();

			for (int i = 0; i < directedEdges.Count; i++)
			{
				DirectedEdge edge = directedEdges[i];
				var key = new UndirectedEdgeKey(edge.From, edge.To);
				if (!edgeCounts.TryGetValue(key, out int count) || count != 1)
				{
					continue;
				}

				if (!adjacency.TryGetValue(edge.From, out List<DirectedEdge> list))
				{
					list = new List<DirectedEdge>();
					adjacency.Add(edge.From, list);
				}
				list.Add(edge);
				unused.Add(edge);
			}

			if (unused.Count == 0)
			{
				return polygons;
			}

			var loops = new List<List<Vector2>>();
			int safety = 0;
			while (unused.Count > 0 && safety++ < 100000)
			{
				DirectedEdge startEdge = GetAny(unused);
				unused.Remove(startEdge);

				var loop = new List<Vector2>();
				Quantized2D start = startEdge.From;
				Quantized2D current = startEdge.To;
				Quantized2D prev = startEdge.From;
				loop.Add(pointLookup[start]);
				loop.Add(pointLookup[current]);

				while (!current.Equals(start))
				{
					if (!adjacency.TryGetValue(current, out List<DirectedEdge> nextEdges) || nextEdges.Count == 0)
					{
						break;
					}

					DirectedEdge chosen = default;
					bool found = false;
					if (nextEdges.Count == 1)
					{
						chosen = nextEdges[0];
						found = unused.Contains(chosen);
					}
					else
					{
						Vector2 prevDir = pointLookup[current] - pointLookup[prev];
						float bestAngle = float.MaxValue;
						for (int i = 0; i < nextEdges.Count; i++)
						{
							DirectedEdge candidate = nextEdges[i];
							if (!unused.Contains(candidate))
							{
								continue;
							}

							Vector2 candDir = pointLookup[candidate.To] - pointLookup[current];
							float angle = AngleBetween(prevDir, candDir);
							if (angle < bestAngle)
							{
								bestAngle = angle;
								chosen = candidate;
								found = true;
							}
						}
					}

					if (!found)
					{
						break;
					}

					unused.Remove(chosen);
					prev = current;
					current = chosen.To;
					loop.Add(pointLookup[current]);
				}

				if (loop.Count >= 3)
				{
					loops.Add(loop);
				}
			}

			if (loops.Count == 0)
			{
				return polygons;
			}

			var result = new List<List<Vector3>>(loops.Count);
			Vector3 origin = planeNormal * planeD;
			for (int l = 0; l < loops.Count; l++)
			{
				List<Vector2> merged2D = loops[l];
				if (merged2D == null || merged2D.Count < 3)
				{
					continue;
				}

				if (VoxelKit.Utils.Geometry.SignedArea(merged2D) < 0f)
				{
					merged2D.Reverse();
				}

				var merged3D = new List<Vector3>(merged2D.Count);
				for (int i = 0; i < merged2D.Count; i++)
				{
					Vector2 uv = merged2D[i];
					merged3D.Add(origin + tangent * uv.x + bitangent * uv.y);
				}

				result.Add(merged3D);
			}

			if (result.Count == 0)
			{
				return polygons;
			}

			return result;
		}

		private static DirectedEdge GetAny(HashSet<DirectedEdge> set)
		{
			foreach (var edge in set)
			{
				return edge;
			}

			return default;
		}

		private static float AngleBetween(Vector2 from, Vector2 to)
		{
			float angle = Mathf.Atan2(VoxelKit.Utils.Geometry.Cross(from, to), Vector2.Dot(from, to));
			if (angle < 0f)
			{
				angle += Mathf.PI * 2f;
			}
			return angle;
		}

		private static void AddTriangulatedPolygon(List<Vector3> polygon, int startIndex, List<int> triangles)
		{
			if (polygon == null || polygon.Count < 3)
			{
				return;
			}

			Vector3 normal = Vector3.zero;
			for (int i = 0; i + 2 < polygon.Count; i++)
			{
				Vector3 candidate = Vector3.Cross(polygon[i + 1] - polygon[i], polygon[i + 2] - polygon[i]);
				if (candidate.sqrMagnitude >= VoxelKit.Utils.Geometry.NormalEpsilon)
				{
					normal = candidate;
					break;
				}
			}

			if (normal.sqrMagnitude < VoxelKit.Utils.Geometry.NormalEpsilon)
			{
				for (int i = 1; i < polygon.Count - 1; i++)
				{
					triangles.Add(startIndex);
					triangles.Add(startIndex + i + 1);
					triangles.Add(startIndex + i);
				}
				return;
			}

			if (!VoxelKit.Utils.Geometry.TryBuildBasis(normal, out Vector3 tangent, out Vector3 bitangent))
			{
				for (int i = 1; i < polygon.Count - 1; i++)
				{
					triangles.Add(startIndex);
					triangles.Add(startIndex + i + 1);
					triangles.Add(startIndex + i);
				}
				return;
			}

			var poly2D = new List<Vector2>(polygon.Count);
			var map = new List<int>(polygon.Count);
			for (int i = 0; i < polygon.Count; i++)
			{
				GetPlaneUV(tangent, bitangent, polygon[i], out float u, out float v);
				poly2D.Add(new Vector2(u, v));
				map.Add(i);
			}

			if (VoxelKit.Utils.Geometry.SignedArea(poly2D) < 0f)
			{
				poly2D.Reverse();
				map.Reverse();
			}

			RemoveCollinear(poly2D, map);
			if (poly2D.Count < 3)
			{
				return;
			}

			var indices = new List<int>(map);

			int guard = 0;
			while (indices.Count > 3 && guard++ < 10000)
			{
				bool earFound = false;
				for (int i = 0; i < indices.Count; i++)
				{
					int prevIndex = indices[(i - 1 + indices.Count) % indices.Count];
					int currIndex = indices[i];
					int nextIndex = indices[(i + 1) % indices.Count];

					int prevLocal = map.IndexOf(prevIndex);
					int currLocal = map.IndexOf(currIndex);
					int nextLocal = map.IndexOf(nextIndex);

					Vector2 a = poly2D[prevLocal];
					Vector2 b = poly2D[currLocal];
					Vector2 c = poly2D[nextLocal];

					if (!VoxelKit.Utils.Geometry.IsConvex(a, b, c))
					{
						continue;
					}

					if (ContainsPointInTriangleMapped(poly2D, map, indices, prevIndex, currIndex, nextIndex))
					{
						continue;
					}

					triangles.Add(startIndex + prevIndex);
					triangles.Add(startIndex + currIndex);
					triangles.Add(startIndex + nextIndex);
					indices.RemoveAt(i);
					earFound = true;
					break;
				}

				if (!earFound)
				{
					break;
				}
			}

			if (indices.Count == 3)
			{
				triangles.Add(startIndex + indices[0]);
				triangles.Add(startIndex + indices[1]);
				triangles.Add(startIndex + indices[2]);
			}
		}

		private static void RemoveCollinear(List<Vector2> poly2D, List<int> map)
		{
			int i = 0;
			while (i < poly2D.Count && poly2D.Count >= 3)
			{
				int prev = (i - 1 + poly2D.Count) % poly2D.Count;
				int next = (i + 1) % poly2D.Count;
				Vector2 a = poly2D[prev];
				Vector2 b = poly2D[i];
				Vector2 c = poly2D[next];
				if (Mathf.Abs(VoxelKit.Utils.Geometry.Cross(b - a, c - b)) < 0.0001f)
				{
					poly2D.RemoveAt(i);
					map.RemoveAt(i);
					if (i > 0)
					{
						i--;
					}
					continue;
				}
				i++;
			}
		}

		private static bool ContainsPointInTriangleMapped(
			List<Vector2> poly2D,
			List<int> map,
			List<int> indices,
			int aIndex,
			int bIndex,
			int cIndex)
		{
			int aLocal = map.IndexOf(aIndex);
			int bLocal = map.IndexOf(bIndex);
			int cLocal = map.IndexOf(cIndex);
			if (aLocal < 0 || bLocal < 0 || cLocal < 0)
			{
				return false;
			}

			Vector2 a = poly2D[aLocal];
			Vector2 b = poly2D[bLocal];
			Vector2 c = poly2D[cLocal];

			for (int i = 0; i < indices.Count; i++)
			{
				int idx = indices[i];
				if (idx == aIndex || idx == bIndex || idx == cIndex)
				{
					continue;
				}

				int local = map.IndexOf(idx);
				if (local < 0)
				{
					continue;
				}

				if (VoxelKit.Utils.Geometry.IsPointInTriangle(poly2D[local], a, b, c))
				{
					return true;
				}
			}

			return false;
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

		private sealed class PlaneGroup
		{
			public readonly Vector3 Normal;
			public readonly float PlaneD;
			public readonly Vector3 Tangent;
			public readonly Vector3 Bitangent;
			public readonly List<List<Vector3>> Polygons;

			public PlaneGroup(Vector3 normal, float planeD, Vector3 tangent, Vector3 bitangent)
			{
				Normal = normal;
				PlaneD = planeD;
				Tangent = tangent;
				Bitangent = bitangent;
				Polygons = new List<List<Vector3>>();
			}
		}

		private readonly struct Quantized2D : IEquatable<Quantized2D>
		{
			public readonly int X;
			public readonly int Y;

			public Quantized2D(int x, int y)
			{
				X = x;
				Y = y;
			}

			public static Quantized2D From(Vector2 value, float epsilon)
			{
				return new Quantized2D(
					Mathf.RoundToInt(value.x / epsilon),
					Mathf.RoundToInt(value.y / epsilon));
			}

			public bool Equals(Quantized2D other)
			{
				return X == other.X && Y == other.Y;
			}

			public override bool Equals(object obj)
			{
				return obj is Quantized2D other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = X;
					hash = (hash * 397) ^ Y;
					return hash;
				}
			}
		}

		private readonly struct UndirectedEdgeKey : IEquatable<UndirectedEdgeKey>
		{
			private readonly Quantized2D A;
			private readonly Quantized2D B;

			public UndirectedEdgeKey(Quantized2D a, Quantized2D b)
			{
				if (a.X < b.X || (a.X == b.X && a.Y <= b.Y))
				{
					A = a;
					B = b;
				}
				else
				{
					A = b;
					B = a;
				}
			}

			public bool Equals(UndirectedEdgeKey other)
			{
				return A.Equals(other.A) && B.Equals(other.B);
			}

			public override bool Equals(object obj)
			{
				return obj is UndirectedEdgeKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = A.GetHashCode();
					hash = (hash * 397) ^ B.GetHashCode();
					return hash;
				}
			}
		}

		private readonly struct DirectedEdge : IEquatable<DirectedEdge>
		{
			public readonly Quantized2D From;
			public readonly Quantized2D To;

			public DirectedEdge(Quantized2D from, Quantized2D to)
			{
				From = from;
				To = to;
			}

			public bool Equals(DirectedEdge other)
			{
				return From.Equals(other.From) && To.Equals(other.To);
			}

			public override bool Equals(object obj)
			{
				return obj is DirectedEdge other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = From.GetHashCode();
					hash = (hash * 397) ^ To.GetHashCode();
					return hash;
				}
			}
		}
	}
}


