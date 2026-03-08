using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Scene loader that binds the battle board model to its presenter.
	/// Reads battle data from context, assigns the board, and centers the player view.
	/// </summary>
	public class Loader : MonoBehaviour
	{
		/// <summary>
		/// Presenter that displays the battle board.
		/// </summary>
		[SerializeField] private Board.Presenter presenter;

		/// <summary>
		/// Player transform to center on the board.
		/// </summary>
		[SerializeField] private Transform playerTransform;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Validate references and bind battle data.
			if (presenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Loader] Battle board presenter is not assigned.");
			}

			BindFromContext();
		}

		/// <summary>
		/// Binds the battle board model from context to the presenter.
		/// </summary>
		private void BindFromContext()
		{
			// Resolve battle data and apply the board model.
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

		/// <summary>
		/// Centers the player transform over the battle board.
		/// </summary>
		private void CenterPlayer(Erelia.Battle.Board.Model board)
		{
			// Move the player to the board center in X/Z.
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
