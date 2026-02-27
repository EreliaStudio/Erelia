using UnityEngine;

namespace Erelia.Battle
{
	public class Loader : MonoBehaviour
	{
		[SerializeField] Board.Presenter presenter;
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
			var context = Erelia.Context.Instance;

			if (context.PendingBattleBoard == null)
			{
				Debug.LogWarning("[Erelia.Battle.Loader] Battle board model is null.");
				return;
			}

			presenter.SetModel(context.PendingBattleBoard);
			CenterPlayer(context.PendingBattleBoard);
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
