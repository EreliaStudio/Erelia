using UnityEngine;

public class BoardPresenter : MonoBehaviour
{
	[SerializeField] private MaskOverlayView overlayView;
	[SerializeField] private VoxelRegistry voxelRegistry;
	[SerializeField] private VoxelMaskRegistry voxelMaskRegistry;

	[SerializeField] private BoardData boardData = new BoardData();
	private BoardOverlayState overlayState = new BoardOverlayState();

	public BoardData BoardData => boardData;
	public BoardOverlayState OverlayState => overlayState;
	public MaskOverlayView OverlayView => overlayView;

	public void Assign(BoardData targetBoardData)
	{
		boardData = targetBoardData;
		overlayState.Initialize(boardData);
		Rebuild();
	}

	public void RefreshOverlay()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		if (boardData == null || boardData.Terrain == null)
		{
			return;
		}

		if (overlayView == null)
		{
			return;
		}

		overlayView.SetMaskMesh(
			VoxelMesher.BuildMaskMesh(
				boardData.Terrain,
				overlayState?.MaskLayer,
				voxelRegistry,
				voxelMaskRegistry));
	}

	private void Awake()
	{
		if (overlayView == null)
		{
			Logger.LogError("[BoardPresenter] MaskOverlayView is not assigned in the inspector. Please assign a MaskOverlayView to the BoardPresenter component.", Logger.Severity.Critical, this);
		}

		if (voxelRegistry == null)
		{
			Logger.LogError("[BoardPresenter] VoxelRegistry is not assigned in the inspector. Please assign a VoxelRegistry to the BoardPresenter component.", Logger.Severity.Critical, this);
		}

		if (voxelMaskRegistry == null)
		{
			Logger.LogError("[BoardPresenter] VoxelMaskRegistry is not assigned in the inspector. Please assign a VoxelMaskRegistry to the BoardPresenter component.", Logger.Severity.Critical, this);
		}

		if (boardData != null)
		{
			boardData.AssignVoxelRegistry(voxelRegistry);
			overlayState.Initialize(boardData);
		}

		Rebuild();
	}
}
