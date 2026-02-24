using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.UI.Effects
{
	public sealed class InstantCoverEffect : Erelia.UI.ScreenTransitionEffect
	{
		private Image coverImage;
		[SerializeField] private Color coverColor = Color.black;

		private void Reset()
		{
			coverImage = GetComponentInChildren<Image>();
		}

		private void Awake()
		{
			EnsureCoverImage();
		}

		public override IEnumerator PlayOn()
		{
			EnsureCoverImage();
			if (coverImage != null)
			{
				coverImage.enabled = true;
			}
			yield break;
		}

		public override IEnumerator PlayOff()
		{
			EnsureCoverImage();
			if (coverImage != null)
			{
				coverImage.enabled = false;
			}
			yield break;
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
