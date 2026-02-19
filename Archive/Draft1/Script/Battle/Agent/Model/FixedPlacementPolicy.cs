using UnityEngine;

namespace Battle.Agent.Model
{
	[System.Serializable]
	public class FixedPlacementPolicy : PlacementPolicyBase
	{
		[SerializeField] private Vector2Int fixedPosition = Vector2Int.zero;
		public Vector2Int FixedPosition => fixedPosition;

		public override Vector2Int GetPlacementPosition(PlacementAreas areas)
		{
			return fixedPosition;
		}
	}
}
