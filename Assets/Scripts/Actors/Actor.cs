using UnityEngine;

[DisallowMultipleComponent]
public class Actor : MonoBehaviour
{
	[SerializeField, Min(0.01f)] private float movementSpeed = 4f;

	public event System.Action<Actor, Vector3Int> CellReached;

	public float MovementSpeed => movementSpeed;

	public void NotifyCellReached(Vector3Int worldCellPosition)
	{
		CellReached?.Invoke(this, worldCellPosition);
	}
}
