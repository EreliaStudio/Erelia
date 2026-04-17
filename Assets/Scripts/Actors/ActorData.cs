using UnityEngine;

[System.Serializable]
public class ActorData
{
	[SerializeField, Min(0.01f)] private float movementSpeed = 4f;

	public float MovementSpeed => movementSpeed;
}
