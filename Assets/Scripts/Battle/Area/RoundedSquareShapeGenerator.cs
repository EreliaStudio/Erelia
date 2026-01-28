using System.Collections.Generic;
using UnityEngine;

public static class RoundedSquareShapeGenerator
{
    public static HashSet<Vector2Int> BuildCells(int radius, int cornerRadius)
    {
        var result = new HashSet<Vector2Int>();
        if (radius <= 0)
        {
            return result;
        }

        int clampedCornerRadius = Mathf.Clamp(cornerRadius, 0, radius);
        int inner = radius - clampedCornerRadius;
        int cornerRadiusSquared = clampedCornerRadius * clampedCornerRadius;

        for (int x = -radius; x <= radius; x++)
        {
            int absX = Mathf.Abs(x);
            for (int z = -radius; z <= radius; z++)
            {
                int absZ = Mathf.Abs(z);

                if (clampedCornerRadius == 0)
                {
                    result.Add(new Vector2Int(x, z));
                    continue;
                }

                if (absX <= inner || absZ <= inner)
                {
                    result.Add(new Vector2Int(x, z));
                    continue;
                }

                int dx = absX - inner;
                int dz = absZ - inner;
                if ((dx * dx) + (dz * dz) <= cornerRadiusSquared)
                {
                    result.Add(new Vector2Int(x, z));
                }
            }
        }

        return result;
    }
}
