#if UNITY_EDITOR
using UnityEngine;

public static class SpriteGuiUtility
{
	public static void DrawSprite(Rect rect, Sprite sprite)
	{
		if (sprite == null || sprite.texture == null)
		{
			return;
		}

		Texture2D texture = sprite.texture;
		Rect spriteRect = sprite.rect;
		Rect uv = new Rect(
			spriteRect.x / texture.width,
			spriteRect.y / texture.height,
			spriteRect.width / texture.width,
			spriteRect.height / texture.height);

		GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
	}
}
#endif
