using System;
using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Serializable registry holding the configured battle phase instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This registry is a Unity-serializable container (via <see cref="SerializableAttribute"/> and
	/// <see cref="SerializeField"/>) that owns one instance per battle phase.
	/// </para>
	/// <para>
	/// The <see cref="BattleManager"/> (or other systems) can resolve phases by id using
	/// <see cref="TryGetPhase(BattlePhaseId, out BattlePhase)"/>.
	/// </para>
	/// <para>
	/// This implementation uses an explicit <c>switch</c> to map <see cref="BattlePhaseId"/> values to fields.
	/// This keeps phase ownership centralized and avoids maintaining a separate runtime dictionary.
	/// </para>
	/// </remarks>
	[Serializable]
	public sealed class BattlePhaseRegistry
	{
		/// <summary>
		/// Instance of the <see cref="InitializePhase"/> phase.
		/// </summary>
		/// <remarks>
		/// Serialized so it can be configured in the Unity Inspector and persisted with the owning object.
		/// </remarks>
		[SerializeField] private InitializePhase initialize = new InitializePhase();

		/// <summary>
		/// Instance of the <see cref="PlacementPhase"/> phase.
		/// </summary>
		[SerializeField] private PlacementPhase placement = new PlacementPhase();

		/// <summary>
		/// Instance of the <see cref="PlayerTurnPhase"/> phase.
		/// </summary>
		[SerializeField] private PlayerTurnPhase playerTurn = new PlayerTurnPhase();

		/// <summary>
		/// Instance of the <see cref="EnemyTurnPhase"/> phase.
		/// </summary>
		[SerializeField] private EnemyTurnPhase enemyTurn = new EnemyTurnPhase();

		/// <summary>
		/// Instance of the <see cref="ResolveActionPhase"/> phase.
		/// </summary>
		[SerializeField] private ResolveActionPhase resolveAction = new ResolveActionPhase();

		/// <summary>
		/// Instance of the <see cref="VictoryPhase"/> phase.
		/// </summary>
		[SerializeField] private VictoryPhase victory = new VictoryPhase();

		/// <summary>
		/// Instance of the <see cref="DefeatPhase"/> phase.
		/// </summary>
		[SerializeField] private DefeatPhase defeat = new DefeatPhase();

		/// <summary>
		/// Instance of the <see cref="CleanupPhase"/> phase.
		/// </summary>
		[SerializeField] private CleanupPhase cleanup = new CleanupPhase();

		/// <summary>
		/// Tries to resolve a phase instance by its identifier.
		/// </summary>
		/// <param name="id">Identifier of the phase to retrieve.</param>
		/// <param name="phase">
		/// When this method returns, contains the resolved phase instance if found; otherwise <c>null</c>.
		/// </param>
		/// <returns>
		/// <c>true</c> if <paramref name="id"/> maps to a non-null configured phase; otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This method does not allocate and performs a direct mapping via a <c>switch</c>.
		/// </para>
		/// <para>
		/// Returning <c>false</c> indicates either:
		/// <list type="bullet">
		/// <item><description><paramref name="id"/> is <see cref="BattlePhaseId.None"/> or unknown.</description></item>
		/// <item><description>The mapped field is currently <c>null</c>.</description></item>
		/// </list>
		/// </para>
		/// </remarks>
		public bool TryGetPhase(BattlePhaseId id, out BattlePhase phase)
		{
			// Map the id to the corresponding serialized field.
			switch (id)
			{
				case BattlePhaseId.Initialize:
					phase = initialize;
					return phase != null;

				case BattlePhaseId.Placement:
					phase = placement;
					return phase != null;

				case BattlePhaseId.PlayerTurn:
					phase = playerTurn;
					return phase != null;

				case BattlePhaseId.EnemyTurn:
					phase = enemyTurn;
					return phase != null;

				case BattlePhaseId.ResolveAction:
					phase = resolveAction;
					return phase != null;

				case BattlePhaseId.Victory:
					phase = victory;
					return phase != null;

				case BattlePhaseId.Defeat:
					phase = defeat;
					return phase != null;

				case BattlePhaseId.Cleanup:
					phase = cleanup;
					return phase != null;

				default:
					// Unknown id (or None): resolve failure.
					phase = null;
					return false;
			}
		}
	}
}