using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.UI.Effects
{
	public sealed class RadarSweepEffect : Erelia.UI.ScreenTransitionEffect
	{
		private Image radialImage;
		[SerializeField] private Color coverColor = Color.black;
		[SerializeField] private float duration = 0.6f;

		private void Reset()
		{
			radialImage = GetComponentInChildren<Image>();
		}

		private void Awake()
		{
			EnsureImage();
		}

		public override IEnumerator PlayOn()
		{
			EnsureImage();
			yield return Animate(0f, 1f);
		}

		public override IEnumerator PlayOff()
		{
			EnsureImage();
			yield return Animate(1f, 0f);
		}

		private IEnumerator Animate(float from, float to)
		{
			if (radialImage == null)
			{
				yield break;
			}

			radialImage.type = Image.Type.Filled;
			radialImage.fillMethod = Image.FillMethod.Radial360;
			radialImage.fillOrigin = 2;
			radialImage.fillClockwise = true;

			float time = 0f;
			while (time < duration)
			{
				time += Time.unscaledDeltaTime;
				float t = Mathf.Clamp01(time / duration);
				radialImage.fillAmount = Mathf.Lerp(from, to, t);
				yield return null;
			}

			radialImage.fillAmount = to;
		}

		private void EnsureImage()
		{
			if (radialImage == null)
			{
				radialImage = GetComponentInChildren<Image>();
			}

			if (radialImage == null)
			{
				GameObject go = new GameObject("CoverImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				go.transform.SetParent(transform, false);
				radialImage = go.GetComponent<Image>();
			}

			if (radialImage != null)
			{
				radialImage.color = coverColor;
				RectTransform imgRect = radialImage.rectTransform;
				imgRect.anchorMin = Vector2.zero;
				imgRect.anchorMax = Vector2.one;
				imgRect.offsetMin = Vector2.zero;
				imgRect.offsetMax = Vector2.zero;
			}
		}
	}
}
