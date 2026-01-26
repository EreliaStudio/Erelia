using UnityEngine;

public class VoxelMap : MonoBehaviour
{
	[SerializeField] private VoxelRegistry voxelDataRegistry;
	[SerializeField] private VoxelMapData data = new VoxelMapData();
	[SerializeField] private VoxelMapView view = new VoxelMapView();

	public VoxelMapData Data => data;
	public VoxelMapView View => view;
	public VoxelRegistry Registry => voxelDataRegistry;

	private void Awake()
	{
		PropagateRegistry();
		view.Initialize(data, voxelDataRegistry, transform);
	}

	private void OnValidate()
	{
		PropagateRegistry();
		if (view != null)
		{
			view.Initialize(data, voxelDataRegistry, transform);
		}
	}

	private void Update()
	{
		if (view != null)
		{
			view.Tick();
		}
	}

	private void PropagateRegistry()
	{
		if (data != null)
		{
			data.SetRegistry(voxelDataRegistry);
		}

		if (view != null)
		{
			view.SetRegistry(voxelDataRegistry);
		}
	}
}
