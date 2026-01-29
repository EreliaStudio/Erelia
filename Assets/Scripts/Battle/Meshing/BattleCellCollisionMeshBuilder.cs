using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleCellCollisionMeshBuilder : BattleCellMesher
{
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
}
