using UnityEngine;

namespace Erelia.Exploration.Player.Camera
{
	/// <summary>
	/// View component for the exploration camera.
	/// Exposes the camera transform for orbit and zoom operations.
	/// </summary>
	public sealed class View : MonoBehaviour
	{
		/// <summary>
		/// Gets the camera transform.
		/// </summary>
		public Transform CameraTransform => transform;
	}
}
