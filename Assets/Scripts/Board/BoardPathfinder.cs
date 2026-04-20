using UnityEngine;

public static class BoardPathfinder
{
	public static bool TryResolveSelectableTarget(BoardData p_boardData, Vector3Int p_localPosition, out Vector3Int p_targetLocalPosition)
	{
		p_targetLocalPosition = default;

		if (p_boardData == null)
		{
			return false;
		}

		int clampedY = Mathf.Clamp(p_localPosition.y, 0, p_boardData.Terrain.SizeY - 1);
		for (int y = clampedY; y >= 0; y--)
		{
			Vector3Int candidate = new Vector3Int(p_localPosition.x, y, p_localPosition.z);
			if (!p_boardData.IsStandable(candidate))
			{
				continue;
			}

			p_targetLocalPosition = candidate;
			return true;
		}

		return false;
	}
}
