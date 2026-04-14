using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActorManager : MonoBehaviour
{
	[SerializeField] private WorldPresenter worldPresenter;

	private readonly Dictionary<Actor, ActorMover> moversByActor = new Dictionary<Actor, ActorMover>();
	private readonly List<Actor> completedActors = new List<Actor>();
	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();

	private void Reset()
	{
		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>();
		}
	}

	private void Awake()
	{
		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>();
		}
	}

	private void OnEnable()
	{
		EventCenter.ActorMoveRequested += OnActorMoveRequested;
	}

	private void OnDisable()
	{
		EventCenter.ActorMoveRequested -= OnActorMoveRequested;
		moversByActor.Clear();
		completedActors.Clear();
		graphCache.Clear();
	}

	private void Update()
	{
		if (worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		completedActors.Clear();

		foreach (KeyValuePair<Actor, ActorMover> pair in moversByActor)
		{
			Actor actor = pair.Key;
			ActorMover mover = pair.Value;
			if (actor == null || mover == null || !mover.Tick(worldPresenter.WorldData, worldPresenter.VoxelRegistry, Time.deltaTime))
			{
				completedActors.Add(actor);
			}
		}

		for (int index = 0; index < completedActors.Count; index++)
		{
			moversByActor.Remove(completedActors[index]);
		}
	}

	private void OnActorMoveRequested(ActorMovementRequest p_request)
	{
		Debug.Log($"ActorManager received move request for '{p_request.Actor?.name ?? "null"}' toward {p_request.DestinationWorldPosition}.", this);

		if (p_request.Actor == null || worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		if (!WorldPathfinder.TryResolveStandingCell(
				worldPresenter.WorldData,
				worldPresenter.VoxelRegistry,
				graphCache,
				p_request.Actor.transform.position,
				out Vector3Int startWorldPosition))
		{
			return;
		}

		if (!WorldPathfinder.TryFindPath(
				worldPresenter.WorldData,
				worldPresenter.VoxelRegistry,
				graphCache,
				startWorldPosition,
				p_request.DestinationWorldPosition,
				out List<Vector3Int> path))
		{
			return;
		}

		ActorMover mover = GetOrCreateMover(p_request.Actor);
		mover.SetPath(path);
	}

	private ActorMover GetOrCreateMover(Actor p_actor)
	{
		if (moversByActor.TryGetValue(p_actor, out ActorMover existingMover) && existingMover != null)
		{
			return existingMover;
		}

		ActorMover mover = new ActorMover(p_actor);
		moversByActor[p_actor] = mover;
		return mover;
	}
}
