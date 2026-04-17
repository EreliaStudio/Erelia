using UnityEngine;

public readonly struct ActorMovementRequest
{
	public readonly ActorPresenter Actor;
	public readonly Vector3Int DestinationWorldPosition;

	public ActorMovementRequest(ActorPresenter p_actor, Vector3Int p_destinationWorldPosition)
	{
		Actor = p_actor;
		DestinationWorldPosition = p_destinationWorldPosition;
	}
}
