using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.UI.Effects
{
	public sealed class MosaicRandomEffect : Erelia.UI.ScreenTransitionEffect
	{
		[SerializeField] private int tilesX = 32;
		[SerializeField] private int tilesY = 32;
		[SerializeField] private float duration = 0.6f;
		[SerializeField] private Color color = Color.black;

		private readonly List<Image> tiles = new List<Image>();
		private RectTransform rectTransform;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			EnsureTiles();
		}

		public override IEnumerator PlayOn()
		{
			yield return AnimateTiles(show: true);
		}

		public override IEnumerator PlayOff()
		{
			yield return AnimateTiles(show: false);
		}

		private IEnumerator AnimateTiles(bool show)
		{
			EnsureTiles();
			if (tiles.Count == 0)
			{
				yield break;
			}

			var order = new List<int>(tiles.Count);
			for (int i = 0; i < tiles.Count; i++)
			{
				order.Add(i);
			}

			for (int i = 0; i < order.Count; i++)
			{
				int j = Random.Range(i, order.Count);
				(order[i], order[j]) = (order[j], order[i]);
			}

			float step = duration / Mathf.Max(1, order.Count);
			for (int i = 0; i < order.Count; i++)
			{
				Image tile = tiles[order[i]];
				if (tile != null)
				{
					tile.enabled = show;
				}
				yield return new WaitForSecondsRealtime(step);
			}
		}

		private void EnsureTiles()
		{
			if (rectTransform == null)
			{
				rectTransform = GetComponent<RectTransform>();
			}

			if (tilesX <= 0 || tilesY <= 0)
			{
				return;
			}

			int needed = tilesX * tilesY;
			if (tiles.Count == needed)
			{
				return;
			}

			ClearTiles();

			for (int y = 0; y < tilesY; y++)
			{
				for (int x = 0; x < tilesX; x++)
				{
					var go = new GameObject($"Tile_{x}_{y}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
					go.transform.SetParent(transform, false);
					var tileRect = go.GetComponent<RectTransform>();
					tileRect.anchorMin = new Vector2((float)x / tilesX, (float)y / tilesY);
					tileRect.anchorMax = new Vector2((float)(x + 1) / tilesX, (float)(y + 1) / tilesY);
					tileRect.offsetMin = Vector2.zero;
					tileRect.offsetMax = Vector2.zero;

					Image img = go.GetComponent<Image>();
					img.color = color;
					img.enabled = false;
					tiles.Add(img);
				}
			}
		}

		private void ClearTiles()
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				if (tiles[i] != null)
				{
					Destroy(tiles[i].gameObject);
				}
			}
			tiles.Clear();
		}
	}
}
