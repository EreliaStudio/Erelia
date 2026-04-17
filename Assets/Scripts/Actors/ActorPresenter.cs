using UnityEngine;

[DisallowMultipleComponent]
public class ActorPresenter : MonoBehaviour
{
	[SerializeField] private ActorView actorView;

	private ActorData actorData;

	public event System.Action<ActorPresenter, Vector3Int> CellReached;

	public ActorData ActorData => actorData;
	public ActorView ActorView => actorView;
	public float MovementSpeed => actorData != null ? actorData.MovementSpeed : 0f;

	public void Bind(ActorData data)
	{
		actorData = data;
	}

	protected virtual void Awake()
	{
		if (actorView == null)
		{
			Logger.LogError("[ActorPresenter] ActorView is not assigned in the inspector. Please assign an ActorView child to the ActorPresenter component.", Logger.Severity.Critical, this);
		}
	}

	public void NotifyCellReached(Vector3Int worldCellPosition)
	{
		CellReached?.Invoke(this, worldCellPosition);
	}
}
