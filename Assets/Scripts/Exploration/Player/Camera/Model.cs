using UnityEngine;

namespace Erelia.Exploration.Player.Camera
{
	/// <summary>
	/// Serializable configuration for exploration camera controls.
	/// Stores orbit and zoom tuning values used by the camera presenter.
	/// </summary>
	[System.Serializable]
	public sealed class Model
	{
		/// <summary>
		/// Sensitivity for mouse orbit input.
		/// </summary>
		[SerializeField] private float mouseOrbitSensitivity = 0.12f;

		/// <summary>
		/// Orbit speed for keyboard input (degrees per second).
		/// </summary>
		[SerializeField] private float keyOrbitSpeed = 90f;

		/// <summary>
		/// Zoom speed for mouse wheel input.
		/// </summary>
		[SerializeField] private float zoomSpeed = 2f;

		/// <summary>
		/// Minimum allowed camera orbit distance.
		/// </summary>
		[SerializeField] private float minOrbitDistance = 2f;

		/// <summary>
		/// Maximum allowed camera orbit distance.
		/// </summary>
		[SerializeField] private float maxOrbitDistance = 25f;

		/// <summary>
		/// Gets mouse orbit sensitivity.
		/// </summary>
		public float MouseOrbitSensitivity => mouseOrbitSensitivity;

		/// <summary>
		/// Gets keyboard orbit speed.
		/// </summary>
		public float KeyOrbitSpeed => keyOrbitSpeed;

		/// <summary>
		/// Gets zoom speed.
		/// </summary>
		public float ZoomSpeed => zoomSpeed;

		/// <summary>
		/// Gets minimum orbit distance.
		/// </summary>
		public float MinOrbitDistance => minOrbitDistance;

		/// <summary>
		/// Gets maximum orbit distance.
		/// </summary>
		public float MaxOrbitDistance => maxOrbitDistance;
	}
}
