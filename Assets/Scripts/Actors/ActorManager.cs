using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActorManager : MonoBehaviour
{
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private GameObject actorPrefab;

	private readonly Dictionary<ActorPresenter, ActorPathDriver> driversByPresenter = new();
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

	public ActorPresenter SpawnActor(Vector3 position, ActorData data, Transform parent = null)
	{
		if (actorPrefab == null)
		{
			return null;
		}

		GameObject instance = Instantiate(actorPrefab, position, Quaternion.identity, parent);
		if (!instance.TryGetComponent(out ActorPresenter presenter))
		{
			return null;
		}

		presenter.Bind(data);
		return presenter;
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

		foreach (KeyValuePair<ActorPresenter, ActorPathDriver> pair in driversByPresenter)
		{
			ActorPresenter presenter = pair.Key;
			ActorPathDriver driver = pair.Value;
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
		if (p_request.Actor == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
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

		GetOrCreateDriver(p_request.Actor).SetPath(path);
	}

	private ActorPathDriver GetOrCreateDriver(ActorPresenter p_presenter)
	{
		if (driversByPresenter.TryGetValue(p_presenter, out ActorPathDriver existingDriver) && existingDriver != null)
		{
			return existingDriver;
		}

		ActorPathDriver driver = new(p_presenter);
		driversByPresenter[p_presenter] = driver;
		return driver;
	}
}
