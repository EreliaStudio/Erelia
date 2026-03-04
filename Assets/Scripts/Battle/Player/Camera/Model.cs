using UnityEngine;

namespace Erelia.Battle.Player.Camera
{
	/// <summary>
	/// Serializable configuration for battle camera controls.
	/// Stores orbit and zoom tuning values used by the camera presenter.
	/// </summary>
	[System.Serializable]
	public sealed class Model
	{
		/// <summary>
		/// Mouse orbit sensitivity.
		/// </summary>
		[SerializeField] private float mouseOrbitSensitivity = 0.12f;
		/// <summary>
		/// Keyboard orbit speed in degrees per second.
		/// </summary>
		[SerializeField] private float keyOrbitSpeed = 90f;
		/// <summary>
		/// Zoom speed for scroll input.
		/// </summary>
		[SerializeField] private float zoomSpeed = 2f;
		/// <summary>
		/// Minimum allowed orbit distance.
		/// </summary>
		[SerializeField] private float minOrbitDistance = 2f;
		/// <summary>
		/// Maximum allowed orbit distance.
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
