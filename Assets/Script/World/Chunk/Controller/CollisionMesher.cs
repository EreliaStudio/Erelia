using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World.Chunk.Controller
{
	[Serializable]
	public abstract class CollisionMesher : World.Chunk.Core.Mesher
	{
		private const int MaxVerticesPerMesh = 65000;
		private const float MergeEpsilon = 0.001f;

		protected abstract bool IsAcceptableDefinition(Voxel.Model.Definition definition);

		protected virtual string MeshName => "CollisionMesh";

		public List<Mesh> BuildMeshes(World.Chunk.Model.Cell[,,] cells)
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

			BuildMeshesFromPolygons(polygons, meshes);
			return meshes;
		}

		private void AddVoxel(
			World.Chunk.Model.Cell[,,] cells,
			int x,
			int y,
			int z,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			if (!TryGetCell(cells, x, y, z, out World.Chunk.Model.Cell cell))
			{
				return;
			}

			if (cell.Id == Voxel.Service.AirID)
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

			if (shape.Collision == Voxel.View.Shape.CollisionMode.CubeEnvelope)
			{
				for (int i = 0; i < Voxel.View.Shape.AxisPlanes.Length; i++)
				{
					Voxel.View.Shape.AxisPlane plane = Voxel.View.Shape.AxisPlanes[i];
					TryAddCubeEnvelopeFace(cells, position, x, y, z, plane, rectGroups, polygons);
				}

				return;
			}

			bool anyOuterVisible = false;
			for (int i = 0; i < Voxel.View.Shape.AxisPlanes.Length; i++)
			{
				Voxel.View.Shape.AxisPlane plane = Voxel.View.Shape.AxisPlanes[i];
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
				IReadOnlyList<Voxel.Model.Face> innerFaces = shape.InnerFaces;
				if (innerFaces == null)
				{
					return;
				}

				for (int i = 0; i < innerFaces.Count; i++)
				{
					Voxel.Model.Face face = TransformFaceCached(innerFaces[i], orientation, flipOrientation);
					AddFacePolygons(face, position, rectGroups, polygons);
				}
			}
		}

		private void TryAddCubeEnvelopeFace(
			World.Chunk.Model.Cell[,,] cells,
			Vector3 position,
			int x,
			int y,
			int z,
			Voxel.View.Shape.AxisPlane plane,
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

			Voxel.Model.Face fullFace = Geometry.GetFullOuterFace(plane);
			AddFacePolygons(fullFace, position, rectGroups, polygons);
		}

		private void TryAddOuterFace(
			World.Chunk.Model.Cell[,,] cells,
			Voxel.View.Shape shape,
			Voxel.Model.Orientation orientation,
			Voxel.Model.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Voxel.View.Shape.AxisPlane plane,
			ref bool anyOuterVisible,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			Vector3Int offset = Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			Voxel.View.Shape.AxisPlane localPlane = Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			IReadOnlyDictionary<Voxel.View.Shape.AxisPlane, Voxel.Model.Face> outerShellFaces = shape.OuterShellFaces;
			if (outerShellFaces == null || !outerShellFaces.TryGetValue(localPlane, out Voxel.Model.Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullFaceOccludedByNeighbor(cells, neighborX, neighborY, neighborZ, plane))
				{
					anyOuterVisible = true;
				}
				return;
			}

			Voxel.Model.Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
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
			Voxel.Model.Face face,
			World.Chunk.Model.Cell[,,] cells,
			int neighborX,
			int neighborY,
			int neighborZ,
			Voxel.View.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFace(cells, neighborX, neighborY, neighborZ, plane, out Voxel.Model.Face neighborFace))
			{
				return false;
			}

			return neighborFace != null && face.IsOccludedBy(neighborFace);
		}

		private bool IsFullFaceOccludedByNeighbor(
			World.Chunk.Model.Cell[,,] cells,
			int neighborX,
			int neighborY,
			int neighborZ,
			Voxel.View.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFace(cells, neighborX, neighborY, neighborZ, plane, out Voxel.Model.Face neighborFace))
			{
				return false;
			}

			if (neighborFace == null)
			{
				return false;
			}

			if (!Geometry.IsFaceCoplanarWithPlane(neighborFace, plane))
			{
				return false;
			}

			Voxel.Model.Face fullFace = Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(neighborFace);
		}

		private bool TryGetOccludingNeighborFace(
			World.Chunk.Model.Cell[,,] cells,
			int neighborX,
			int neighborY,
			int neighborZ,
			Voxel.View.Shape.AxisPlane plane,
			out Voxel.Model.Face neighborFace)
		{
			neighborFace = null;

			if (!TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Model.Cell neighborCell))
			{
				return false;
			}

			if (neighborCell == null || neighborCell.Id == Voxel.Service.AirID)
			{
				return false;
			}

			if (!TryGetDefinition(neighborCell, out Voxel.Model.Definition neighborDefinition))
			{
				return false;
			}

			if (!IsAcceptableDefinition(neighborDefinition))
			{
				return false;
			}

			Voxel.View.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			Voxel.View.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
			if (neighborShape.Collision == Voxel.View.Shape.CollisionMode.CubeEnvelope)
			{
				neighborFace = Geometry.GetFullOuterFace(oppositePlane);
				return neighborFace != null;
			}

			Voxel.Model.Orientation neighborOrientation = neighborCell.Orientation;
			Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Voxel.View.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			IReadOnlyDictionary<Voxel.View.Shape.AxisPlane, Voxel.Model.Face> neighborOuterFaces = neighborShape.OuterShellFaces;
			if (neighborOuterFaces == null || !neighborOuterFaces.TryGetValue(neighborLocalPlane, out Voxel.Model.Face otherFace))
			{
				return false;
			}

			neighborFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			return neighborFace != null;
		}

		private void AddFacePolygons(
			Voxel.Model.Face face,
			Vector3 positionOffset,
			Dictionary<RectKey, List<Rect2D>> rectGroups,
			List<List<Vector3>> polygons)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Voxel.Model.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Voxel.Model.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				var worldVerts = new List<Vector3>(faceVertices.Count);
				for (int i = 0; i < faceVertices.Count; i++)
				{
					worldVerts.Add(positionOffset + faceVertices[i].Position);
				}

				if (TryExtractAxisAlignedRect(worldVerts, out Rect2D rect))
				{
					var key = new RectKey(rect.Plane, rect.PlaneCoord);
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

		private bool TryExtractAxisAlignedRect(List<Vector3> worldVerts, out Rect2D rect)
		{
			rect = default;
			if (worldVerts == null || worldVerts.Count != 4)
			{
				return false;
			}

			Vector3 normal = Vector3.Cross(worldVerts[1] - worldVerts[0], worldVerts[2] - worldVerts[0]);
			if (!Geometry.TryFromNormal(normal, out Voxel.View.Shape.AxisPlane plane))
			{
				return false;
			}

			int axis = PlaneToAxis(plane);
			float coord = axis == 0 ? worldVerts[0].x : axis == 1 ? worldVerts[0].y : worldVerts[0].z;

			for (int i = 1; i < worldVerts.Count; i++)
			{
				float value = axis == 0 ? worldVerts[i].x : axis == 1 ? worldVerts[i].y : worldVerts[i].z;
				if (Mathf.Abs(value - coord) > MergeEpsilon)
				{
					return false;
				}
			}

			GetPlaneUV(plane, worldVerts[0], out float u0, out float v0);
			float minU = u0;
			float maxU = u0;
			float minV = v0;
			float maxV = v0;

			for (int i = 1; i < worldVerts.Count; i++)
			{
				GetPlaneUV(plane, worldVerts[i], out float u, out float v);
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
				GetPlaneUV(plane, worldVerts[i], out float u, out float v);
				if (!Approximately(u, minU) && !Approximately(u, maxU))
				{
					return false;
				}

				if (!Approximately(v, minV) && !Approximately(v, maxV))
				{
					return false;
				}
			}

			rect = new Rect2D(plane, coord, minU, maxU, minV, maxV);
			return true;
		}

		private static int PlaneToAxis(Voxel.View.Shape.AxisPlane plane)
		{
			switch (plane)
			{
				case Voxel.View.Shape.AxisPlane.PosX:
				case Voxel.View.Shape.AxisPlane.NegX:
					return 0;
				case Voxel.View.Shape.AxisPlane.PosY:
				case Voxel.View.Shape.AxisPlane.NegY:
					return 1;
				case Voxel.View.Shape.AxisPlane.PosZ:
				case Voxel.View.Shape.AxisPlane.NegZ:
					return 2;
				default:
					return 0;
			}
		}

		private static void GetPlaneUV(Voxel.View.Shape.AxisPlane plane, Vector3 position, out float u, out float v)
		{
			switch (plane)
			{
				case Voxel.View.Shape.AxisPlane.PosX:
				case Voxel.View.Shape.AxisPlane.NegX:
					u = position.z;
					v = position.y;
					break;
				case Voxel.View.Shape.AxisPlane.PosY:
				case Voxel.View.Shape.AxisPlane.NegY:
					u = position.x;
					v = position.z;
					break;
				case Voxel.View.Shape.AxisPlane.PosZ:
				case Voxel.View.Shape.AxisPlane.NegZ:
					u = position.x;
					v = position.y;
					break;
				default:
					u = 0f;
					v = 0f;
					break;
			}
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
			if (a.Plane != b.Plane || !Approximately(a.PlaneCoord, b.PlaneCoord))
			{
				return false;
			}

			bool sameV = Approximately(a.MinV, b.MinV) && Approximately(a.MaxV, b.MaxV);
			bool sameU = Approximately(a.MinU, b.MinU) && Approximately(a.MaxU, b.MaxU);

			if (sameV && (Approximately(a.MaxU, b.MinU) || Approximately(b.MaxU, a.MinU)))
			{
				float minU = Mathf.Min(a.MinU, b.MinU);
				float maxU = Mathf.Max(a.MaxU, b.MaxU);
				merged = new Rect2D(a.Plane, a.PlaneCoord, minU, maxU, a.MinV, a.MaxV);
				return true;
			}

			if (sameU && (Approximately(a.MaxV, b.MinV) || Approximately(b.MaxV, a.MinV)))
			{
				float minV = Mathf.Min(a.MinV, b.MinV);
				float maxV = Mathf.Max(a.MaxV, b.MaxV);
				merged = new Rect2D(a.Plane, a.PlaneCoord, a.MinU, a.MaxU, minV, maxV);
				return true;
			}

			return false;
		}

		private static List<Vector3> RectToPolygon(Rect2D rect)
		{
			float x = rect.PlaneCoord;
			float y = rect.PlaneCoord;
			float z = rect.PlaneCoord;

			switch (rect.Plane)
			{
				case Voxel.View.Shape.AxisPlane.PosX:
					return new List<Vector3>
					{
						new Vector3(x, rect.MinV, rect.MinU),
						new Vector3(x, rect.MinV, rect.MaxU),
						new Vector3(x, rect.MaxV, rect.MaxU),
						new Vector3(x, rect.MaxV, rect.MinU)
					};
				case Voxel.View.Shape.AxisPlane.NegX:
					return new List<Vector3>
					{
						new Vector3(x, rect.MinV, rect.MinU),
						new Vector3(x, rect.MaxV, rect.MinU),
						new Vector3(x, rect.MaxV, rect.MaxU),
						new Vector3(x, rect.MinV, rect.MaxU)
					};
				case Voxel.View.Shape.AxisPlane.PosY:
					return new List<Vector3>
					{
						new Vector3(rect.MinU, y, rect.MinV),
						new Vector3(rect.MaxU, y, rect.MinV),
						new Vector3(rect.MaxU, y, rect.MaxV),
						new Vector3(rect.MinU, y, rect.MaxV)
					};
				case Voxel.View.Shape.AxisPlane.NegY:
					return new List<Vector3>
					{
						new Vector3(rect.MinU, y, rect.MinV),
						new Vector3(rect.MinU, y, rect.MaxV),
						new Vector3(rect.MaxU, y, rect.MaxV),
						new Vector3(rect.MaxU, y, rect.MinV)
					};
				case Voxel.View.Shape.AxisPlane.PosZ:
					return new List<Vector3>
					{
						new Vector3(rect.MinU, rect.MinV, z),
						new Vector3(rect.MinU, rect.MaxV, z),
						new Vector3(rect.MaxU, rect.MaxV, z),
						new Vector3(rect.MaxU, rect.MinV, z)
					};
				case Voxel.View.Shape.AxisPlane.NegZ:
					return new List<Vector3>
					{
						new Vector3(rect.MinU, rect.MinV, z),
						new Vector3(rect.MaxU, rect.MinV, z),
						new Vector3(rect.MaxU, rect.MaxV, z),
						new Vector3(rect.MinU, rect.MaxV, z)
					};
				default:
					return new List<Vector3>();
			}
		}

		private void BuildMeshesFromPolygons(List<List<Vector3>> polygons, List<Mesh> meshes)
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
					AddMesh(vertices, triangles, meshes, ref meshIndex);
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
				AddMesh(vertices, triangles, meshes, ref meshIndex);
			}
		}

		private void AddMesh(List<Vector3> vertices, List<int> triangles, List<Mesh> meshes, ref int meshIndex)
		{
			var mesh = new Mesh
			{
				name = meshIndex == 0 ? MeshName : $"{MeshName}_{meshIndex}"
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

		private readonly struct RectKey : IEquatable<RectKey>
		{
			public readonly Voxel.View.Shape.AxisPlane Plane;
			public readonly int CoordKey;

			public RectKey(Voxel.View.Shape.AxisPlane plane, float coord)
			{
				Plane = plane;
				CoordKey = Mathf.RoundToInt(coord / MergeEpsilon);
			}

			public bool Equals(RectKey other)
			{
				return Plane == other.Plane && CoordKey == other.CoordKey;
			}

			public override bool Equals(object obj)
			{
				return obj is RectKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((int)Plane * 397) ^ CoordKey;
				}
			}
		}

		private readonly struct Rect2D
		{
			public readonly Voxel.View.Shape.AxisPlane Plane;
			public readonly float PlaneCoord;
			public readonly float MinU;
			public readonly float MaxU;
			public readonly float MinV;
			public readonly float MaxV;

			public Rect2D(Voxel.View.Shape.AxisPlane plane, float planeCoord, float minU, float maxU, float minV, float maxV)
			{
				Plane = plane;
				PlaneCoord = planeCoord;
				MinU = minU;
				MaxU = maxU;
				MinV = minV;
				MaxV = maxV;
			}
		}
	}
}
