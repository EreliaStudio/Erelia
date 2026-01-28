using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelMapView
{
	[HideInInspector][SerializeField] private VoxelRegistry registry;
	[SerializeField] private Material chunkMaterial;
	[SerializeField] private Transform target;
	[SerializeField] private int viewRadius = 0;
	[SerializeField] private int verticalRadius = 0;
	[SerializeField] private int chunkCreateBudgetPerTick = 2;

	[HideInInspector] public List<ChunkCoord> VisibleCoords = new List<ChunkCoord>();

	[SerializeField] private VoxelRenderMeshBuilder renderMesher = new VoxelRenderMeshBuilder();
	[SerializeField] private VoxelSolidCollisionMeshBuilder solidCollisionMesher = new VoxelSolidCollisionMeshBuilder();
	[SerializeField] private VoxelBushTriggerMeshBuilder bushTriggerMesher = new VoxelBushTriggerMeshBuilder();
	private readonly Dictionary<ChunkCoord, ChunkView> views = new Dictionary<ChunkCoord, ChunkView>();
	private readonly HashSet<ChunkCoord> desiredCoords = new HashSet<ChunkCoord>();
	private readonly Queue<ChunkCoord> pendingCreate = new Queue<ChunkCoord>();
	private readonly HashSet<ChunkCoord> pendingSet = new HashSet<ChunkCoord>();
	private readonly List<ChunkCoord> desiredList = new List<ChunkCoord>();
	private ChunkCoord lastCenter;
	private bool hasCenter;
	private VoxelMapData mapData;
	private Transform owner;
	private VoxelMap ownerMap;

	public void Initialize(VoxelMapData data, VoxelRegistry registryValue, Transform ownerTransform)
	{
		mapData = data;
		owner = ownerTransform;
		ownerMap = ownerTransform != null ? ownerTransform.GetComponent<VoxelMap>() : null;
		SetRegistry(registryValue);
	}

	public void Tick()
	{
		if (mapData == null || owner == null)
		{
			return;
		}

		if (target == null && Camera.main != null)
		{
			target = Camera.main.transform;
		}

		if (target == null)
		{
			return;
		}

		ChunkCoord center = ChunkCoord.FromWorld(target.position);
		if (!hasCenter || !center.Equals(lastCenter))
		{
			lastCenter = center;
			hasCenter = true;
			UpdateVisible(center);
		}

		ProcessPendingCreation();
	}

	public void SetRegistry(VoxelRegistry value)
	{
		registry = value;
		renderMesher.SetRegistry(registry);
		solidCollisionMesher.SetRegistry(registry);
		bushTriggerMesher.SetRegistry(registry);
	}

	private void UpdateVisible(ChunkCoord center)
	{
		desiredCoords.Clear();
		pendingCreate.Clear();
		pendingSet.Clear();
		desiredList.Clear();

		int radiusSquared = viewRadius * viewRadius;

		for (int x = -viewRadius; x <= viewRadius; x++)
		{
			for (int z = -viewRadius; z <= viewRadius; z++)
			{
				int distanceSquared = (x * x) + (z * z);
				if (distanceSquared > radiusSquared)
				{
					continue;
				}

				for (int y = -verticalRadius; y <= verticalRadius; y++)
				{
					var coord = new ChunkCoord(center.X + x, center.Y + y, center.Z + z);
					desiredCoords.Add(coord);
					desiredList.Add(coord);
				}
			}
		}

		SortPendingByDistance(center);

		var toRemove = new List<ChunkCoord>();
		foreach (var pair in views)
		{
			if (!desiredCoords.Contains(pair.Key))
			{
				toRemove.Add(pair.Key);
			}
		}

		for (int i = 0; i < toRemove.Count; i++)
		{
			ChunkCoord coord = toRemove[i];
			if (views.TryGetValue(coord, out ChunkView view))
			{
				UnityEngine.Object.Destroy(view.gameObject);
			}
			views.Remove(coord);
		}

		VisibleCoords.Clear();
		VisibleCoords.AddRange(desiredCoords);
	}

	private void RebuildAllViews()
	{
		foreach (var pair in views)
		{
			if (pair.Value != null)
			{
				pair.Value.RebuildMesh();
			}
		}
	}

	private void SortPendingByDistance(ChunkCoord center)
	{
		if (desiredList.Count <= 1)
		{
			return;
		}

		desiredList.Sort((a, b) =>
		{
			int aDx = a.X - center.X;
			int aDz = a.Z - center.Z;
			int bDx = b.X - center.X;
			int bDz = b.Z - center.Z;

			int aDist = (aDx * aDx) + (aDz * aDz);
			int bDist = (bDx * bDx) + (bDz * bDz);
			return aDist.CompareTo(bDist);
		});

		for (int i = 0; i < desiredList.Count; i++)
		{
			ChunkCoord coord = desiredList[i];
			if (!views.ContainsKey(coord) && !pendingSet.Contains(coord))
			{
				pendingCreate.Enqueue(coord);
				pendingSet.Add(coord);
			}
		}
	}

	private void ProcessPendingCreation()
	{
		if (chunkCreateBudgetPerTick <= 0 || pendingCreate.Count == 0)
		{
			return;
		}

		int created = 0;
		while (created < chunkCreateBudgetPerTick && pendingCreate.Count > 0)
		{
			ChunkCoord coord = pendingCreate.Dequeue();
			pendingSet.Remove(coord);

			if (!desiredCoords.Contains(coord) || views.ContainsKey(coord))
			{
				continue;
			}

			Chunk chunk = mapData.GetOrCreateChunk(coord);
			ChunkView view = CreateChunkView(coord, chunk);
			views.Add(coord, view);
			created++;
		}
	}

	private ChunkView CreateChunkView(ChunkCoord coord, Chunk chunk)
	{
		var go = new GameObject("Chunk " + coord);
		go.transform.SetParent(owner, false);
		go.transform.localPosition = new Vector3(
			coord.X * Chunk.SizeX,
			coord.Y * Chunk.SizeY,
			coord.Z * Chunk.SizeZ);

		ChunkView view = go.AddComponent<ChunkView>();
		view.Initialize(coord, chunk, renderMesher, solidCollisionMesher, bushTriggerMesher, chunkMaterial, ownerMap);
		return view;
	}
}
