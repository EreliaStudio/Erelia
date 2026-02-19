using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Agent.Model
{
	public enum PlacementLine
	{
		FrontLine,
		MiddleLine,
		BackLine
	}

	public sealed class PlacementAreas
	{
		public IReadOnlyList<Vector2Int> FrontLine { get; }
		public IReadOnlyList<Vector2Int> MiddleLine { get; }
		public IReadOnlyList<Vector2Int> BackLine { get; }

		public PlacementAreas(
			IReadOnlyList<Vector2Int> frontLine,
			IReadOnlyList<Vector2Int> middleLine,
			IReadOnlyList<Vector2Int> backLine)
		{
			FrontLine = frontLine ?? Array.Empty<Vector2Int>();
			MiddleLine = middleLine ?? Array.Empty<Vector2Int>();
			BackLine = backLine ?? Array.Empty<Vector2Int>();
		}

		public IReadOnlyList<Vector2Int> GetLine(PlacementLine line)
		{
			switch (line)
			{
				case PlacementLine.FrontLine:
					return FrontLine;
				case PlacementLine.MiddleLine:
					return MiddleLine;
				case PlacementLine.BackLine:
					return BackLine;
				default:
					return Array.Empty<Vector2Int>();
			}
		}
	}

	[System.Serializable]
	public abstract class PlacementPolicyBase
	{
		public abstract Vector2Int GetPlacementPosition(PlacementAreas areas);
	}
}
