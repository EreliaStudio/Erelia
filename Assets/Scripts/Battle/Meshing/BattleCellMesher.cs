using System.Collections.Generic;
using UnityEngine;

public class BattleCellMesher
{
    private const float MaskHeightOffset = 0.02f;

    protected void AddQuad(
        int x,
        int y,
        int z,
        Vector2 uvMin,
        Vector2 uvMax,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs)
    {
        float left = x;
        float right = x + 1f;
        float bottom = z;
        float top = z + 1f;
        float height = y + MaskHeightOffset;

        int baseIndex = vertices.Count;
        vertices.Add(new Vector3(left, height, bottom));
        vertices.Add(new Vector3(right, height, bottom));
        vertices.Add(new Vector3(right, height, top));
        vertices.Add(new Vector3(left, height, top));

        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 1);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex);
        triangles.Add(baseIndex + 2);
        triangles.Add(baseIndex + 3);

        uvs.Add(new Vector2(uvMin.x, uvMin.y));
        uvs.Add(new Vector2(uvMax.x, uvMin.y));
        uvs.Add(new Vector2(uvMax.x, uvMax.y));
        uvs.Add(new Vector2(uvMin.x, uvMax.y));
    }
}
