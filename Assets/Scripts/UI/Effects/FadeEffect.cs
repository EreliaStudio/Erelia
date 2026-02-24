using System.Collections;
using UnityEngine;

namespace Erelia.UI
{
	public sealed class FadeEffect : Erelia.UI.ScreenTransitionEffect
	{
		[SerializeField] private CanvasGroup group;
		[SerializeField] private float fadeDuration = 0.4f;

		private void Reset()
		{
			group = GetComponentInChildren<CanvasGroup>();
		}

		public override IEnumerator PlayOn()
		{
			yield return FadeTo(1f);
		}

		public override IEnumerator PlayOff()
		{
			yield return FadeTo(0f);
		}

		private IEnumerator FadeTo(float target)
		{
			if (group == null)
			{
				yield break;
			}

			float start = group.alpha;
			float time = 0f;
			while (time < fadeDuration)
			{
				time += Time.unscaledDeltaTime;
				float t = Mathf.Clamp01(time / fadeDuration);
				group.alpha = Mathf.Lerp(start, target, t);
				yield return null;
			}

			group.alpha = target;
			group.blocksRaycasts = target > 0f;
		}
	}
}
