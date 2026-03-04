using UnityEngine;

namespace Erelia.Battle.Player
{
	/// <summary>
	/// View component for the battle player.
	/// Provides a linked camera transform used to orient movement.
	/// </summary>
	public sealed class View : MonoBehaviour
	{
		/// <summary>
		/// Optional camera transform linked to the player.
		/// </summary>
		[SerializeField] private Transform linkedCamera;

		/// <summary>
		/// Gets the linked camera transform or this transform as a fallback.
		/// </summary>
		public Transform LinkedCamera => linkedCamera != null ? linkedCamera : transform;
	}
}
