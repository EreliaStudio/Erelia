using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Erelia.Battle.Phase
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
	/// <see cref="TryGetPhase(Id, out Root)"/>.
	/// </para>
	/// <para>
	/// This implementation uses an explicit <c>switch</c> to map <see cref="Id"/> values to fields.
	/// This keeps phase ownership centralized and avoids maintaining a separate runtime dictionary.
	/// </para>
	/// </remarks>
	[Serializable]
	[MovedFrom(true, sourceNamespace: "Erelia.Battle", sourceAssembly: "Assembly-CSharp", sourceClassName: "BattlePhaseRegistry")]
	public sealed class Registry
	{
		/// <summary>
		/// Instance of the initialize phase.
		/// </summary>
		/// <remarks>
		/// Serialized so it can be configured in the Unity Inspector and persisted with the owning object.
		/// </remarks>
		[SerializeField] private Erelia.Battle.Phase.Initialize.MainRoot initialize = new Erelia.Battle.Phase.Initialize.MainRoot();

		/// <summary>
		/// Instance of the placement phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.Phase.Placement.MainRoot placement = new Erelia.Battle.Phase.Placement.MainRoot();

		/// <summary>
		/// Instance of the player turn phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.Phase.PlayerTurn.MainRoot playerTurn = new Erelia.Battle.Phase.PlayerTurn.MainRoot();

		/// <summary>
		/// Instance of the enemy turn phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.Phase.EnemyTurn.MainRoot enemyTurn = new Erelia.Battle.Phase.EnemyTurn.MainRoot();

		/// <summary>
		/// Instance of the resolve action phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.Phase.ResolveAction.MainRoot resolveAction = new Erelia.Battle.Phase.ResolveAction.MainRoot();

		/// <summary>
		/// Instance of the victory phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.Phase.Victory.MainRoot victory = new Erelia.Battle.Phase.Victory.MainRoot();

		/// <summary>
		/// Instance of the defeat phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.Phase.Defeat.MainRoot defeat = new Erelia.Battle.Phase.Defeat.MainRoot();

		/// <summary>
		/// Instance of the cleanup phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.Phase.Cleanup.MainRoot cleanup = new Erelia.Battle.Phase.Cleanup.MainRoot();

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
		/// <item><description><paramref name="id"/> is <see cref="Id.None"/> or unknown.</description></item>
		/// <item><description>The mapped field is currently <c>null</c>.</description></item>
		/// </list>
		/// </para>
		/// </remarks>
		public bool TryGetPhase(Id id, out Root phase)
		{
			// Map the id to the corresponding serialized field.
			switch (id)
			{
				case Id.Initialize:
					phase = initialize;
					return phase != null;

				case Id.Placement:
					phase = placement;
					return phase != null;

				case Id.PlayerTurn:
					phase = playerTurn;
					return phase != null;

				case Id.EnemyTurn:
					phase = enemyTurn;
					return phase != null;

				case Id.ResolveAction:
					phase = resolveAction;
					return phase != null;

				case Id.Victory:
					phase = victory;
					return phase != null;

				case Id.Defeat:
					phase = defeat;
					return phase != null;

				case Id.Cleanup:
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
