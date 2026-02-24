using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.UI
{
	public sealed class ScreenTransitionManager : MonoBehaviour
	{
		[SerializeField] private List<ScreenTransitionEffect> effects = new List<ScreenTransitionEffect>();
		[SerializeField] private bool disableInactive = true;

		private ScreenTransitionEffect current;
		private bool isTransitioning;

		private void OnEnable()
		{
			Erelia.Event.Bus.Subscribe<Erelia.Event.EnterTransitionOn>(OnEnterTransitionOn);
			Erelia.Event.Bus.Subscribe<Erelia.Event.EnterTransitionOff>(OnEnterTransitionOff);
		}

		private void OnDisable()
		{
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.EnterTransitionOn>(OnEnterTransitionOn);
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.EnterTransitionOff>(OnEnterTransitionOff);
		}

		private void Awake()
		{
			if (!disableInactive)
			{
				return;
			}

			for (int i = 0; i < effects.Count; i++)
			{
				if (effects[i] != null)
				{
					effects[i].gameObject.SetActive(false);
				}
			}
		}

		public IEnumerator PlayOn()
		{
			current = PickRandom();
			if (current == null)
			{
				yield break;
			}

			current.gameObject.SetActive(true);
			yield return current.PlayOn();
			Erelia.Event.Bus.Emit(new Erelia.Event.ScreenHided());
		}

		public IEnumerator PlayOff()
		{
			if (current == null)
			{
				current = PickRandom();
			}

			if (current == null)
			{
				yield break;
			}

			current.gameObject.SetActive(true);
			yield return current.PlayOff();

			if (disableInactive)
			{
				current.gameObject.SetActive(false);
			}

			Erelia.Event.Bus.Emit(new Erelia.Event.ScreenRevealed());
		}

		private void OnEnterTransitionOn(Erelia.Event.EnterTransitionOn evt)
		{
			if (isTransitioning)
			{
				return;
			}

			StartCoroutine(PlayOnRoutine());
		}

		private void OnEnterTransitionOff(Erelia.Event.EnterTransitionOff evt)
		{
			if (isTransitioning)
			{
				return;
			}

			StartCoroutine(PlayOffRoutine());
		}

		private IEnumerator PlayOnRoutine()
		{
			isTransitioning = true;
			yield return PlayOn();
			isTransitioning = false;
		}

		private IEnumerator PlayOffRoutine()
		{
			isTransitioning = true;
			yield return PlayOff();
			isTransitioning = false;
		}

		private ScreenTransitionEffect PickRandom()
		{
			if (effects == null || effects.Count == 0)
			{
				return null;
			}

			int start = Random.Range(0, effects.Count);
			for (int i = 0; i < effects.Count; i++)
			{
				int idx = (start + i) % effects.Count;
				if (effects[idx] != null)
				{
					return effects[idx];
				}
			}

			return null;
		}
	}
}
