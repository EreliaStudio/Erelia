using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActorManager : MonoBehaviour
{
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private GameObject actorPrefab;

	private readonly Dictionary<ActorPresenter, MovablePathDriver> driversByPresenter = new();
	private readonly List<ActorPresenter> completedPresenters = new();
	private readonly WorldTraversalGraphCache graphCache = new();

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[ActorManager] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the ActorManager component.", Logger.Severity.Critical, this);
		}

		if (actorPrefab == null)
		{
			Logger.LogError("[ActorManager] ActorPrefab is not assigned in the inspector. Please assign an actor prefab to the ActorManager component.", Logger.Severity.Critical, this);
		}
	}

	public ActorPresenter SpawnActor(Vector3Int worldCell, ActorData data)
	{
		if (data == null)
		{
			return null;
		}

		Vector3Int resolvedWorldCell = ResolveRuntimeWorldCell(worldCell);
		Vector3 position = ResolveActorWorldPoint(resolvedWorldCell);
		data.SetPosition(position, true);

		GameObject instance = Instantiate(actorPrefab, position, Quaternion.identity, gameObject.transform.parent);
		if (!instance.TryGetComponent(out ActorPresenter presenter))
		{
			return null;
		}

		presenter.Bind(data);
		return presenter;
	}

	public bool TryGetWorldCell(ActorPresenter actor, out Vector3Int worldCell)
	{
		return TryResolveWorldCell(actor, out worldCell);
	}

	public bool TrySetWorldCell(ActorPresenter actor, Vector3Int worldCell)
	{
		if (actor == null || actor.ActorData == null)
		{
			return false;
		}

		Vector3Int resolvedWorldCell = ResolveRuntimeWorldCell(worldCell);
		if (driversByPresenter.TryGetValue(actor, out MovablePathDriver driver))
		{
			driver.Stop();
		}
		actor.ActorData.SetPosition(ResolveActorWorldPoint(resolvedWorldCell), true);
		return true;
	}

	private void OnEnable()
	{
		EventCenter.ActorMoveRequested += OnActorMoveRequested;
	}

	private void OnDisable()
	{
		EventCenter.ActorMoveRequested -= OnActorMoveRequested;
		driversByPresenter.Clear();
		completedPresenters.Clear();
		graphCache.Clear();
	}

	private void Update()
	{
		if (worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		completedPresenters.Clear();

		foreach (KeyValuePair<ActorPresenter, MovablePathDriver> pair in driversByPresenter)
		{
			ActorPresenter presenter = pair.Key;
			MovablePathDriver driver = pair.Value;
			if (presenter == null || driver == null || !driver.Tick(worldPresenter.WorldData, worldPresenter.VoxelRegistry, Time.deltaTime))
			{
				completedPresenters.Add(presenter);
			}
		}

		for (int index = 0; index < completedPresenters.Count; index++)
		{
			driversByPresenter.Remove(completedPresenters[index]);
		}
	}

	private void OnActorMoveRequested(ActorMovementRequest p_request)
	{
		if (p_request.Actor == null ||
			p_request.Actor.ActorData == null ||
			worldPresenter.WorldData == null ||
			worldPresenter.VoxelRegistry == null)
		{
			return;
		}

		if (!WorldPathfinder.TryResolveStandingCell(
				worldPresenter.WorldData,
				worldPresenter.VoxelRegistry,
				graphCache,
				p_request.Actor.ActorData.Position.Value,
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

		GetOrCreateDriver(p_request.Actor).SetPath(path);
	}

	private bool TryResolveWorldCell(ActorPresenter actor, out Vector3Int worldCell)
	{
		worldCell = default;
		return actor != null &&
			actor.ActorData != null &&
			worldPresenter != null &&
			worldPresenter.WorldData != null &&
			worldPresenter.VoxelRegistry != null &&
			WorldPathfinder.TryResolveStandingCell(
				worldPresenter.WorldData,
				worldPresenter.VoxelRegistry,
				graphCache,
				actor.ActorData.Position.Value,
				out worldCell);
	}

	private Vector3Int ResolveRuntimeWorldCell(Vector3Int worldCell)
	{
		if (worldPresenter == null ||
			worldPresenter.WorldData == null ||
			worldPresenter.VoxelRegistry == null)
		{
			return worldCell;
		}

		Vector3 approximateWorldPoint = new Vector3(worldCell.x + 0.5f, worldCell.y, worldCell.z + 0.5f);
		return WorldPathfinder.TryResolveStandingCell(
			worldPresenter.WorldData,
			worldPresenter.VoxelRegistry,
			graphCache,
			approximateWorldPoint,
			out Vector3Int resolvedWorldCell)
			? resolvedWorldCell
			: worldCell;
	}

	private Vector3 ResolveActorWorldPoint(Vector3Int worldCell)
	{
		if (worldPresenter != null &&
			worldPresenter.WorldData != null &&
			worldPresenter.VoxelRegistry != null &&
			WorldPathfinder.TryGetStandingWorldPoint(
				worldPresenter.WorldData,
				worldPresenter.VoxelRegistry,
				worldCell,
				out Vector3 worldPoint))
		{
			return worldPoint;
		}

		return new Vector3(worldCell.x + 0.5f, worldCell.y, worldCell.z + 0.5f);
	}

	private MovablePathDriver GetOrCreateDriver(ActorPresenter p_presenter)
	{
		if (driversByPresenter.TryGetValue(p_presenter, out MovablePathDriver existingDriver) && existingDriver != null)
		{
			return existingDriver;
		}

		MovablePathDriver driver = new(p_presenter.ActorData);
		driversByPresenter[p_presenter] = driver;
		return driver;
	}
}
