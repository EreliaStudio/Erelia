using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkBushTriggerMeshBuilder : ChunkMesher
{
	public List<Mesh> BuildBushTriggerMeshes(Chunk chunk)
	{
		var meshes = new List<Mesh>();
		if (chunk == null)
		{
			return meshes;
		}

		bool[,,] visited = new bool[Chunk.SizeX, Chunk.SizeY, Chunk.SizeZ];
		for (int x = 0; x < Chunk.SizeX; x++)
		{
			for (int y = 0; y < Chunk.SizeY; y++)
			{
				for (int z = 0; z < Chunk.SizeZ; z++)
				{
					if (visited[x, y, z] || !IsBushAt(chunk, x, y, z))
					{
						continue;
					}

					List<Vector3Int> island = FloodFillBush(chunk, x, y, z, visited);
					Mesh islandMesh = BuildIslandMesh(chunk, island);
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

	private List<Vector3Int> FloodFillBush(Chunk chunk, int startX, int startY, int startZ, bool[,,] visited)
	{
		var island = new List<Vector3Int>();
		var queue = new Queue<Vector3Int>();
		queue.Enqueue(new Vector3Int(startX, startY, startZ));
		visited[startX, startY, startZ] = true;

		while (queue.Count > 0)
		{
			Vector3Int current = queue.Dequeue();
			island.Add(current);

			TryEnqueueBush(chunk, current.x + 1, current.y, current.z, visited, queue);
			TryEnqueueBush(chunk, current.x - 1, current.y, current.z, visited, queue);
			TryEnqueueBush(chunk, current.x, current.y + 1, current.z, visited, queue);
			TryEnqueueBush(chunk, current.x, current.y - 1, current.z, visited, queue);
			TryEnqueueBush(chunk, current.x, current.y, current.z + 1, visited, queue);
			TryEnqueueBush(chunk, current.x, current.y, current.z - 1, visited, queue);
		}

		return island;
	}

	private void TryEnqueueBush(Chunk chunk, int x, int y, int z, bool[,,] visited, Queue<Vector3Int> queue)
	{
		if (x < 0 || x >= Chunk.SizeX || y < 0 || y >= Chunk.SizeY || z < 0 || z >= Chunk.SizeZ)
		{
			return;
		}

		if (visited[x, y, z] || !IsBushAt(chunk, x, y, z))
		{
			return;
		}

		visited[x, y, z] = true;
		queue.Enqueue(new Vector3Int(x, y, z));
	}

	private Mesh BuildIslandMesh(Chunk chunk, List<Vector3Int> island)
	{
		var mesh = new Mesh();
		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var uvs = new List<Vector2>();

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
		mesh.RecalculateNormals();
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
}
