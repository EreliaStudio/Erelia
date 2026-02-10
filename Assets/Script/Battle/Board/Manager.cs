using UnityEngine;

namespace Battle.Board
{
	public class Manager : MonoBehaviour
	{
		[SerializeField] private Material voxelMaterial = null;
		[SerializeField] private Material cellMaskMaterial = null;

		private Battle.Board.View.BoardView boardView = null;
		private Battle.Board.Controller.BoardController boardController = null;

		private void Awake()
		{
			InitializeBoardView();
			InitializeBoardController();
		}

		private void InitializeBoardView()
		{
			var go = new GameObject("BoardView");
			go.transform.SetParent(transform, false);
			boardView = go.AddComponent<Board.View.BoardView>();
			boardView.Configure(voxelMaterial, cellMaskMaterial);
		}

		private void InitializeBoardController()
		{
			var go = new GameObject("BoardController");
			go.transform.SetParent(transform, false);
			boardController = go.AddComponent<Board.Controller.BoardController>();
			boardController.Configure();
		}
	}
}