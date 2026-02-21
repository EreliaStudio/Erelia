using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Voxel
{
	static class Mesher
	{
		public delegate bool Predicate(Cell[,,] cells, int x, int y, int z, Cell cell, Definition definition);

		private static readonly Predicate DefaultPredicate = (cells, x, y, z, cell, definition) => true;
		private const int MaxVerticesPerMesh = 65000;
		private const float MergeEpsilon = 0.001f;

		public static Mesh BuildRenderMesh(Voxel.Cell[,,] pack, Predicate predicate)
		{
			Mesh result = new Mesh();
			if (pack == null)
			{
				return result;
			}

			predicate ??= DefaultPredicate;

			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var uvs = new List<Vector2>();

			int sizeX = pack.GetLength(0);
			int sizeY = pack.GetLength(1);
			int sizeZ = pack.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						AddRenderVoxel(pack, predicate, x, y, z, vertices, triangles, uvs);
					}
				}
			}

			result.SetVertices(vertices);
			result.SetTriangles(triangles, 0);
			result.SetUVs(0, uvs);
			result.RecalculateNormals();
			result.RecalculateBounds();

			//Build the render mesh for the whole voxel pack. No need to split it into multiples separated meshes if they are concaves, as render doesn't care about convex state

			return result;
		}

		public static List<Mesh> BuildCollisionMeshes(Voxel.Cell[,,] pack, Predicate predicate, bool NeedConvexification)
		{
			List<Mesh> result = new List<Mesh>();
			if (pack == null)
			{
				return result;
			}

			predicate ??= DefaultPredicate;

			var rectGroups = new Dictionary<RectKey, List<Rect2D>>();
			var polygons = new List<List<Vector3>>();

			int sizeX = pack.GetLength(0);
			int sizeY = pack.GetLength(1);
			int sizeZ = pack.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						AddCollisionVoxel(pack, predicate, x, y, z, rectGroups, polygons);
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

			if (polygons.Count == 0)
			{
				return result;
			}

			List<List<List<Vector3>>> islands = NeedConvexification
				? SplitPolygonsIntoIslands(polygons)
				: new List<List<List<Vector3>>> { polygons };

			for (int i = 0; i < islands.Count; i++)
			{
				BuildMeshesFromPolygons(islands[i], result, $"CollisionMesh_Island{i}");
			}

			//Build the collision mesh for the whole voxel pack.
			// Note that, contrary of BuildRenderMesh, if NeedConvexification is set to true, this function must return a list of mesh that are strictly convex, as it may be used for trigger or not. 
			// If the user set NeedConvexification to false, no need to make it convex, you can keep a concave shape

			return result;
		}

		private static void AddRenderVoxel(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int x,
			int y,
			int z,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (!TryGetCell(pack, x, y, z, out Cell cell))
			{
				return;
			}

			if (cell.Id < 0)
			{
				return;
			}

			if (!TryGetDefinition(cell, out Definition definition))
			{
				return;
			}

			if (!predicate(pack, x, y, z, cell, definition))
			{
				return;
			}

			Erelia.Voxel.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Erelia.Voxel.Orientation orientation = cell.Orientation;
			Erelia.Voxel.FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			for (int i = 0; i < Erelia.Voxel.Shape.AxisPlanes.Length; i++)
			{
				Erelia.Voxel.Shape.AxisPlane plane = Erelia.Voxel.Shape.AxisPlanes[i];
				TryAddOuterRenderFace(
					pack,
					shape,
					orientation,
					flipOrientation,
					position,
					x,
					y,
					z,
					plane,
					ref anyOuterVisible,
					vertices,
					triangles,
					uvs);
			}

			if (!anyOuterVisible)
			{
				return;
			}

			List<Erelia.Voxel.Face> innerFaces = shape.RenderFaces.Inner;
			if (innerFaces == null)
			{
				return;
			}

			for (int i = 0; i < innerFaces.Count; i++)
			{
				AddFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
			}
		}

		private static void TryAddOuterRenderFace(
			Voxel.Cell[,,] pack,
			Erelia.Voxel.Shape shape,
			Erelia.Voxel.Orientation orientation,
			Erelia.Voxel.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Erelia.Voxel.Shape.AxisPlane plane,
			ref bool anyOuterVisible,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			Vector3Int offset = Utils.Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			bool hasNeighbor = TryGetCell(pack, neighborX, neighborY, neighborZ, out Cell neighborCell);
			Definition neighborDefinition = null;
			if (hasNeighbor && !TryGetDefinition(neighborCell, out neighborDefinition))
			{
				hasNeighbor = false;
			}

			Erelia.Voxel.Shape neighborShape = hasNeighbor ? neighborDefinition.Shape : null;
			Erelia.Voxel.Shape.AxisPlane localPlane = Utils.Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> outerShellFaces = shape.RenderFaces.OuterShell;
			if (outerShellFaces == null || !outerShellFaces.TryGetValue(localPlane, out Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullyOccludedByNeighborRender(pack, neighborX, neighborY, neighborZ, plane))
				{
					anyOuterVisible = true;
				}
				return;
			}

			Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (rotatedFace == null)
			{
				return;
			}

			bool isOccluded = false;
			if (hasNeighbor && neighborShape != null)
			{
				Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
				Erelia.Voxel.Orientation neighborOrientation = neighborCell != null ? neighborCell.Orientation : Erelia.Voxel.Orientation.PositiveX;
				Erelia.Voxel.FlipOrientation neighborFlipOrientation = neighborCell != null ? neighborCell.FlipOrientation : Erelia.Voxel.FlipOrientation.PositiveY;
				Erelia.Voxel.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
				Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> neighborOuterFaces = neighborShape.RenderFaces.OuterShell;
				if (neighborOuterFaces != null && neighborOuterFaces.TryGetValue(neighborLocalPlane, out Face otherFace))
				{
					Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
					isOccluded = IsFaceOccluded(rotatedFace, rotatedOtherFace);
				}
			}

			if (isOccluded)
			{
				return;
			}

			AddFace(rotatedFace, position, vertices, triangles, uvs);
			anyOuterVisible = true;
		}

		private static bool IsFullyOccludedByNeighborRender(
			Voxel.Cell[,,] pack,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane)
		{
			if (!TryGetCell(pack, neighborX, neighborY, neighborZ, out Cell neighborCell))
			{
				return false;
			}

			if (neighborCell == null || neighborCell.Id < 0)
			{
				return false;
			}

			if (!TryGetDefinition(neighborCell, out Definition neighborDefinition))
			{
				return false;
			}

			Erelia.Voxel.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			Erelia.Voxel.Orientation neighborOrientation = neighborCell.Orientation;
			Erelia.Voxel.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Erelia.Voxel.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (neighborShape.RenderFaces.OuterShell == null
				|| !neighborShape.RenderFaces.OuterShell.TryGetValue(neighborLocalPlane, out Face otherFace))
			{
				return false;
			}

			Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			if (!Utils.Geometry.IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
			{
				return false;
			}

			Face fullFace = Utils.Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
		}

		private static void AddCollisionVoxel(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int x,
			int y,
			int z,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			if (!TryGetCell(pack, x, y, z, out Cell cell))
			{
				return;
			}

			if (cell.Id < 0)
			{
				return;
			}

			if (!TryGetDefinition(cell, out Definition definition))
			{
				return;
			}

			if (!predicate(pack, x, y, z, cell, definition))
			{
				return;
			}

			Erelia.Voxel.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Erelia.Voxel.Orientation orientation = cell.Orientation;
			Erelia.Voxel.FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			for (int i = 0; i < Erelia.Voxel.Shape.AxisPlanes.Length; i++)
			{
				Erelia.Voxel.Shape.AxisPlane plane = Erelia.Voxel.Shape.AxisPlanes[i];
				TryAddOuterCollisionFace(
					pack,
					predicate,
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

			if (!anyOuterVisible)
			{
				return;
			}

			List<Erelia.Voxel.Face> innerFaces = shape.CollisionFaces.Inner;
			if (innerFaces == null)
			{
				return;
			}

			for (int i = 0; i < innerFaces.Count; i++)
			{
				Face rotated = TransformFaceCached(innerFaces[i], orientation, flipOrientation);
				AddFacePolygons(rotated, position, rectGroups, polygons);
			}
		}

		private static void TryAddOuterCollisionFace(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			Erelia.Voxel.Shape shape,
			Erelia.Voxel.Orientation orientation,
			Erelia.Voxel.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Erelia.Voxel.Shape.AxisPlane plane,
			ref bool anyOuterVisible,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			Vector3Int offset = Utils.Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			Erelia.Voxel.Shape.AxisPlane localPlane = Utils.Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> outerShellFaces = shape.CollisionFaces.OuterShell;
			if (outerShellFaces == null || !outerShellFaces.TryGetValue(localPlane, out Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullFaceOccludedByNeighborCollision(pack, predicate, neighborX, neighborY, neighborZ, plane))
				{
					anyOuterVisible = true;
				}
				return;
			}

			Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (rotatedFace == null)
			{
				return;
			}

			if (IsFaceOccludedByNeighborCollision(rotatedFace, pack, predicate, neighborX, neighborY, neighborZ, plane))
			{
				return;
			}

			AddFacePolygons(rotatedFace, position, rectGroups, polygons);
			anyOuterVisible = true;
		}

		private static bool IsFaceOccludedByNeighborCollision(
			Face face,
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFaceCollision(pack, predicate, neighborX, neighborY, neighborZ, plane, out Face neighborFace))
			{
				return false;
			}

			return neighborFace != null && IsFaceOccluded(face, neighborFace);
		}

		private static bool IsFullFaceOccludedByNeighborCollision(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFaceCollision(pack, predicate, neighborX, neighborY, neighborZ, plane, out Face neighborFace))
			{
				return false;
			}

			if (neighborFace == null)
			{
				return false;
			}

			Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			if (!Utils.Geometry.IsFaceCoplanarWithPlane(neighborFace, oppositePlane))
			{
				return false;
			}

			Face fullFace = Utils.Geometry.GetFullOuterFace(oppositePlane);
			return fullFace != null && fullFace.IsOccludedBy(neighborFace);
		}

		private static bool TryGetOccludingNeighborFaceCollision(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane,
			out Face neighborFace)
		{
			neighborFace = null;

			if (!TryGetCell(pack, neighborX, neighborY, neighborZ, out Cell neighborCell))
			{
				return false;
			}

			if (neighborCell == null || neighborCell.Id < 0)
			{
				return false;
			}

			if (!TryGetDefinition(neighborCell, out Definition neighborDefinition))
			{
				return false;
			}

			if (!predicate(pack, neighborX, neighborY, neighborZ, neighborCell, neighborDefinition))
			{
				return false;
			}

			Erelia.Voxel.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			Erelia.Voxel.Orientation neighborOrientation = neighborCell.Orientation;
			Erelia.Voxel.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Erelia.Voxel.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> neighborOuterFaces = neighborShape.CollisionFaces.OuterShell;
			if (neighborOuterFaces == null || !neighborOuterFaces.TryGetValue(neighborLocalPlane, out Face otherFace))
			{
				return false;
			}

			neighborFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			return neighborFace != null;
		}

		private static void AddFace(
			Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Face.Vertex>> facePolygons = face.Polygons;

			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Face.Vertex vertex = faceVertices[i];
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

		private static void AddFacePolygons(
			Face face,
			Vector3 positionOffset,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Face.Vertex> faceVertices = facePolygons[p];
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
			if (normal.sqrMagnitude < Utils.Geometry.NormalEpsilon)
			{
				return false;
			}

			Vector3 planeNormal = normal.normalized;
			if (!Utils.Geometry.TryBuildBasis(planeNormal, out Vector3 tangent, out Vector3 bitangent))
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

		private static void BuildMeshesFromPolygons(List<List<Vector3>> polygons, List<Mesh> meshes, string namePrefix)
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

		private static void AddMesh(List<Vector3> vertices, List<int> triangles, List<Mesh> meshes, ref int meshIndex, string namePrefix)
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

		private static Mesh CombineMeshes(List<Mesh> meshes, string name)
		{
			if (meshes == null || meshes.Count == 0)
			{
				return new Mesh { name = name };
			}

			if (meshes.Count == 1 && meshes[0] != null)
			{
				meshes[0].name = name;
				return meshes[0];
			}

			var combine = new List<CombineInstance>(meshes.Count);
			for (int i = 0; i < meshes.Count; i++)
			{
				Mesh mesh = meshes[i];
				if (mesh == null)
				{
					continue;
				}

				combine.Add(new CombineInstance
				{
					mesh = mesh,
					transform = Matrix4x4.identity
				});
			}

			var combined = new Mesh { name = name };
			if (combine.Count > 0)
			{
				combined.CombineMeshes(combine.ToArray(), true, false);
				combined.RecalculateNormals();
				combined.RecalculateBounds();
			}

			return combined;
		}

		private static bool TryGetCell(Voxel.Cell[,,] cellPack, int x, int y, int z, out Voxel.Cell cell)
		{
			cell = null;
			if (cellPack == null ||
				x < 0 || x >= cellPack.GetLength(0) ||
				y < 0 || y >= cellPack.GetLength(1) ||
				z < 0 || z >= cellPack.GetLength(2))
			{
				return false;
			}

			cell = cellPack[x, y, z];
			return true;
		}

		private static bool TryGetDefinition(Voxel.Cell cell, out Voxel.Definition definition)
		{
			definition = null;
			if (cell == null)
			{
				return false;
			}

			return Voxel.Registry.TryGet(cell.Id, out definition);
		}

		private static Face TransformFaceCached(Face face, Erelia.Voxel.Orientation orientation, Erelia.Voxel.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (Mesherutils.FaceByOrientationCache.TryGetValue(face, orientation, flipOrientation, out Face output))
			{
				return output;
			}

			return face;
		}

		private static bool IsFaceOccluded(Face face, Face occluder)
		{
			if (face == null || occluder == null)
			{
				return false;
			}

			if (Mesherutils.FaceVsFaceOcclusionCache.TryGetValue(face, occluder, out bool isOccluded))
			{
				return isOccluded;
			}

			return face.IsOccludedBy(occluder);
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


