using UnityEngine;

namespace Erelia.Battle.Unit
{
	/// <summary>
	/// Resolves world positions for staged units displayed beside the board.
	/// </summary>
	public static class StagingPositionUtility
	{
		private const float ReserveSpacingZ = 2f;

		public static Vector3 ResolveWorldPosition(
			Erelia.Battle.Board.Presenter boardPresenter,
			Erelia.Battle.Board.Model board,
			Erelia.Battle.Unit.Team team,
			int teamIndex,
			int teamCount,
			float reserveSideOffset,
			float reserveHeight)
		{
			if (board == null)
			{
				return Vector3.zero;
			}

			float x = -reserveSideOffset;
			float z = Mathf.Max(0, teamIndex) * ReserveSpacingZ;
			Vector3 localPosition = new Vector3(x, reserveHeight, z);
			return boardPresenter != null
				? boardPresenter.transform.TransformPoint(localPosition)
				: localPosition;
		}
	}
}
