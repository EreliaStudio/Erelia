using UnityEngine;

[RequireComponent(typeof(PlayerView))]
[DisallowMultipleComponent]
public class PlayerPresenter : MonoBehaviour
{
	[SerializeField] private PlayerData playerData = new PlayerData();
	[SerializeField] private PlayerView playerView;
	[SerializeField] private Transform playerCamera;
	[SerializeField] private Vector3 cameraTargetLocalPoint = Vector3.zero;
	[SerializeField] private bool controlCameraLook = true;

	private Vector3 lastWorldPosition;
	private ChunkCoordinates lastChunkCoordinates;

	public PlayerData PlayerData => playerData;
	public PlayerView PlayerView => playerView;

	private void Reset()
	{
		if (playerView == null)
		{
			playerView = GetComponent<PlayerView>();
		}

		if (playerCamera == null)
		{
			Camera childCamera = GetComponentInChildren<Camera>();
			if (childCamera != null)
			{
				playerCamera = childCamera.transform;
			}
		}
	}

	private void Awake()
	{
		InitializeFromTransform();
	}

	private void Start()
	{
		EventCenter.EmitPlayerMoved(lastWorldPosition);
		EventCenter.EmitPlayerChunkChanged(lastChunkCoordinates);
	}

	private void Update()
	{
		Vector3 currentWorldPosition = transform.position;
		ChunkCoordinates currentChunkCoordinates = ChunkCoordinates.FromWorldPosition(currentWorldPosition);

		if (currentWorldPosition != lastWorldPosition)
		{
			lastWorldPosition = currentWorldPosition;
			playerData.CellPosition = Vector3Int.FloorToInt(currentWorldPosition);
			EventCenter.EmitPlayerMoved(currentWorldPosition);
		}

		if (!currentChunkCoordinates.Equals(lastChunkCoordinates))
		{
			lastChunkCoordinates = currentChunkCoordinates;
			playerData.ChunkCoordinates = currentChunkCoordinates;
			EventCenter.EmitPlayerChunkChanged(currentChunkCoordinates);
		}
	}

	private void LateUpdate()
	{
		UpdateCameraLook();
	}

	private void InitializeFromTransform()
	{
		lastWorldPosition = transform.position;
		lastChunkCoordinates = ChunkCoordinates.FromWorldPosition(lastWorldPosition);
		playerData.CellPosition = Vector3Int.FloorToInt(lastWorldPosition);
		playerData.ChunkCoordinates = lastChunkCoordinates;
	}

	private void UpdateCameraLook()
	{
		if (!controlCameraLook || playerCamera == null)
		{
			return;
		}

		Vector3 localCameraPosition = transform.InverseTransformPoint(playerCamera.position);
		Vector3 localDirection = cameraTargetLocalPoint - localCameraPosition;
		if (localDirection.sqrMagnitude <= 0.0001f)
		{
			return;
		}

		Vector3 worldDirection = transform.TransformDirection(localDirection.normalized);
		playerCamera.rotation = Quaternion.LookRotation(worldDirection, transform.up);
	}
}
