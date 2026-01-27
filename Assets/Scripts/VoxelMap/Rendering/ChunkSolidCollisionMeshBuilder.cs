using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkSolidCollisionMeshBuilder : ChunkMesher
{
	public List<Mesh> BuildSolidMeshes(Chunk chunk)
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
					if (visited[x, y, z] || !IsSolidAt(chunk, x, y, z))
					{
						continue;
					}

					List<Vector3Int> island = FloodFillSolid(chunk, x, y, z, visited);
					Mesh islandMesh = BuildIslandMesh(chunk, island);
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

	private List<Vector3Int> FloodFillSolid(Chunk chunk, int startX, int startY, int startZ, bool[,,] visited)
	{
		var island = new List<Vector3Int>();
		var queue = new Queue<Vector3Int>();
		queue.Enqueue(new Vector3Int(startX, startY, startZ));
		visited[startX, startY, startZ] = true;

		while (queue.Count > 0)
		{
			Vector3Int current = queue.Dequeue();
			island.Add(current);

			TryEnqueueSolid(chunk, current.x + 1, current.y, current.z, visited, queue);
			TryEnqueueSolid(chunk, current.x - 1, current.y, current.z, visited, queue);
			TryEnqueueSolid(chunk, current.x, current.y + 1, current.z, visited, queue);
			TryEnqueueSolid(chunk, current.x, current.y - 1, current.z, visited, queue);
			TryEnqueueSolid(chunk, current.x, current.y, current.z + 1, visited, queue);
			TryEnqueueSolid(chunk, current.x, current.y, current.z - 1, visited, queue);
		}

		return island;
	}

	private void TryEnqueueSolid(Chunk chunk, int x, int y, int z, bool[,,] visited, Queue<Vector3Int> queue)
	{
		if (x < 0 || x >= Chunk.SizeX || y < 0 || y >= Chunk.SizeY || z < 0 || z >= Chunk.SizeZ)
		{
			return;
		}

		if (visited[x, y, z] || !IsSolidAt(chunk, x, y, z))
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
			AddVoxelCollision(chunk, cell.x, cell.y, cell.z, vertices, triangles, uvs);
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
				AddFace(TransformFace(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
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
		VoxelFace rotatedFace = TransformFace(face, orientation, flipOrientation);
		if (hasNeighbor)
		{
			OuterShellPlane oppositePlane = OuterShellPlaneUtil.GetOppositePlane(plane);
			Orientation neighborOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].Orientation;
			FlipOrientation neighborFlipOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].FlipOrientation;
			OuterShellPlane neighborLocalPlane = MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (neighbor.OuterShellFaces.TryGetValue(neighborLocalPlane, out VoxelFace otherFace))
			{
				VoxelFace rotatedOtherFace = TransformFace(otherFace, neighborOrientation, neighborFlipOrientation);
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
