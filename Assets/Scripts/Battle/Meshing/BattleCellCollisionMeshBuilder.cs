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

                    meshes.Add(BuildCellBoxMesh(x, y, z));
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

    private Mesh BuildCellBoxMesh(int x, int y, int z)
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
            new Vector3(left, minY, bottom),
            new Vector3(right, minY, bottom),
            new Vector3(right, minY, top),
            new Vector3(left, minY, top),
            new Vector3(left, maxY, bottom),
            new Vector3(right, maxY, bottom),
            new Vector3(right, maxY, top),
            new Vector3(left, maxY, top)
        };

        int[] boxTriangles =
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4,
            1, 2, 6, 1, 6, 5,
            2, 3, 7, 2, 7, 6,
            3, 0, 4, 3, 4, 7
        };

        Mesh mesh = new Mesh
        {
            name = $"BattleCellCollisionBox {x},{y},{z}"
        };
        mesh.SetVertices(boxVertices);
        mesh.SetTriangles(boxTriangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
