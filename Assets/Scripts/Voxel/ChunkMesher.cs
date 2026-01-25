using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkMesher
{
    [HideInInspector] private VoxelRegistry registry;
    public void SetRegistry(VoxelRegistry value)
    {
        registry = value;
    }

    public Mesh BuildMesh(Chunk chunk)
    {
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        for (int x = 0; x < Chunk.SizeX; x++)
        {
            for (int y = 0; y < Chunk.SizeY; y++)
            {
                for (int z = 0; z < Chunk.SizeZ; z++)
                {
                    AddVoxel(chunk, x, y, z, vertices, triangles, uvs);
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        return mesh;
    }

    private void AddVoxel(Chunk chunk, int x, int y, int z, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        if (!TryGetVoxelDefinition(chunk, x, y, z, out Voxel voxel))
        {
            return;
        }

        Vector3 position = new Vector3(x, y, z);
        bool anyOuterVisible = false;

        TryAddOuterFace(chunk, voxel, position, x, y, z, OuterShellPlane.PosX, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, position, x, y, z, OuterShellPlane.NegX, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, position, x, y, z, OuterShellPlane.PosY, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, position, x, y, z, OuterShellPlane.NegY, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, position, x, y, z, OuterShellPlane.PosZ, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, position, x, y, z, OuterShellPlane.NegZ, vertices, triangles, uvs, ref anyOuterVisible);

        if (anyOuterVisible)
        {
            IReadOnlyList<VoxelFace> innerFaces = voxel.InnerFaces;
            for (int i = 0; i < innerFaces.Count; i++)
            {
                AddFace(innerFaces[i], position, vertices, triangles, uvs);
            }
        }
    }

    private void TryAddOuterFace(
        Chunk chunk,
        Voxel voxel,
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
        bool hasNeighbor = TryGetVoxelDefinition(chunk, x + offset.x, y + offset.y, z + offset.z, out Voxel neighbor);

        if (!voxel.OuterShellFaces.TryGetValue(plane, out VoxelFace face))
        {
            return;
        }

        if (face == null || face.Vertices.Count < 3)
        {
            return;
        }

        if (hasNeighbor)
        {
            OuterShellPlane oppositePlane = OuterShellPlaneUtil.GetOppositePlane(plane);
            if (neighbor.OuterShellFaces.TryGetValue(oppositePlane, out VoxelFace otherFace)
                && face.IsOccludedBy(otherFace))
            {
                return;
            }
        }

        AddFace(face, position, vertices, triangles, uvs);
        anyOuterVisible = true;
    }

    private bool TryGetVoxelDefinition(Chunk chunk, int x, int y, int z, out Voxel voxel)
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

    private void AddFace(
        VoxelFace face,
        Vector3 offset,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs)
    {
        if (face == null || face.Vertices.Count < 3)
        {
            return;
        }

        int start = vertices.Count;
        List<FaceVertex> faceVertices = face.Vertices;
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
