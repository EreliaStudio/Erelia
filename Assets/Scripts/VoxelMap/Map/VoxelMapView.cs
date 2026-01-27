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

	[HideInInspector] public List<ChunkCoord> VisibleCoords = new List<ChunkCoord>();

	[SerializeField] private ChunkRenderMeshBuilder renderMesher = new ChunkRenderMeshBuilder();
	[SerializeField] private ChunkSolidCollisionMeshBuilder solidCollisionMesher = new ChunkSolidCollisionMeshBuilder();
	[SerializeField] private ChunkBushTriggerMeshBuilder bushTriggerMesher = new ChunkBushTriggerMeshBuilder();
	private readonly Dictionary<ChunkCoord, ChunkView> views = new Dictionary<ChunkCoord, ChunkView>();
	private ChunkCoord lastCenter;
	private bool hasCenter;
	private VoxelMapData mapData;
	private Transform owner;

	public void Initialize(VoxelMapData data, VoxelRegistry registryValue, Transform ownerTransform)
	{
		mapData = data;
		owner = ownerTransform;
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
		var desired = new HashSet<ChunkCoord>();

		for (int x = -viewRadius; x <= viewRadius; x++)
		{
			for (int z = -viewRadius; z <= viewRadius; z++)
			{
				for (int y = -verticalRadius; y <= verticalRadius; y++)
				{
					var coord = new ChunkCoord(center.X + x, center.Y + y, center.Z + z);
					desired.Add(coord);
					if (!views.ContainsKey(coord))
					{
						Chunk chunk = mapData.GetOrCreateChunk(coord);
						ChunkView view = CreateChunkView(coord, chunk);
						views.Add(coord, view);
					}
				}
			}
		}

		var toRemove = new List<ChunkCoord>();
		foreach (var pair in views)
		{
			if (!desired.Contains(pair.Key))
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
		VisibleCoords.AddRange(desired);
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
		view.Initialize(coord, chunk, renderMesher, solidCollisionMesher, bushTriggerMesher, chunkMaterial);
		return view;
	}
}
