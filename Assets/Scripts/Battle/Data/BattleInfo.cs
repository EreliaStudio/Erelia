using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Derived battle info used during setup and placement.
	/// Stores the placement centers for player and enemy teams.
	/// </summary>
	[System.Serializable]
	public sealed class Info
	{
		/// <summary>
		/// Center cell used for player placement.
		/// </summary>
		public Vector2Int PlayerPlacementCenter;
		/// <summary>
		/// Center cell used for enemy placement.
		/// </summary>
		public Vector2Int EnemyPlacementCenter;
	}
}
