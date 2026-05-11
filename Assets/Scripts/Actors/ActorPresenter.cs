using UnityEngine;

[DisallowMultipleComponent]
public class ActorPresenter : MonoBehaviour
{
	[SerializeField] private ActorView actorView;

	private ActorData actorData;

	public ActorData ActorData => actorData;
	public ActorView ActorView => actorView;
	public float MovementSpeed => actorData != null ? actorData.MovementSpeed : 0f;

	public void Bind(ActorData data)
	{
		if (actorData?.Position != null)
		{
			actorData.Position.Changed -= OnActorPositionChanged;
		}

		actorData = data;

		if (actorData?.Position != null)
		{
			actorData.Position.Changed += OnActorPositionChanged;
			OnActorPositionChanged(actorData.Position.Value);
		}
	}

	protected virtual void Awake()
	{
		if (actorView == null)
		{
			Logger.LogError("[ActorPresenter] ActorView is not assigned in the inspector. Please assign an ActorView child to the ActorPresenter component.", Logger.Severity.Critical, this);
		}
	}

	protected virtual void OnDestroy()
	{
		if (actorData?.Position != null)
		{
			actorData.Position.Changed -= OnActorPositionChanged;
		}
	}

	private void OnActorPositionChanged(Vector3 position)
	{
		transform.position = position;
	}
}
