using UnityEngine;
using Utils;

namespace World.Controller
{
	public class BushTriggerReporter : MonoBehaviour
	{
		private void OnTriggerStay(Collider other)
		{
			HandleTrigger(other);
		}

		private void HandleTrigger(Collider other)
		{
			Debug.Log("Coucou");
			if (other == null || !other.CompareTag("Player"))
			{
				return;
			}

			ServiceLocator.Instance.PlayerService.NotifyPlayerWalkingInBush();
		}
	}
}
