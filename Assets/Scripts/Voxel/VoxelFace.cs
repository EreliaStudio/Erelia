using System.Collections.Generic;
using UnityEngine;

public struct FaceVertex
{
    public Vector3 Position;
    public Vector2 TileUV;
}

public class VoxelFace
{
    public List<FaceVertex> Vertices = new List<FaceVertex>();

    public void ApplyOffset(Vector2 tileOffset)
    {
        for (int i = 0; i < Vertices.Count; i++)
        {
            FaceVertex vertex = Vertices[i];
            vertex.TileUV += tileOffset;
            Vertices[i] = vertex;
        }
    }

    public bool IsOccludedBy(VoxelFace other)
    {
        if (other == null || Vertices == null || other.Vertices == null || Vertices.Count < 3 || other.Vertices.Count < 3)
        {
            return false;
        }

        Vector3 normal = GeometryUtils.GetNormal(Vertices);
        if (normal.sqrMagnitude < OuterShellPlaneUtil.NormalEpsilon)
        {
            return false;
        }

        return GeometryUtils.IsPolygonContained(Vertices, other.Vertices, normal);
    }
}
