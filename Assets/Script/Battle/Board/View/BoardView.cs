using UnityEngine;

namespace Battle.Board.View
{
	public class BoardView : MonoBehaviour
	{
		[SerializeField] private Material chunkMaterial = null;

		private void Awake()
		{
			
		}

		public void Configure(Material material)
		{
			chunkMaterial = material;
		}
	}
}