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

                    AddMaskFaces(cell, board, x, y, z, mappings, vertices, triangles, uvs);
                }
            }
        }

        if (vertices.Count == 0)
        {
            return mesh;
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

                    Mesh mesh = BuildCellMaskMesh(board, cell, x, y, z, mappings);
                    if (mesh != null && mesh.vertexCount > 0)
                    {
                        meshes.Add(mesh);
                    }
                }
            }
        }

        return meshes;
    }

    private static VoxelRegistry ResolveRegistry()
    {
        return BattleRequestStore.Current?.Registry;
    }

    private static void AddMaskFaces(
        BattleCell cell,
        BattleBoardData board,
        int x,
        int y,
        int z,
        IReadOnlyList<BattleMaskSpriteMapping> mappings,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs)
    {
        if (board == null || board.Voxels == null || mappings == null)
        {
            return;
        }

        VoxelRegistry registry = ResolveRegistry();
        if (registry == null)
        {
            return;
        }

        if (!TryGetMaskVoxelCell(board, registry, x, y, z, out VoxelCell voxelCell))
        {
            return;
        }

        if (!registry.TryGetVoxel(voxelCell.Id, out Voxel voxel) || voxel == null)
        {
            return;
        }

        IReadOnlyList<VoxelFace> maskFaces = voxel.MaskFaces;
        if (maskFaces == null || maskFaces.Count == 0)
        {
            return;
        }

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

            for (int f = 0; f < maskFaces.Count; f++)
            {
                AddMaskFace(maskFaces[f], voxelCell.Orientation, voxelCell.FlipOrientation, x, y, z, uvMin, uvMax, vertices, triangles, uvs);
            }
        }
    }

    private static Mesh BuildCellMaskMesh(
        BattleBoardData board,
        BattleCell cell,
        int x,
        int y,
        int z,
        IReadOnlyList<BattleMaskSpriteMapping> mappings)
    {
        if (board == null || board.Voxels == null || mappings == null)
        {
            return null;
        }

        VoxelRegistry registry = ResolveRegistry();
        if (registry == null)
        {
            return null;
        }

        if (!TryGetMaskVoxelCell(board, registry, x, y, z, out VoxelCell voxelCell))
        {
            return null;
        }

        if (!registry.TryGetVoxel(voxelCell.Id, out Voxel voxel) || voxel == null)
        {
            return null;
        }

        IReadOnlyList<VoxelFace> maskFaces = voxel.MaskFaces;
        if (maskFaces == null || maskFaces.Count == 0)
        {
            return null;
        }

        var mesh = new Mesh { name = $"BattleCellCollisionMask {x},{y},{z}" };
        var localVertices = new List<Vector3>();
        var localTriangles = new List<int>();
        var localUvs = new List<Vector2>();

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

            for (int f = 0; f < maskFaces.Count; f++)
            {
                AddMaskFace(maskFaces[f], voxelCell.Orientation, voxelCell.FlipOrientation, x, y, z, uvMin, uvMax, localVertices, localTriangles, localUvs);
            }
        }

        if (localVertices.Count == 0)
        {
            return mesh;
        }

        mesh.SetVertices(localVertices);
        FlipWindingInPlace(localTriangles);
        mesh.SetTriangles(localTriangles, 0);
        mesh.SetUVs(0, localUvs);
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void AddMaskFace(
        VoxelFace face,
        Orientation orientation,
        FlipOrientation flipOrientation,
        int x,
        int y,
        int z,
        Vector2 uvMin,
        Vector2 uvMax,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs)
    {
        if (face == null || face.Polygons == null || face.Polygons.Count == 0)
        {
            return;
        }

        Vector3 offset = new Vector3(x, y, z);
        List<List<FaceVertex>> polygons = face.Polygons;
        for (int p = 0; p < polygons.Count; p++)
        {
            List<FaceVertex> polygon = polygons[p];
            if (polygon == null || polygon.Count < 3)
            {
                continue;
            }

            int start = vertices.Count;
            for (int i = 0; i < polygon.Count; i++)
            {
                FaceVertex vertex = polygon[i];
                Vector3 local = TransformPosition(vertex.Position, orientation, flipOrientation);
                vertices.Add(offset + local);
                uvs.Add(MapMaskUv(vertex.TileUV, uvMin, uvMax));
            }

            for (int i = 1; i < polygon.Count - 1; i++)
            {
                triangles.Add(start);
                triangles.Add(start + i);
                triangles.Add(start + i + 1);
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

    private static Vector2 MapMaskUv(Vector2 baseUv, Vector2 uvMin, Vector2 uvMax)
    {
        return new Vector2(
            Mathf.Lerp(uvMin.x, uvMax.x, baseUv.x),
            Mathf.Lerp(uvMin.y, uvMax.y, baseUv.y));
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

    private static bool TryGetMaskVoxelCell(
        BattleBoardData board,
        VoxelRegistry registry,
        int x,
        int y,
        int z,
        out VoxelCell voxelCell)
    {
        voxelCell = default;
        if (board == null || board.Voxels == null || registry == null)
        {
            return false;
        }

        if (x < 0 || x >= board.SizeX || y < 0 || y >= board.SizeY || z < 0 || z >= board.SizeZ)
        {
            return false;
        }

        voxelCell = board.Voxels[x, y, z];
        return voxelCell.Id != registry.AirId;
    }
}
