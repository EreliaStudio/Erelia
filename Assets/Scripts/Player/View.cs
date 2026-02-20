using UnityEngine;

namespace Erelia.Player
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private Transform target;

		public Transform Target => target != null ? target : transform;
	}
}
