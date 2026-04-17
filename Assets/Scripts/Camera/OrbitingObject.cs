using UnityEngine;

[DisallowMultipleComponent]
public class OrbitingObject : MonoBehaviour
{
	[SerializeField] private Vector3 orbitTargetLocalPoint = Vector3.zero;
	[SerializeField, Min(0f)] private float orbitSpeed = 120f;

	private void Awake()
	{
		LookAtTarget();
	}

	private void Start()
	{
		LookAtTarget();
	}

	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			LookAtTarget();
		}
	}

	private void LateUpdate()
	{
		LookAtTarget();
	}

	public void Orbit(float axis, float deltaTime)
	{
		if (Mathf.Abs(axis) <= 0.0001f)
		{
			return;
		}

		Vector3 localOffset = transform.localPosition - orbitTargetLocalPoint;
		if (localOffset.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		Quaternion rotation = Quaternion.AngleAxis(axis * orbitSpeed * deltaTime, Vector3.up);
		transform.localPosition = orbitTargetLocalPoint + rotation * localOffset;
	}

	private void LookAtTarget()
	{
		Vector3 worldTarget = transform.parent != null
			? transform.parent.TransformPoint(orbitTargetLocalPoint)
			: orbitTargetLocalPoint;

		Vector3 direction = worldTarget - transform.position;
		if (direction.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
	}
}
