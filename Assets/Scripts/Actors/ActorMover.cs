using System.Collections.Generic;
using UnityEngine;

public sealed class ActorMover
{
	private const float PositionEpsilon = 0.0001f;

	private enum MovementPhase
	{
		MoveToTransitionPoint,
		MoveToStationaryPoint
	}

	private readonly Actor actor;
	private readonly List<Vector3Int> path = new List<Vector3Int>();
	private int pathIndex;
	private MovementPhase movementPhase = MovementPhase.MoveToTransitionPoint;

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
		movementPhase = MovementPhase.MoveToTransitionPoint;
	}

	public void Stop()
	{
		path.Clear();
		pathIndex = 0;
		movementPhase = MovementPhase.MoveToTransitionPoint;
	}

	public bool Tick(WorldData p_worldData, VoxelRegistry p_voxelRegistry, float p_deltaTime)
	{
		if (actor == null || !IsMoving)
		{
			return false;
		}

		Vector3Int previousCell = path[pathIndex - 1];
		Vector3Int targetCell = path[pathIndex];
		CardinalHeightSet.Direction exitDirection = ResolveExitDirection(previousCell, targetCell);
		CardinalHeightSet.Direction entryDirection = ResolveEntryDirection(previousCell, targetCell);
		if (exitDirection == CardinalHeightSet.Direction.Stationary || entryDirection == CardinalHeightSet.Direction.Stationary)
		{
			Stop();
			return false;
		}

		if (!TryGetCurrentPhaseTarget(p_worldData, p_voxelRegistry, previousCell, targetCell, exitDirection, entryDirection, out Vector3 targetWorldPoint))
		{
			Stop();
			return false;
		}

		Transform actorTransform = actor.transform;
		actorTransform.position = Vector3.MoveTowards(actorTransform.position, targetWorldPoint, actor.MovementSpeed * p_deltaTime);
		if ((actorTransform.position - targetWorldPoint).sqrMagnitude > PositionEpsilon)
		{
			return true;
		}

		actorTransform.position = targetWorldPoint;
		if (movementPhase == MovementPhase.MoveToTransitionPoint)
		{
			movementPhase = MovementPhase.MoveToStationaryPoint;
			return true;
		}

		actor.NotifyCellReached(targetCell);
		pathIndex++;
		movementPhase = MovementPhase.MoveToTransitionPoint;

		if (pathIndex >= path.Count)
		{
			Stop();
			return false;
		}

		return true;
	}

	private bool TryGetCurrentPhaseTarget(
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		Vector3Int p_previousCell,
		Vector3Int p_targetCell,
		CardinalHeightSet.Direction p_exitDirection,
		CardinalHeightSet.Direction p_entryDirection,
		out Vector3 p_targetWorldPoint)
	{
		if (movementPhase == MovementPhase.MoveToTransitionPoint)
		{
			return TryGetTransitionWorldPoint(
				p_worldData,
				p_voxelRegistry,
				p_previousCell,
				p_targetCell,
				p_exitDirection,
				p_entryDirection,
				out p_targetWorldPoint);
		}

		return VoxelTraversalUtility.TryGetTraversalWorldPoint(p_worldData, p_targetCell, CardinalHeightSet.Direction.Stationary, p_voxelRegistry, out p_targetWorldPoint);
	}

	private static bool TryGetTransitionWorldPoint(
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		Vector3Int p_previousCell,
		Vector3Int p_targetCell,
		CardinalHeightSet.Direction p_exitDirection,
		CardinalHeightSet.Direction p_entryDirection,
		out Vector3 p_transitionWorldPoint)
	{
		p_transitionWorldPoint = default;

		if (!VoxelTraversalUtility.TryGetTraversalWorldPoint(p_worldData, p_previousCell, p_exitDirection, p_voxelRegistry, out Vector3 exitWorldPoint) ||
			!VoxelTraversalUtility.TryGetTraversalWorldPoint(p_worldData, p_targetCell, p_entryDirection, p_voxelRegistry, out Vector3 entryWorldPoint))
		{
			return false;
		}

		bool useExitPoint = exitWorldPoint.y >= entryWorldPoint.y;
		Vector3 selectedPoint = useExitPoint ? exitWorldPoint : entryWorldPoint;
		Vector3 otherPoint = useExitPoint ? entryWorldPoint : exitWorldPoint;

		p_transitionWorldPoint = new Vector3(
			selectedPoint.x,
			selectedPoint.y,
			selectedPoint.z);

		if ((selectedPoint - otherPoint).sqrMagnitude > PositionEpsilon)
		{
			p_transitionWorldPoint = new Vector3(
				(selectedPoint.x + otherPoint.x) * 0.5f,
				selectedPoint.y,
				(selectedPoint.z + otherPoint.z) * 0.5f);
		}

		return true;
	}

	private static CardinalHeightSet.Direction ResolveEntryDirection(Vector3Int p_previousCell, Vector3Int p_targetCell)
	{
		int deltaX = p_targetCell.x - p_previousCell.x;
		int deltaZ = p_targetCell.z - p_previousCell.z;

		if (deltaX > 0)
		{
			return CardinalHeightSet.Direction.NegativeX;
		}

		if (deltaX < 0)
		{
			return CardinalHeightSet.Direction.PositiveX;
		}

		if (deltaZ > 0)
		{
			return CardinalHeightSet.Direction.NegativeZ;
		}

		if (deltaZ < 0)
		{
			return CardinalHeightSet.Direction.PositiveZ;
		}

		return CardinalHeightSet.Direction.Stationary;
	}

	private static CardinalHeightSet.Direction ResolveExitDirection(Vector3Int p_previousCell, Vector3Int p_targetCell)
	{
		int deltaX = p_targetCell.x - p_previousCell.x;
		int deltaZ = p_targetCell.z - p_previousCell.z;

		if (deltaX > 0)
		{
			return CardinalHeightSet.Direction.PositiveX;
		}

		if (deltaX < 0)
		{
			return CardinalHeightSet.Direction.NegativeX;
		}

		if (deltaZ > 0)
		{
			return CardinalHeightSet.Direction.PositiveZ;
		}

		if (deltaZ < 0)
		{
			return CardinalHeightSet.Direction.NegativeZ;
		}

		return CardinalHeightSet.Direction.Stationary;
	}
}
