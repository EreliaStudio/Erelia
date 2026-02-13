using UnityEngine;
using Utils;

namespace Battle.Board
{
	public class Manager : MonoBehaviour
	{
		[SerializeField] private Material voxelMaterial = null;
		[SerializeField] private Material cellMaskMaterial = null;

		private Battle.Board.View.Presenter boardPresenter = null;
		private Battle.Board.Controller.BodyCollider boardCollider = null;

		private void Awake()
		{
			InitializeBoardPresenter();
			InitializeBoardCollider();
		}

		private void InitializeBoardPresenter()
		{
			var go = new GameObject("BoardPresenter");
			go.transform.SetParent(transform, false);
			boardPresenter = go.AddComponent<Board.View.Presenter>();
			boardPresenter.Initialize(voxelMaterial, cellMaskMaterial);
		}

		private void InitializeBoardCollider()
		{
			var go = new GameObject("BoardCollider");
			go.transform.SetParent(transform, false);
			boardCollider = go.AddComponent<Board.Controller.BodyCollider>();
		}

		public void Refresh(Battle.Board.Model.Data data)
		{
			boardPresenter.Rebuild(data);
			boardCollider.Rebuild(data);
		}
	}
}
