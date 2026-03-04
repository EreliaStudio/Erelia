using UnityEngine;

namespace Erelia.Exploration.Player
{
	/// <summary>
	/// View component for the exploration player.
	/// Provides a linked camera transform for movement orientation, falling back to self.
	/// </summary>
	public sealed class View : MonoBehaviour
	{
		/// <summary>
		/// Optional camera transform linked to the player.
		/// </summary>
		[SerializeField] private Transform linkedCamera;

		/// <summary>
		/// Returns the linked camera transform if assigned; otherwise this object's transform.
		/// </summary>
		public Transform LinkedCamera => linkedCamera != null ? linkedCamera : transform;
	}
}
