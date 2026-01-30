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
    private const float MaskHeightOffset = 0.02f;

    public IReadOnlyList<BattleMaskSpriteMapping> Mappings => mappings;

    public Mesh BuildMesh(BattleBoardData board, VoxelRegistry registry)
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

                    AddMaskQuads(cell, board, registry, x, y, z);
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
        BattleBoardData board,
        VoxelRegistry registry,
        int x,
        int y,
        int z)
    {
        float height = ResolveMaskHeight(board, registry, x, y, z);
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

            AddQuadAtHeight(x, z, height, uvMin, uvMax, vertices, triangles, uvs);
        }
    }

    private static float ResolveMaskHeight(BattleBoardData board, VoxelRegistry registry, int x, int y, int z)
    {
        if (board == null || registry == null)
        {
            return y + MaskHeightOffset;
        }

        int voxelY = y - 1;
        if (voxelY < 0 || voxelY >= board.SizeY)
        {
            return y + MaskHeightOffset;
        }

        if (board.Voxels == null)
        {
            return y + MaskHeightOffset;
        }

        VoxelCell cell = board.Voxels[x, voxelY, z];
        if (cell.Id == registry.AirId)
        {
            return y + MaskHeightOffset;
        }

        if (!registry.TryGetVoxel(cell.Id, out Voxel voxel) || voxel == null)
        {
            return y + MaskHeightOffset;
        }

        if (!TryGetMaxVoxelLocalY(voxel, cell.Orientation, cell.FlipOrientation, out float maxLocalY))
        {
            return y + MaskHeightOffset;
        }

        return voxelY + maxLocalY + MaskHeightOffset;
    }

    private static bool TryGetMaxVoxelLocalY(
        Voxel voxel,
        Orientation orientation,
        FlipOrientation flipOrientation,
        out float maxLocalY)
    {
        maxLocalY = 0f;
        if (voxel == null)
        {
            return false;
        }

        bool found = false;
        float maxY = float.MinValue;

        IReadOnlyList<VoxelFace> innerFaces = voxel.InnerFaces;
        if (innerFaces != null)
        {
            for (int i = 0; i < innerFaces.Count; i++)
            {
                CollectMaxY(innerFaces[i], orientation, flipOrientation, ref maxY, ref found);
            }
        }

        IReadOnlyDictionary<OuterShellPlane, VoxelFace> outerFaces = voxel.OuterShellFaces;
        if (outerFaces != null)
        {
            foreach (var pair in outerFaces)
            {
                CollectMaxY(pair.Value, orientation, flipOrientation, ref maxY, ref found);
            }
        }

        if (!found)
        {
            return false;
        }

        maxLocalY = maxY;
        return true;
    }

    private static void CollectMaxY(
        VoxelFace face,
        Orientation orientation,
        FlipOrientation flipOrientation,
        ref float maxY,
        ref bool found)
    {
        if (face == null || face.Polygons == null)
        {
            return;
        }

        List<List<FaceVertex>> polygons = face.Polygons;
        for (int p = 0; p < polygons.Count; p++)
        {
            List<FaceVertex> polygon = polygons[p];
            if (polygon == null)
            {
                continue;
            }

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector3 transformed = TransformPosition(polygon[i].Position, orientation, flipOrientation);
                if (transformed.y > maxY)
                {
                    maxY = transformed.y;
                }
                found = true;
            }
        }
    }

    private static Vector3 TransformPosition(Vector3 position, Orientation orientation, FlipOrientation flipOrientation)
    {
        int steps = OrientationToSteps(orientation);
        Vector3 local = position;
        if (steps != 0)
        {
            Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 offset = local - pivot;
            Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
            local = rotation * offset + pivot;
        }

        if (flipOrientation == FlipOrientation.NegativeY)
        {
            local.y = 1f - local.y;
        }

        return local;
    }

    private static int OrientationToSteps(Orientation orientation)
    {
        switch (orientation)
        {
            case Orientation.PositiveX:
                return 0;
            case Orientation.PositiveZ:
                return 1;
            case Orientation.NegativeX:
                return 2;
            case Orientation.NegativeZ:
                return 3;
            default:
                return 0;
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
