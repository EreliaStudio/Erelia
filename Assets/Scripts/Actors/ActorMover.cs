using System.Collections.Generic;
using UnityEngine;

public sealed class ActorMover
{
	private const float PositionEpsilon = 0.0001f;

	private readonly Actor actor;
	private readonly List<Vector3Int> path = new List<Vector3Int>();
	private int pathIndex;

	public Actor Actor => actor;
	public bool IsMoving => actor != null && pathIndex > 0 && pathIndex < path.Count;

	public ActorMover(Actor p_actor)
	{
		actor = p_actor;
	}

	public void SetPath(IReadOnlyList<Vector3Int> p_path)
	{
		path.Clear();

		if (p_path == null || p_path.Count <= 1)
		{
			pathIndex = 0;
			return;
		}

		for (int index = 0; index < p_path.Count; index++)
		{
			path.Add(p_path[index]);
		}

		pathIndex = 1;
	}

	public void Stop()
	{
		path.Clear();
		pathIndex = 0;
	}

	public bool Tick(WorldData p_worldData, VoxelRegistry p_voxelRegistry, float p_deltaTime)
	{
		if (actor == null || !IsMoving)
		{
			return false;
		}

		Vector3Int targetCell = path[pathIndex];
		if (!WorldPathfinder.TryGetStandingWorldPoint(p_worldData, p_voxelRegistry, targetCell, out Vector3 targetWorldPoint))
		{
			Stop();
			return false;
		}

		Transform actorTransform = actor.transform;
		Vector3 currentPosition = actorTransform.position;
		Vector3 horizontalTarget = new Vector3(targetWorldPoint.x, currentPosition.y, targetWorldPoint.z);
		Vector3 horizontalPosition = new Vector3(currentPosition.x, 0f, currentPosition.z);
		Vector3 targetHorizontalPosition = new Vector3(targetWorldPoint.x, 0f, targetWorldPoint.z);

		if ((horizontalPosition - targetHorizontalPosition).sqrMagnitude > PositionEpsilon)
		{
			Vector3 movedHorizontal = Vector3.MoveTowards(currentPosition, horizontalTarget, actor.MovementSpeed * p_deltaTime);
			actorTransform.position = new Vector3(movedHorizontal.x, currentPosition.y, movedHorizontal.z);
			return true;
		}

		float nextY = Mathf.MoveTowards(currentPosition.y, targetWorldPoint.y, actor.MovementSpeed * p_deltaTime);
		actorTransform.position = new Vector3(targetWorldPoint.x, nextY, targetWorldPoint.z);

		if (Mathf.Abs(nextY - targetWorldPoint.y) > PositionEpsilon)
		{
			return true;
		}

		actorTransform.position = targetWorldPoint;
		actor.NotifyCellReached(targetCell);
		pathIndex++;

		if (pathIndex >= path.Count)
		{
			Stop();
			return false;
		}

		return true;
	}
}
