using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChunkMesher
{
	[HideInInspector] protected VoxelRegistry registry;
	private static readonly VoxelFace FullPosXFace = GeometryUtils.CreateRectangle(
		new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero });
	private static readonly VoxelFace FullNegXFace = GeometryUtils.CreateRectangle(
		new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero });
	private static readonly VoxelFace FullPosYFace = GeometryUtils.CreateRectangle(
		new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero });
	private static readonly VoxelFace FullNegYFace = GeometryUtils.CreateRectangle(
		new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero });
	private static readonly VoxelFace FullPosZFace = GeometryUtils.CreateRectangle(
		new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero });
	private static readonly VoxelFace FullNegZFace = GeometryUtils.CreateRectangle(
		new GeometryUtils.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero },
		new GeometryUtils.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero });

	public void SetRegistry(VoxelRegistry value)
	{
		registry = value;
	}

	protected bool TryGetVoxelDefinition(Chunk chunk, int x, int y, int z, out Voxel voxel)
	{
		voxel = null;
		if (registry == null)
		{
			return false;
		}

		if (x < 0 || x >= Chunk.SizeX || y < 0 || y >= Chunk.SizeY || z < 0 || z >= Chunk.SizeZ)
		{
			return false;
		}

		int id = chunk.Voxels[x, y, z].Id;
		if (id == registry.AirId)
		{
			return false;
		}

		return registry.TryGetVoxel(id, out voxel) && voxel != null;
	}

	protected static int OrientationToSteps(Orientation orientation)
	{
		switch (orientation)
		{
			case Orientation.PositiveX:
				return 0;
			case Orientation.PositiveZ:
				return 1;
			case Orientation.NegativeX:
				return 2;
			case Orientation.NegativeZ:
				return 3;
			default:
				return 0;
		}
	}

	protected static OuterShellPlane MapWorldPlaneToLocal(OuterShellPlane plane, Orientation orientation)
	{
		return RotatePlane(plane, -OrientationToSteps(orientation));
	}

	protected static OuterShellPlane MapWorldPlaneToLocal(OuterShellPlane plane, Orientation orientation, FlipOrientation flipOrientation)
	{
		OuterShellPlane rotated = RotatePlane(plane, -OrientationToSteps(orientation));
		if (flipOrientation == FlipOrientation.NegativeY)
		{
			return FlipPlaneY(rotated);
		}

		return rotated;
	}

	protected static OuterShellPlane RotatePlane(OuterShellPlane plane, int steps)
	{
		int normalized = ((steps % 4) + 4) % 4;
		if (normalized == 0)
		{
			return plane;
		}

		Vector3 normal = OuterShellPlaneUtil.PlaneToNormal(plane);
		Quaternion rotation = Quaternion.AngleAxis(-normalized * 90f, Vector3.up);
		Vector3 rotatedNormal = rotation * normal;
		if (OuterShellPlaneUtil.TryFromNormal(rotatedNormal, out OuterShellPlane rotatedPlane))
		{
			return rotatedPlane;
		}

		return plane;
	}

	protected static OuterShellPlane FlipPlaneY(OuterShellPlane plane)
	{
		switch (plane)
		{
			case OuterShellPlane.PosY:
				return OuterShellPlane.NegY;
			case OuterShellPlane.NegY:
				return OuterShellPlane.PosY;
			default:
				return plane;
		}
	}

	protected static VoxelFace TransformFace(VoxelFace face, Orientation orientation, FlipOrientation flipOrientation)
	{
		if (face == null || face.Polygons == null || face.Polygons.Count == 0)
		{
			return face;
		}

		int steps = OrientationToSteps(orientation);
		var rotated = new VoxelFace();
		Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
		List<List<FaceVertex>> sourcePolygons = face.Polygons;
		for (int p = 0; p < sourcePolygons.Count; p++)
		{
			List<FaceVertex> sourceVertices = sourcePolygons[p];
			if (sourceVertices == null || sourceVertices.Count == 0)
			{
				continue;
			}

			var rotatedPolygon = new List<FaceVertex>(sourceVertices.Count);
			for (int i = 0; i < sourceVertices.Count; i++)
			{
				FaceVertex vertex = sourceVertices[i];
				Vector3 local = vertex.Position;
				if (steps != 0)
				{
					Vector3 offset = local - pivot;
					Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
					local = rotation * offset + pivot;
				}

				if (flipOrientation == FlipOrientation.NegativeY)
				{
					local.y = 1f - local.y;
				}

				vertex.Position = local;
				rotatedPolygon.Add(vertex);
			}

			if (flipOrientation == FlipOrientation.NegativeY)
			{
				rotatedPolygon.Reverse();
			}

			rotated.Polygons.Add(rotatedPolygon);
		}

		return rotated;
	}

	protected void AddFace(
		VoxelFace face,
		Vector3 offset,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs)
	{
		if (face == null || face.Polygons == null || face.Polygons.Count == 0)
		{
			return;
		}

		List<List<FaceVertex>> facePolygons = face.Polygons;
		for (int p = 0; p < facePolygons.Count; p++)
		{
			List<FaceVertex> faceVertices = facePolygons[p];
			if (faceVertices == null || faceVertices.Count < 3)
			{
				continue;
			}

			int start = vertices.Count;
			for (int i = 0; i < faceVertices.Count; i++)
			{
				FaceVertex vertex = faceVertices[i];
				vertices.Add(offset + vertex.Position);
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

	protected bool IsFullyOccludedByNeighbor(
		Chunk chunk,
		Voxel neighbor,
		int neighborX,
		int neighborY,
		int neighborZ,
		OuterShellPlane plane,
		bool hasNeighbor)
	{
		if (!hasNeighbor)
		{
			return false;
		}

		OuterShellPlane oppositePlane = OuterShellPlaneUtil.GetOppositePlane(plane);
		Orientation neighborOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].Orientation;
		FlipOrientation neighborFlipOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].FlipOrientation;
		OuterShellPlane neighborLocalPlane = MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
		if (!neighbor.OuterShellFaces.TryGetValue(neighborLocalPlane, out VoxelFace otherFace))
		{
			return false;
		}

		VoxelFace rotatedOtherFace = TransformFace(otherFace, neighborOrientation, neighborFlipOrientation);
		if (!IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
		{
			return false;
		}

		VoxelFace fullFace = GetFullOuterFace(plane);
		return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
	}

	protected static VoxelFace GetFullOuterFace(OuterShellPlane plane)
	{
		switch (plane)
		{
			case OuterShellPlane.PosX:
				return FullPosXFace;
			case OuterShellPlane.NegX:
				return FullNegXFace;
			case OuterShellPlane.PosY:
				return FullPosYFace;
			case OuterShellPlane.NegY:
				return FullNegYFace;
			case OuterShellPlane.PosZ:
				return FullPosZFace;
			case OuterShellPlane.NegZ:
				return FullNegZFace;
			default:
				return null;
		}
	}

	protected static bool IsFaceCoplanarWithPlane(VoxelFace face, OuterShellPlane plane)
	{
		if (face == null || face.Polygons == null || face.Polygons.Count == 0)
		{
			return false;
		}

		float target = 0f;
		int axis = 0;
		switch (plane)
		{
			case OuterShellPlane.PosX:
				axis = 0;
				target = 1f;
				break;
			case OuterShellPlane.NegX:
				axis = 0;
				target = 0f;
				break;
			case OuterShellPlane.PosY:
				axis = 1;
				target = 1f;
				break;
			case OuterShellPlane.NegY:
				axis = 1;
				target = 0f;
				break;
			case OuterShellPlane.PosZ:
				axis = 2;
				target = 1f;
				break;
			case OuterShellPlane.NegZ:
				axis = 2;
				target = 0f;
				break;
		}

		const float epsilon = 0.0001f;
		List<List<FaceVertex>> polygons = face.Polygons;
		for (int p = 0; p < polygons.Count; p++)
		{
			List<FaceVertex> polygon = polygons[p];
			if (polygon == null)
			{
				continue;
			}

			for (int i = 0; i < polygon.Count; i++)
			{
				Vector3 pos = polygon[i].Position;
				float value = axis == 0 ? pos.x : axis == 1 ? pos.y : pos.z;
				if (Mathf.Abs(value - target) > epsilon)
				{
					return false;
				}
			}
		}

		return true;
	}

	protected static bool IsFullFace(VoxelFace face, OuterShellPlane plane)
	{
		if (face == null || !face.HasRenderablePolygons)
		{
			return false;
		}

		VoxelFace fullFace = GetFullOuterFace(plane);
		if (fullFace == null)
		{
			return false;
		}

		if (!IsFaceCoplanarWithPlane(face, plane))
		{
			return false;
		}

		return fullFace.IsOccludedBy(face) && face.IsOccludedBy(fullFace);
	}
}
