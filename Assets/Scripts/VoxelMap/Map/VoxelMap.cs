using System;
using UnityEngine;
using UnityEngine.Events;

public class VoxelMap : MonoBehaviour
{
	[SerializeField] private VoxelRegistry voxelDataRegistry;
	[SerializeField] private VoxelMapData data = new VoxelMapData();
	[SerializeField] private VoxelMapView view = new VoxelMapView();
	[SerializeField] private BattleAreaProfile bushAreaProfile;
	[SerializeField] private UnityEvent onPlayerEnterBush = new UnityEvent();
	[SerializeField] private UnityEvent onPlayerStayInBush = new UnityEvent();
	[SerializeField] private UnityEvent onPlayerExitBush = new UnityEvent();

	public VoxelMapData Data => data;
	public VoxelMapView View => view;
	public VoxelRegistry Registry => voxelDataRegistry;
	public BattleAreaProfile BushAreaProfile => bushAreaProfile;
	public event Action<BushTriggerContext> PlayerEnteredBush;
	public event Action<BushTriggerContext> PlayerStayInBush;
	public event Action<BushTriggerContext> PlayerExitBush;

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

	internal void NotifyPlayerEnteredBush(BushTriggerContext context)
	{
		PlayerEnteredBush?.Invoke(context);
		onPlayerEnterBush?.Invoke();
	}

	internal void NotifyPlayerStayInBush(BushTriggerContext context)
	{
		PlayerStayInBush?.Invoke(context);
		onPlayerStayInBush?.Invoke();
	}

	internal void NotifyPlayerExitBush(BushTriggerContext context)
	{
		PlayerExitBush?.Invoke(context);
		onPlayerExitBush?.Invoke();
	}
}
