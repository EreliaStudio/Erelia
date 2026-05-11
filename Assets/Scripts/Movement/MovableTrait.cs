using System;
using UnityEngine;

[Serializable]
public class MovableTrait
{
	[SerializeField, Min(0.01f)] public float MovementSpeed = 4f;
	[SerializeField] public ObservableValue<Vector3> Position = new ObservableValue<Vector3>();

	public event Action<MovableTrait, Vector3Int> CellReached;

	public void SetPosition(Vector3 p_position, bool p_forceNotify = false)
	{
		Position.Set(p_position, p_forceNotify);
	}

	public void NotifyCellReached(Vector3Int p_cellPosition)
	{
		CellReached?.Invoke(this, p_cellPosition);
		OnCellReached(p_cellPosition);
	}

	protected virtual void OnCellReached(Vector3Int p_cellPosition)
	{
	}
}
