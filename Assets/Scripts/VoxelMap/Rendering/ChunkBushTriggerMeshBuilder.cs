using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkBushTriggerMeshBuilder : ChunkMesher
{
	[NonSerialized] private int[] visited;
	[NonSerialized] private int visitedStamp = 1;
	[NonSerialized] private readonly Queue<Vector3Int> floodQueue = new Queue<Vector3Int>();
	[NonSerialized] private readonly List<Vector3Int> islandCells = new List<Vector3Int>();
	[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
	[NonSerialized] private readonly List<int> triangles = new List<int>();
	[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();

	public List<Mesh> BuildBushTriggerMeshes(Chunk chunk)
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
					if (visited[index] == visitedStamp || !IsBushAt(chunk, x, y, z))
					{
						continue;
					}

					FloodFillBush(chunk, x, y, z, islandCells);
					Mesh islandMesh = BuildIslandMesh(chunk, islandCells);
					if (islandMesh != null && islandMesh.vertexCount > 0)
					{
						islandMesh.name = "BushCollisionMesh";
						meshes.Add(islandMesh);
					}
				}
			}
		}

		return meshes;
	}

	private void FloodFillBush(Chunk chunk, int startX, int startY, int startZ, List<Vector3Int> island)
	{
		island.Clear();
		floodQueue.Clear();
		floodQueue.Enqueue(new Vector3Int(startX, startY, startZ));
		visited[GetIndex(startX, startY, startZ)] = visitedStamp;

		while (floodQueue.Count > 0)
		{
			Vector3Int current = floodQueue.Dequeue();
			island.Add(current);

			TryEnqueueBush(chunk, current.x + 1, current.y, current.z);
			TryEnqueueBush(chunk, current.x - 1, current.y, current.z);
			TryEnqueueBush(chunk, current.x, current.y + 1, current.z);
			TryEnqueueBush(chunk, current.x, current.y - 1, current.z);
			TryEnqueueBush(chunk, current.x, current.y, current.z + 1);
			TryEnqueueBush(chunk, current.x, current.y, current.z - 1);
		}
	}

	private void TryEnqueueBush(Chunk chunk, int x, int y, int z)
	{
		if (x < 0 || x >= Chunk.SizeX || y < 0 || y >= Chunk.SizeY || z < 0 || z >= Chunk.SizeZ)
		{
			return;
		}

		int index = GetIndex(x, y, z);
		if (visited[index] == visitedStamp || !IsBushAt(chunk, x, y, z))
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
			Vector3 position = new Vector3(cell.x, cell.y, cell.z);
			TryAddBushFace(chunk, cell.x, cell.y, cell.z, OuterShellPlane.PosX, position, vertices, triangles, uvs);
			TryAddBushFace(chunk, cell.x, cell.y, cell.z, OuterShellPlane.NegX, position, vertices, triangles, uvs);
			TryAddBushFace(chunk, cell.x, cell.y, cell.z, OuterShellPlane.PosY, position, vertices, triangles, uvs);
			TryAddBushFace(chunk, cell.x, cell.y, cell.z, OuterShellPlane.NegY, position, vertices, triangles, uvs);
			TryAddBushFace(chunk, cell.x, cell.y, cell.z, OuterShellPlane.PosZ, position, vertices, triangles, uvs);
			TryAddBushFace(chunk, cell.x, cell.y, cell.z, OuterShellPlane.NegZ, position, vertices, triangles, uvs);
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

	private void TryAddBushFace(
		Chunk chunk,
		int x,
		int y,
		int z,
		OuterShellPlane plane,
		Vector3 position,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs)
	{
		Vector3Int offset = OuterShellPlaneUtil.PlaneToOffset(plane);
		int neighborX = x + offset.x;
		int neighborY = y + offset.y;
		int neighborZ = z + offset.z;
		if (IsBushAt(chunk, neighborX, neighborY, neighborZ))
		{
			return;
		}

		VoxelFace fullFace = GetFullOuterFace(plane);
		if (fullFace == null || !fullFace.HasRenderablePolygons)
		{
			return;
		}

		AddFace(fullFace, position, vertices, triangles, uvs);
	}

	private bool IsBushAt(Chunk chunk, int x, int y, int z)
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

		return voxel.Collision == VoxelCollision.Bush;
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
}
