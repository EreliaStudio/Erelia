using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkSolidCollisionMeshBuilder : ChunkMesher
{
	[NonSerialized] private int[] visited;
	[NonSerialized] private int visitedStamp = 1;
	[NonSerialized] private readonly Queue<Vector3Int> floodQueue = new Queue<Vector3Int>();
	[NonSerialized] private readonly List<Vector3Int> islandCells = new List<Vector3Int>();
	[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
	[NonSerialized] private readonly List<int> triangles = new List<int>();
	[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();

	public List<Mesh> BuildSolidMeshes(Chunk chunk)
	{
		var meshes = new List<Mesh>();
		if (chunk == null)
		{
			return meshes;
		}

		BeginVisitedPass();
		for (int x = 0; x < Chunk.SizeX; x++)
		{
			for (int y = 0; y < Chunk.SizeY; y++)
			{
				for (int z = 0; z < Chunk.SizeZ; z++)
				{
					int index = GetIndex(x, y, z);
					if (visited[index] == visitedStamp || !IsSolidAt(chunk, x, y, z))
					{
						continue;
					}

					FloodFillSolid(chunk, x, y, z, islandCells);
					Mesh islandMesh = BuildIslandMesh(chunk, islandCells);
					if (islandMesh != null && islandMesh.vertexCount > 0)
					{
						islandMesh.name = "SolidCollisionMesh";
						meshes.Add(islandMesh);
					}
				}
			}
		}

		return meshes;
	}

	private void FloodFillSolid(Chunk chunk, int startX, int startY, int startZ, List<Vector3Int> island)
	{
		island.Clear();
		floodQueue.Clear();
		floodQueue.Enqueue(new Vector3Int(startX, startY, startZ));
		visited[GetIndex(startX, startY, startZ)] = visitedStamp;

		while (floodQueue.Count > 0)
		{
			Vector3Int current = floodQueue.Dequeue();
			island.Add(current);

			TryEnqueueSolid(chunk, current.x + 1, current.y, current.z);
			TryEnqueueSolid(chunk, current.x - 1, current.y, current.z);
			TryEnqueueSolid(chunk, current.x, current.y + 1, current.z);
			TryEnqueueSolid(chunk, current.x, current.y - 1, current.z);
			TryEnqueueSolid(chunk, current.x, current.y, current.z + 1);
			TryEnqueueSolid(chunk, current.x, current.y, current.z - 1);
		}
	}

	private void TryEnqueueSolid(Chunk chunk, int x, int y, int z)
	{
		if (x < 0 || x >= Chunk.SizeX || y < 0 || y >= Chunk.SizeY || z < 0 || z >= Chunk.SizeZ)
		{
			return;
		}

		int index = GetIndex(x, y, z);
		if (visited[index] == visitedStamp || !IsSolidAt(chunk, x, y, z))
		{
			return;
		}

		visited[index] = visitedStamp;
		floodQueue.Enqueue(new Vector3Int(x, y, z));
	}

	private Mesh BuildIslandMesh(Chunk chunk, List<Vector3Int> island)
	{
		var mesh = new Mesh();
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();

		for (int i = 0; i < island.Count; i++)
		{
			Vector3Int cell = island[i];
			AddVoxelCollision(chunk, cell.x, cell.y, cell.z, vertices, triangles, uvs);
		}

		if (vertices.Count == 0)
		{
			return mesh;
		}

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		return mesh;
	}

	private void AddVoxelCollision(
		Chunk chunk,
		int x,
		int y,
		int z,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs)
	{
		if (!TryGetVoxelDefinition(chunk, x, y, z, out Voxel voxel))
		{
			return;
		}

		if (voxel.Collision != VoxelCollision.Solid)
		{
			return;
		}

		Orientation orientation = chunk.Voxels[x, y, z].Orientation;
		FlipOrientation flipOrientation = chunk.Voxels[x, y, z].FlipOrientation;
		Vector3 position = new Vector3(x, y, z);
		bool anyOuterVisible = false;

		TryAddOuterFaceCollision(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosX, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFaceCollision(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegX, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFaceCollision(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosY, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFaceCollision(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegY, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFaceCollision(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosZ, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFaceCollision(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegZ, vertices, triangles, uvs, ref anyOuterVisible);

		if (anyOuterVisible)
		{
			IReadOnlyList<VoxelFace> innerFaces = voxel.InnerFaces;
			for (int i = 0; i < innerFaces.Count; i++)
			{
				AddFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
			}
		}
	}

	private void TryAddOuterFaceCollision(
		Chunk chunk,
		Voxel voxel,
		Orientation orientation,
		FlipOrientation flipOrientation,
		Vector3 position,
		int x,
		int y,
		int z,
		OuterShellPlane plane,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs,
		ref bool anyOuterVisible)
	{
		Vector3Int offset = OuterShellPlaneUtil.PlaneToOffset(plane);
		int neighborX = x + offset.x;
		int neighborY = y + offset.y;
		int neighborZ = z + offset.z;
		bool hasNeighbor = TryGetVoxelDefinition(chunk, neighborX, neighborY, neighborZ, out Voxel neighbor);
		if (hasNeighbor && neighbor.Collision != VoxelCollision.Solid)
		{
			hasNeighbor = false;
		}

		OuterShellPlane localPlane = MapWorldPlaneToLocal(plane, orientation, flipOrientation);

		if (!voxel.OuterShellFaces.TryGetValue(localPlane, out VoxelFace face))
		{
			if (!IsFullyOccludedByNeighbor(chunk, neighbor, neighborX, neighborY, neighborZ, plane, hasNeighbor))
			{
				anyOuterVisible = true;
			}
			return;
		}

		if (face == null || !face.HasRenderablePolygons)
		{
			if (!IsFullyOccludedByNeighbor(chunk, neighbor, neighborX, neighborY, neighborZ, plane, hasNeighbor))
			{
				anyOuterVisible = true;
			}
			return;
		}

		bool isOccluded = false;
		VoxelFace rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
		if (hasNeighbor)
		{
			OuterShellPlane oppositePlane = OuterShellPlaneUtil.GetOppositePlane(plane);
			Orientation neighborOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].Orientation;
			FlipOrientation neighborFlipOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].FlipOrientation;
			OuterShellPlane neighborLocalPlane = MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (neighbor.OuterShellFaces.TryGetValue(neighborLocalPlane, out VoxelFace otherFace))
			{
				VoxelFace rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
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

		AddFace(rotatedFace, position, vertices, triangles, uvs);
		anyOuterVisible = true;
	}

	private void BeginVisitedPass()
	{
		int size = Chunk.SizeX * Chunk.SizeY * Chunk.SizeZ;
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

	private static int GetIndex(int x, int y, int z)
	{
		return (x * Chunk.SizeY + y) * Chunk.SizeZ + z;
	}

	private bool IsSolidAt(Chunk chunk, int x, int y, int z)
	{
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

		if (!registry.TryGetVoxel(id, out Voxel voxel) || voxel == null)
		{
			return false;
		}

		return voxel.Collision == VoxelCollision.Solid;
	}
}
