using UnityEngine;

namespace Erelia.Battle
{
	public class Loader : MonoBehaviour
	{
		[SerializeField] private Board.Presenter presenter;

		[SerializeField] private Transform playerTransform;

		private void Awake()
		{
			if (presenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Loader] Battle board presenter is not assigned.");
			}

			BindFromContext();
		}

		private void BindFromContext()
		{
			Erelia.Battle.Data data = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Board.Model board = data != null ? data.Board : null;
			if (board == null)
			{
				Debug.LogWarning("[Erelia.Battle.Loader] Battle board model is null.");
				return;
			}

			if (presenter != null)
			{
				presenter.SetModel(board);
			}

			CenterPlayer(board);
		}

		private void CenterPlayer(Erelia.Battle.Board.Model board)
		{
			if (playerTransform == null || board == null)
			{
				return;
			}

			Vector3 position = playerTransform.position;
			playerTransform.position = new Vector3(
				board.SizeX * 0.5f,
				position.y,
				board.SizeZ * 0.5f);
		}
	}
}
