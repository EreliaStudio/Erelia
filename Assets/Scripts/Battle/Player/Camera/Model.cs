using UnityEngine;

namespace Erelia.Battle.Player.Camera
{
	[System.Serializable]
	public sealed class Model
	{
		[SerializeField] private float mouseOrbitSensitivity = 0.12f;
		[SerializeField] private float keyOrbitSpeed = 90f;
		[SerializeField] private float zoomSpeed = 2f;
		[SerializeField] private float minOrbitDistance = 2f;
		[SerializeField] private float maxOrbitDistance = 25f;

		public float MouseOrbitSensitivity => mouseOrbitSensitivity;
		public float KeyOrbitSpeed => keyOrbitSpeed;
		public float ZoomSpeed => zoomSpeed;
		public float MinOrbitDistance => minOrbitDistance;
		public float MaxOrbitDistance => maxOrbitDistance;
	}
}
