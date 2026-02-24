using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.UI.Effects
{
	public sealed class DiagonalWipeEffect : Erelia.UI.ScreenTransitionEffect
	{
		[SerializeField] private RectTransform maskRect;
		private Image coverImage;
		[SerializeField] private Color coverColor = Color.black;
		[SerializeField] private float duration = 0.6f;
		[SerializeField] private float angle = 45f;

		private void Reset()
		{
			maskRect = GetComponent<RectTransform>();
			coverImage = GetComponentInChildren<Image>();
		}

		private void Awake()
		{
			EnsureMask();
		}

		public override IEnumerator PlayOn()
		{
			yield return Animate(0f, 1f);
		}

		public override IEnumerator PlayOff()
		{
			yield return Animate(1f, 0f);
		}

		private IEnumerator Animate(float from, float to)
		{
			if (maskRect == null)
			{
				yield break;
			}

			EnsureMask();
			RectTransform parent = maskRect.parent as RectTransform;
			Vector2 full = parent != null ? parent.rect.size : maskRect.rect.size;
			if (full.x <= 0f || full.y <= 0f)
			{
				yield break;
			}

			float diagonal = Mathf.Sqrt((full.x * full.x) + (full.y * full.y));
			maskRect.localRotation = Quaternion.Euler(0f, 0f, angle);
			maskRect.pivot = new Vector2(0f, 0.5f);
			maskRect.anchorMin = new Vector2(0.5f, 0.5f);
			maskRect.anchorMax = new Vector2(0.5f, 0.5f);
			maskRect.anchoredPosition = Vector2.zero;

			float time = 0f;
			while (time < duration)
			{
				time += Time.unscaledDeltaTime;
				float t = Mathf.Clamp01(time / duration);
				float w = Mathf.Lerp(from, to, t) * diagonal;
				maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
				maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, diagonal);
				yield return null;
			}
		}

		private void EnsureMask()
		{
			if (maskRect == null)
			{
				maskRect = GetComponent<RectTransform>();
			}

			if (maskRect != null && maskRect.GetComponent<RectMask2D>() == null)
			{
				maskRect.gameObject.AddComponent<RectMask2D>();
			}

			EnsureCoverImage();
		}

		private void EnsureCoverImage()
		{
			if (coverImage == null)
			{
				coverImage = GetComponentInChildren<Image>();
			}

			if (coverImage == null)
			{
				GameObject go = new GameObject("CoverImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				go.transform.SetParent(transform, false);
				coverImage = go.GetComponent<Image>();
			}

			if (coverImage != null)
			{
				coverImage.color = coverColor;
				RectTransform imgRect = coverImage.rectTransform;
				imgRect.anchorMin = Vector2.zero;
				imgRect.anchorMax = Vector2.one;
				imgRect.offsetMin = Vector2.zero;
				imgRect.offsetMax = Vector2.zero;
			}
		}
	}
}
