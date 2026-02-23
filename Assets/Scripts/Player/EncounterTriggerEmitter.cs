using UnityEngine;

namespace Erelia.Player
{
	public sealed class EncounterTriggerEmitter : MonoBehaviour
	{
		private void OnEnable()
		{
			Erelia.Event.Bus.Subscribe<Erelia.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void OnDisable()
		{
			Erelia.Event.Bus.Unsubscribe<Erelia.Event.PlayerMotion>(OnPlayerMotion);
		}

		private void OnPlayerMotion(Erelia.Event.PlayerMotion evt)
		{
			if (evt == null)
			{
				return;
			}

			Erelia.Event.Bus.Emit(new Erelia.Event.EncounterTriggerEvent());
		}
	}
}
