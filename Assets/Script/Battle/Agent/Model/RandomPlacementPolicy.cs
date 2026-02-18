using UnityEngine;

namespace Battle.Agent.Model
{
	[System.Serializable]
	public class RandomPlacementPolicy : PlacementPolicyBase
	{
		[SerializeField] private PlacementLine line = PlacementLine.MiddleLine;
		public PlacementLine Line => line;

		public override Vector2Int GetPlacementPosition(PlacementAreas areas)
		{
			if (areas == null)
			{
				Debug.LogWarning("RandomPlacementPolicy: PlacementAreas was null. Returning Vector2Int.zero.");
				return Vector2Int.zero;
			}

			var lineCells = areas.GetLine(line);
			if (lineCells == null || lineCells.Count == 0)
			{
				Debug.LogWarning($"RandomPlacementPolicy: No placement cells available for {line}. Returning Vector2Int.zero.");
				return Vector2Int.zero;
			}

			int index = UnityEngine.Random.Range(0, lineCells.Count);
			return lineCells[index];
		}
	}
}
