using UnityEngine;

namespace Erelia.Player
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private Transform linkedCamera;

		public Transform LinkedCamera => linkedCamera != null ? linkedCamera : transform;
	}
}
