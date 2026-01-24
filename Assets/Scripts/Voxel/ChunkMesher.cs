using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkMesher
{
    [HideInInspector] private VoxelDataRegistry registry;
    public Vector2Int AtlasSize = new Vector2Int(1, 1);

    public void SetRegistry(VoxelDataRegistry value)
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
                    AddCube(chunk, x, y, z, vertices, triangles, uvs);
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        return mesh;
    }

    private void AddCube(Chunk chunk, int x, int y, int z, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        if (!IsSolid(chunk, x, y, z))
        {
            return;
        }

        Vector3 position = new Vector3(x, y, z);
        Vector2Int tileAnchor = GetTileAnchor(chunk.Voxels[x, y, z].DataId);
        Vector3 p000 = position + new Vector3(0f, 0f, 0f);
        Vector3 p001 = position + new Vector3(0f, 0f, 1f);
        Vector3 p010 = position + new Vector3(0f, 1f, 0f);
        Vector3 p011 = position + new Vector3(0f, 1f, 1f);
        Vector3 p100 = position + new Vector3(1f, 0f, 0f);
        Vector3 p101 = position + new Vector3(1f, 0f, 1f);
        Vector3 p110 = position + new Vector3(1f, 1f, 0f);
        Vector3 p111 = position + new Vector3(1f, 1f, 1f);

        if (ShouldRenderFace(chunk, x, y, z, 1, 0, 0)) AddFace(p100, p101, p111, p110, tileAnchor, vertices, triangles, uvs); // +X
        if (ShouldRenderFace(chunk, x, y, z, -1, 0, 0)) AddFace(p000, p010, p011, p001, tileAnchor, vertices, triangles, uvs); // -X
        if (ShouldRenderFace(chunk, x, y, z, 0, 1, 0)) AddFace(p010, p110, p111, p011, tileAnchor, vertices, triangles, uvs); // +Y
        if (ShouldRenderFace(chunk, x, y, z, 0, -1, 0)) AddFace(p000, p001, p101, p100, tileAnchor, vertices, triangles, uvs); // -Y
        if (ShouldRenderFace(chunk, x, y, z, 0, 0, 1)) AddFace(p001, p011, p111, p101, tileAnchor, vertices, triangles, uvs); // +Z
        if (ShouldRenderFace(chunk, x, y, z, 0, 0, -1)) AddFace(p000, p100, p110, p010, tileAnchor, vertices, triangles, uvs); // -Z
    }
 
    private bool ShouldRenderFace(Chunk chunk, int x, int y, int z, int dx, int dy, int dz)
    {
        int nx = x + dx;
        int ny = y + dy;
        int nz = z + dz;

        if (nx < 0 || nx >= Chunk.SizeX || ny < 0 || ny >= Chunk.SizeY || nz < 0 || nz >= Chunk.SizeZ)
        {
            return true;
        }

        return !IsSolid(chunk, nx, ny, nz);
    }

    private bool IsSolid(Chunk chunk, int x, int y, int z)
    {
        return chunk.Voxels[x, y, z].DataId != registry.AirId;
    }

    private void AddFace(
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d,
        Vector2Int tileAnchor,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs)
    {
        int start = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);

        // Wind clockwise so normals face outward with Unity's default backface culling.
        triangles.Add(start);
        triangles.Add(start + 2);
        triangles.Add(start + 1);
        triangles.Add(start);
        triangles.Add(start + 3);
        triangles.Add(start + 2);

        Vector2 tileSize = GetTileSize();
        Vector2 uv0 = new Vector2(tileAnchor.x * tileSize.x, tileAnchor.y * tileSize.y);
        Vector2 uv1 = new Vector2(uv0.x + tileSize.x, uv0.y);
        Vector2 uv2 = new Vector2(uv0.x + tileSize.x, uv0.y + tileSize.y);
        Vector2 uv3 = new Vector2(uv0.x, uv0.y + tileSize.y);

        uvs.Add(uv0);
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }

    private Vector2Int GetTileAnchor(int dataId)
    {
        if (registry.TryGetData(dataId, out VoxelData voxelData) && voxelData != null)
        {
            return voxelData.tileAnchor;
        }

        return Vector2Int.zero;
    }

    private Vector2 GetTileSize()
    {
        if (AtlasSize.x <= 0 || AtlasSize.y <= 0)
        {
            return Vector2.one;
        }

        return new Vector2(1f / AtlasSize.x, 1f / AtlasSize.y);
    }
}
