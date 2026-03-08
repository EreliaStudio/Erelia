using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase
{
	/// <summary>
	/// Shared phase-specific runtime info populated across the battle flow.
	/// </summary>
	[System.Serializable]
	public sealed class Info
	{
		[SerializeField] private List<Vector3Int> acceptableCoordinates = new List<Vector3Int>();

		/// <summary>
		/// Coordinates that are acceptable floor candidates for later phases.
		/// </summary>
		public IReadOnlyList<Vector3Int> AcceptableCoordinates => acceptableCoordinates;

		/// <summary>
		/// Clears all cached acceptable coordinates.
		/// </summary>
		public void ClearAcceptableCoordinates()
		{
			acceptableCoordinates.Clear();
		}

		/// <summary>
		/// Adds an acceptable coordinate to the cached list.
		/// </summary>
		public void AddAcceptableCoordinate(Vector3Int coordinate)
		{
			acceptableCoordinates.Add(coordinate);
		}
	}
}
