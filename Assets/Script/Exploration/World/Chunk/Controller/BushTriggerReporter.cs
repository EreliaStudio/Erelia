using UnityEngine;
using Utils;

namespace World.Chunk.Controller
{
	public class BushTriggerReporter : MonoBehaviour
	{
		private void OnTriggerStay(UnityEngine.Collider other)
		{
			HandleTrigger(other);
		}

		private void HandleTrigger(UnityEngine.Collider other)
		{
			if (other == null || !other.CompareTag("Player"))
			{
				return;
			}

			ServiceLocator.Instance.PlayerService.NotifyPlayerWalkingInBush();
		}
	}
}
