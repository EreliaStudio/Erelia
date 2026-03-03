using UnityEngine;

namespace Erelia.Core.Encounter
{
	/// <summary>
	/// Serializable configuration describing how and which encounters can spawn in a zone/biome.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An encounter table typically controls:
	/// <list type="bullet">
	/// <item><description><see cref="EncounterChance"/>: probability to attempt an encounter (0..1).</description></item>
	/// <item><description>Spawn distribution parameters (<see cref="BaseRadius"/>, <see cref="NoiseAmplitude"/>, <see cref="NoiseScale"/>, <see cref="NoiseSeed"/>).</description></item>
	/// <item><description><see cref="PlacementRadius"/>: minimum spacing where each team will be able to place itself.</description></item>
	/// <item><description><see cref="Teams"/>: weighted selection of teams to spawn, referenced by a team JSON path.</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// JSON format (Unity <see cref="JsonUtility"/>) example:
	/// </para>
	/// <code>
	/// {
	///   "EncounterChance": 0.25,
	///   "BaseRadius": 10,
	///   "NoiseAmplitude": 4,
	///   "NoiseScale": 0.15,
	///   "NoiseSeed": 1337,
	///   "PlacementRadius": 3,
	///   "Teams": [
	///     { "TeamPath": "Encounters/Teams/Forest_Team_A.json", "Weight": 70 },
	///     { "TeamPath": "Encounters/Teams/Forest_Team_B.json", "Weight": 30 }
	///   ]
	/// }
	/// </code>
	/// <para>
	/// Notes:
	/// <list type="bullet">
	/// <item><description>Property names match the public field names (Unity JSON is case-sensitive).</description></item>
	/// <item><description><see cref="Teams"/> entries reference external team files (see <c>Erelia.Core.Creature.Team</c> JSON).</description></item>
	/// <item><description>Serialization is handled externally via <see cref="JsonUtility"/>.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	[System.Serializable]
	public sealed class EncounterTable
	{
		/// <summary>
		/// Weighted reference to a team definition used by this encounter table.
		/// </summary>
		/// <remarks>
		/// Each entry points to a serialized team (usually JSON) and specifies its relative selection weight.
		/// </remarks>
		[System.Serializable]
		public struct TeamEntry
		{
			/// <summary>
			/// Path to the team data (typically a JSON file path resolved via <c>PathUtils</c>).
			/// </summary>
			public string TeamPath;

			/// <summary>
			/// Relative weight used during random selection among <see cref="Teams"/>.
			/// </summary>
			/// <remarks>
			/// Higher values make the team more likely to be chosen.
			/// </remarks>
			[Min(1)]
			public int Weight;
		}

		/// <summary>
		/// Probability (0..1) that an encounter attempt succeeds when evaluated.
		/// </summary>
		[Range(0f, 1f)]
		public float EncounterChance;

		/// <summary>
		/// Base radius parameter used by the battle board size.
		/// </summary>
		public int BaseRadius = 10;

		/// <summary>
		/// Amplitude applied to noise-driven variation around <see cref="BaseRadius"/>.
		/// </summary>
		public int NoiseAmplitude = 4;

		/// <summary>
		/// Scale of the noise sampling used for distribution (higher = smoother, lower = more variation).
		/// </summary>
		public float NoiseScale = 0.15f;

		/// <summary>
		/// Seed used to make the noise field deterministic.
		/// </summary>
		public int NoiseSeed = 1337;

		/// <summary>
		/// Minimum spacing radius used when placing encounter points.
		/// </summary>
		[Min(1)]
		public int PlacementRadius = 3;

		/// <summary>
		/// Weighted list of possible teams that can be selected for an encounter.
		/// </summary>
		public TeamEntry[] Teams;

	}
}
