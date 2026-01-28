using System;
using System.Collections.Generic;
using UnityEngine;

public static class OrganicBoardShapeGenerator
{
    private static readonly Vector2Int[] Neighbors =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    public static HashSet<Vector2Int> BuildCells(int size, int seed, float noiseScale, float noiseStrength, float minEdgeChance, int minCells)
    {
        var result = new HashSet<Vector2Int>();
        if (size <= 0)
        {
            return result;
        }

        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        var random = new System.Random(seed);

        queue.Enqueue(Vector2Int.zero);
        visited.Add(Vector2Int.zero);
        result.Add(Vector2Int.zero);

        float seedOffset = seed * 0.0137f;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            for (int i = 0; i < Neighbors.Length; i++)
            {
                Vector2Int next = current + Neighbors[i];
                if (visited.Contains(next))
                {
                    continue;
                }

                visited.Add(next);
                if (next.magnitude > size)
                {
                    continue;
                }

                float dist01 = Mathf.Clamp01(next.magnitude / Mathf.Max(1f, size));
                float baseChance = Mathf.Lerp(1f, minEdgeChance, dist01);
                float noise = Mathf.PerlinNoise((next.x + seedOffset) * noiseScale, (next.y + seedOffset) * noiseScale);
                float chance = Mathf.Clamp01(baseChance + (noise - 0.5f) * noiseStrength);

                if (random.NextDouble() <= chance)
                {
                    result.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        if (result.Count < minCells)
        {
            result.Clear();
            int limit = Mathf.Max(1, size);
            for (int x = -limit; x <= limit; x++)
            {
                for (int y = -limit; y <= limit; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (cell.magnitude <= limit)
                    {
                        result.Add(cell);
                    }
                }
            }
        }

        return result;
    }
}
