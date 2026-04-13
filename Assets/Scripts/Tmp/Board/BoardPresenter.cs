using UnityEngine;

public class BoardPresenter : MonoBehaviour
{
	[SerializeField] private BoardView boardView;
	[SerializeField] private VoxelRegistry voxelRegistry;
	[SerializeField] private VoxelMaskRegistry voxelMaskRegistry;

	[SerializeField] private BoardData boardData = new BoardData();

	public BoardData BoardData => boardData;
	public BoardView View => boardView;

	public void Assign(BoardData targetBoardData)
	{
		boardData = targetBoardData;
		Rebuild();
	}

	public void Rebuild()
	{
		if (boardData == null || boardData.Terrain == null)
		{
			return;
		}

		boardView.SetMaskMesh(
			VoxelMesher.BuildMaskMesh(
				boardData.Terrain.Cells,
				boardData.Terrain.MaskCells,
				voxelRegistry,
				voxelMaskRegistry));
	}

	private void Awake()
	{
		if (boardData != null)
		{
			boardData.AssignVoxelRegistry(voxelRegistry);
		}

		Rebuild();
	}
}