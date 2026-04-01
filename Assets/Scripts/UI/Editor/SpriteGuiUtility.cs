using UnityEngine;

public static class SpriteGuiUtility
{
	public static void DrawSprite(Rect p_rect, Sprite p_sprite)
	{
		if (p_sprite == null || p_sprite.texture == null)
		{
			return;
		}

		Texture2D texture = p_sprite.texture;
		Rect spriteRect = p_sprite.rect;

		Rect uv = new Rect(
			spriteRect.x / texture.width,
			spriteRect.y / texture.height,
			spriteRect.width / texture.width,
			spriteRect.height / texture.height
		);

		GUI.DrawTextureWithTexCoords(p_rect, texture, uv, true);
	}
}