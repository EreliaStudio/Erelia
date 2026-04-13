using UnityEngine;

public class BoardPresenter : MonoBehaviour
{
	[SerializeField] private BoardView boardView;
	[SerializeField] private VoxelRegistry voxelRegistry;
	[SerializeField] private VoxelMaskRegistry voxelMaskRegistry;

	[SerializeField] private Board board = new Board();

	public Board Board => board;
	public BoardView View => boardView;

	public void Assign(Board targetBoard)
	{
		board = targetBoard;
		Rebuild();
	}

	public void Rebuild()
	{
		if (board == null)
		{
			return ;
		}	

		boardView.SetMaskMesh(VoxelMesher.BuildMaskMesh(board.Cells, board.MaskCells, voxelRegistry, voxelMaskRegistry));
	}

	private void Awake()
	{
		Rebuild();
	}
}
