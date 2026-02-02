using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleCellCollisionMeshBuilder : BattleCellMesher
{
    private const float MaskThickness = 0.1f;
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector2> uvs = new List<Vector2>();

    public Mesh BuildMesh(BattleBoardData board, IReadOnlyList<BattleMaskSpriteMapping> mappings)
    {
        var mesh = new Mesh { name = "BattleCellCollisionMesh" };
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        if (board == null || mappings == null || mappings.Count == 0)
        {
            return mesh;
        }

        for (int x = 0; x < board.SizeX; x++)
        {
            for (int y = 0; y < board.SizeY; y++)
            {
                for (int z = 0; z < board.SizeZ; z++)
                {
                    BattleCell cell = board.MaskCells[x, y, z];
                    if (cell == null || cell.IsEmpty)
                    {
                        continue;
                    }

                    AddMaskQuads(cell, x, y, z, mappings);
                }
            }
        }

        mesh.SetVertices(vertices);
        FlipWindingInPlace(triangles);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();

        return mesh;
    }

    public List<Mesh> BuildMeshes(BattleBoardData board, IReadOnlyList<BattleMaskSpriteMapping> mappings)
    {
        var meshes = new List<Mesh>();
        if (board == null || mappings == null || mappings.Count == 0)
        {
            return meshes;
        }

        for (int x = 0; x < board.SizeX; x++)
        {
            for (int y = 0; y < board.SizeY; y++)
            {
                for (int z = 0; z < board.SizeZ; z++)
                {
                    BattleCell cell = board.MaskCells[x, y, z];
                    if (cell == null || cell.IsEmpty)
                    {
                        continue;
                    }

                    if (!TryGetCellUv(cell, mappings, out Vector2 uvMin, out Vector2 uvMax))
                    {
                        uvMin = Vector2.zero;
                        uvMax = Vector2.one;
                    }

                    meshes.Add(BuildCellBoxMesh(x, y, z, uvMin, uvMax));
                }
            }
        }

        return meshes;
    }

    private void AddMaskQuads(
        BattleCell cell,
        int x,
        int y,
        int z,
        IReadOnlyList<BattleMaskSpriteMapping> mappings)
    {
        Vector2 uvMin = Vector2.zero;
        Vector2 uvMax = Vector2.one;

        for (int i = 0; i < mappings.Count; i++)
        {
            BattleMaskSpriteMapping mapping = mappings[i];
            if (!cell.HasMask(mapping.Mask))
            {
                continue;
            }

            AddQuad(x, y, z, uvMin, uvMax, vertices, triangles, uvs);
        }
    }

    private Mesh BuildCellBoxMesh(int x, int y, int z, Vector2 uvMin, Vector2 uvMax)
    {
        float left = x;
        float right = x + 1f;
        float bottom = z;
        float top = z + 1f;
        float centerY = y + 0.02f;
        float minY = centerY - (MaskThickness * 0.5f);
        float maxY = centerY + (MaskThickness * 0.5f);

        Vector3[] boxVertices =
        {
            // Bottom face
            new Vector3(left, minY, bottom),
            new Vector3(right, minY, bottom),
            new Vector3(right, minY, top),
            new Vector3(left, minY, top),
            // Top face
            new Vector3(left, maxY, bottom),
            new Vector3(right, maxY, bottom),
            new Vector3(right, maxY, top),
            new Vector3(left, maxY, top),
            // Left face
            new Vector3(left, minY, bottom),
            new Vector3(left, minY, top),
            new Vector3(left, maxY, top),
            new Vector3(left, maxY, bottom),
            // Right face
            new Vector3(right, minY, bottom),
            new Vector3(right, maxY, bottom),
            new Vector3(right, maxY, top),
            new Vector3(right, minY, top),
            // Front face
            new Vector3(left, minY, bottom),
            new Vector3(left, maxY, bottom),
            new Vector3(right, maxY, bottom),
            new Vector3(right, minY, bottom),
            // Back face
            new Vector3(left, minY, top),
            new Vector3(right, minY, top),
            new Vector3(right, maxY, top),
            new Vector3(left, maxY, top)
        };

        int[] boxTriangles =
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            8, 10, 9, 8, 11, 10,
            12, 14, 13, 12, 15, 14,
            16, 18, 17, 16, 19, 18,
            20, 22, 21, 20, 23, 22
        };
        FlipWindingInPlace(boxTriangles);

        Vector2 uv00 = new Vector2(0f, 0f);
        Vector2 uv10 = new Vector2(1f, 0f);
        Vector2 uv11 = new Vector2(1f, 1f);
        Vector2 uv01 = new Vector2(0f, 1f);
        Vector2[] boxUvs =
        {
            // Bottom face (default)
            uv00, uv10, uv11, uv01,
            // Top face (use sprite UV)
            new Vector2(uvMin.x, uvMin.y),
            new Vector2(uvMax.x, uvMin.y),
            new Vector2(uvMax.x, uvMax.y),
            new Vector2(uvMin.x, uvMax.y),
            // Left face (default)
            uv00, uv10, uv11, uv01,
            // Right face (default)
            uv00, uv10, uv11, uv01,
            // Front face (default)
            uv00, uv10, uv11, uv01,
            // Back face (default)
            uv00, uv10, uv11, uv01
        };

        Mesh mesh = new Mesh
        {
            name = $"BattleCellCollisionBox {x},{y},{z}"
        };
        mesh.SetVertices(boxVertices);
        mesh.SetTriangles(boxTriangles, 0);
        mesh.SetUVs(0, boxUvs);
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void FlipWindingInPlace(List<int> indices)
    {
        if (indices == null)
        {
            return;
        }

        for (int i = 0; i + 2 < indices.Count; i += 3)
        {
            int temp = indices[i + 1];
            indices[i + 1] = indices[i + 2];
            indices[i + 2] = temp;
        }
    }

    private static void FlipWindingInPlace(int[] indices)
    {
        if (indices == null)
        {
            return;
        }

        for (int i = 0; i + 2 < indices.Length; i += 3)
        {
            int temp = indices[i + 1];
            indices[i + 1] = indices[i + 2];
            indices[i + 2] = temp;
        }
    }

    private static bool TryGetCellUv(
        BattleCell cell,
        IReadOnlyList<BattleMaskSpriteMapping> mappings,
        out Vector2 uvMin,
        out Vector2 uvMax)
    {
        uvMin = Vector2.zero;
        uvMax = Vector2.one;
        if (cell == null || mappings == null)
        {
            return false;
        }

        for (int i = 0; i < mappings.Count; i++)
        {
            BattleMaskSpriteMapping mapping = mappings[i];
            if (!cell.HasMask(mapping.Mask))
            {
                continue;
            }

            if (!TryGetSpriteUv(mapping.Sprite, out uvMin, out uvMax))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private static bool TryGetSpriteUv(Sprite sprite, out Vector2 uvMin, out Vector2 uvMax)
    {
        uvMin = Vector2.zero;
        uvMax = Vector2.one;
        if (sprite == null)
        {
            return false;
        }

        Texture2D texture = sprite.texture;
        if (texture == null)
        {
            return false;
        }

        Rect rect = sprite.textureRect;
        float uMin = rect.xMin / texture.width;
        float uMax = rect.xMax / texture.width;
        float vMin = rect.yMin / texture.height;
        float vMax = rect.yMax / texture.height;

        uvMin = new Vector2(uMin, vMin);
        uvMax = new Vector2(uMax, vMax);
        return true;
    }
}
