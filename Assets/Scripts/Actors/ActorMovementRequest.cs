using UnityEngine;

public readonly struct ActorMovementRequest
{
	public readonly Actor Actor;
	public readonly Vector3Int DestinationWorldPosition;

	public ActorMovementRequest(Actor p_actor, Vector3Int p_destinationWorldPosition)
	{
		Actor = p_actor;
		DestinationWorldPosition = p_destinationWorldPosition;
	}
}
