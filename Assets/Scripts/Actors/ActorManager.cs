using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActorManager : MonoBehaviour
{
	[SerializeField] private WorldPresenter worldPresenter;

	private readonly Dictionary<ActorPresenter, ActorPathDriver> driversByPresenter = new Dictionary<ActorPresenter, ActorPathDriver>();
	private readonly List<ActorPresenter> completedPresenters = new List<ActorPresenter>();
	private readonly WorldTraversalGraphCache graphCache = new WorldTraversalGraphCache();

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[ActorManager] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the ActorManager component.", Logger.Severity.Critical, this);
		}
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
		if (worldPresenter == null || worldPresenter.WorldData == null || worldPresenter.VoxelRegistry == null)
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

		GetOrCreateDriver(p_request.Actor).SetPath(path);
	}

	private ActorPathDriver GetOrCreateDriver(ActorPresenter p_presenter)
	{
		if (driversByPresenter.TryGetValue(p_presenter, out ActorPathDriver existingDriver) && existingDriver != null)
		{
			return existingDriver;
		}

		ActorPathDriver driver = new ActorPathDriver(p_presenter);
		driversByPresenter[p_presenter] = driver;
		return driver;
	}
}
