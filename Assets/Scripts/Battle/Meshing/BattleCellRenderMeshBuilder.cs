using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleCellRenderMeshBuilder : BattleCellMesher
{
    [SerializeField] private List<BattleMaskSpriteMapping> mappings = new List<BattleMaskSpriteMapping>();
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector2> uvs = new List<Vector2>();

    public IReadOnlyList<BattleMaskSpriteMapping> Mappings => mappings;

    public Mesh BuildMesh(BattleBoardData board)
    {
        var mesh = new Mesh { name = "BattleCellRenderMesh" };
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

                    AddMaskQuads(cell, x, y, z);
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
        int z)
    {
        for (int i = 0; i < mappings.Count; i++)
        {
            BattleMaskSpriteMapping mapping = mappings[i];
            if (!cell.HasMask(mapping.Mask))
            {
                continue;
            }

            if (!TryGetSpriteUv(mapping.Sprite, out Vector2 uvMin, out Vector2 uvMax))
            {
                continue;
            }

            AddQuad(x, y, z, uvMin, uvMax, vertices, triangles, uvs);
        }
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
