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

        Orientation orientation = chunk.Voxels[x, y, z].Orientation;
        FlipOrientation flipOrientation = chunk.Voxels[x, y, z].FlipOrientation;
        Vector3 position = new Vector3(x, y, z);
        bool anyOuterVisible = false;

        TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosX, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegX, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosY, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegY, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosZ, vertices, triangles, uvs, ref anyOuterVisible);
        TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegZ, vertices, triangles, uvs, ref anyOuterVisible);

        if (anyOuterVisible)
        {
            IReadOnlyList<VoxelFace> innerFaces = voxel.InnerFaces;
            for (int i = 0; i < innerFaces.Count; i++)
            {
                AddFace(TransformFace(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
            }
        }
    }

    private void TryAddOuterFace(
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

        OuterShellPlane localPlane = MapWorldPlaneToLocal(plane, orientation, flipOrientation);
        if (!voxel.OuterShellFaces.TryGetValue(localPlane, out VoxelFace face))
        {
            return;
        }

        if (face == null || face.Vertices.Count < 3)
        {
            return;
        }

        bool inBounds = neighborX >= 0 && neighborX < Chunk.SizeX
            && neighborY >= 0 && neighborY < Chunk.SizeY
            && neighborZ >= 0 && neighborZ < Chunk.SizeZ;
        int neighborId = inBounds ? chunk.Voxels[neighborX, neighborY, neighborZ].Id : registry != null ? registry.AirId : 0;
        Debug.Log(
            $"Occlusion check: voxel ({x},{y},{z}) plane {plane} -> neighbor ({neighborX},{neighborY},{neighborZ}) " +
            $"inBounds={inBounds} hasNeighbor={hasNeighbor} neighborId={neighborId}");

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

        Debug.Log($"Occlusion result: voxel ({x},{y},{z}) plane {plane} occluded={isOccluded}");

        if (isOccluded)
        {
            return;
        }

        AddFace(rotatedFace, position, vertices, triangles, uvs);
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

    private static OuterShellPlane MapWorldPlaneToLocal(OuterShellPlane plane, Orientation orientation)
    {
        return RotatePlane(plane, -OrientationToSteps(orientation));
    }

    private static OuterShellPlane MapWorldPlaneToLocal(OuterShellPlane plane, Orientation orientation, FlipOrientation flipOrientation)
    {
        OuterShellPlane rotated = RotatePlane(plane, -OrientationToSteps(orientation));
        if (flipOrientation == FlipOrientation.NegativeY)
        {
            return FlipPlaneY(rotated);
        }

        return rotated;
    }

    private static OuterShellPlane RotatePlane(OuterShellPlane plane, int steps)
    {
        int normalized = ((steps % 4) + 4) % 4;
        if (normalized == 0)
        {
            return plane;
        }

        Vector3 normal = OuterShellPlaneUtil.PlaneToNormal(plane);
        Quaternion rotation = Quaternion.AngleAxis(-normalized * 90f, Vector3.up);
        Vector3 rotatedNormal = rotation * normal;
        if (OuterShellPlaneUtil.TryFromNormal(rotatedNormal, out OuterShellPlane rotatedPlane))
        {
            return rotatedPlane;
        }

        return plane;
    }

    private static OuterShellPlane FlipPlaneY(OuterShellPlane plane)
    {
        switch (plane)
        {
            case OuterShellPlane.PosY:
                return OuterShellPlane.NegY;
            case OuterShellPlane.NegY:
                return OuterShellPlane.PosY;
            default:
                return plane;
        }
    }

    private static VoxelFace TransformFace(VoxelFace face, Orientation orientation, FlipOrientation flipOrientation)
    {
        if (face == null || face.Vertices == null || face.Vertices.Count == 0)
        {
            return face;
        }

        int steps = OrientationToSteps(orientation);
        var rotated = new VoxelFace();
        Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
        List<FaceVertex> sourceVertices = face.Vertices;
        for (int i = 0; i < sourceVertices.Count; i++)
        {
            FaceVertex vertex = sourceVertices[i];
            Vector3 local = vertex.Position;
            if (steps != 0)
            {
                Vector3 offset = local - pivot;
                Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
                local = rotation * offset + pivot;
            }

            if (flipOrientation == FlipOrientation.NegativeY)
            {
                local.y = 1f - local.y;
            }

            vertex.Position = local;
            rotated.Vertices.Add(vertex);
        }

        if (flipOrientation == FlipOrientation.NegativeY)
        {
            rotated.Vertices.Reverse();
        }

        return rotated;
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
