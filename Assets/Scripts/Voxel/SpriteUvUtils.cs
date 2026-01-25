using UnityEngine;

public static class SpriteUvUtils
{
    public static void GetSpriteUvRect(Sprite sprite, out Vector2 uvAnchor, out Vector2 uvSize)
    {
        if (sprite == null || sprite.uv == null || sprite.uv.Length == 0)
        {
            uvAnchor = Vector2.zero;
            uvSize = Vector2.one;
            return;
        }

        Vector2 min = sprite.uv[0];
        Vector2 max = sprite.uv[0];

        for (int i = 1; i < sprite.uv.Length; i++)
        {
            Vector2 uv = sprite.uv[i];
            min = Vector2.Min(min, uv);
            max = Vector2.Max(max, uv);
        }

        uvAnchor = min;
        uvSize = max - min;
    }
}
