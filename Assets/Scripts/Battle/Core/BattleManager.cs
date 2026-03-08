using UnityEngine;
using PhaseId = Erelia.Battle.Phase.Id;
using PhaseRegistry = Erelia.Battle.Phase.Registry;
using PhaseRoot = Erelia.Battle.Phase.Root;

namespace Erelia.Battle
{
	/// <summary>
	/// Controls the battle phase state machine.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This component owns the active <see cref="Erelia.Battle.Phase.Root"/> and drives it each frame via
	/// <see cref="Erelia.Battle.Phase.Root.Tick(BattleManager, float)"/>.
	/// </para>
	/// <para>
	/// Phase transitions are requested through <see cref="RequestTransition(PhaseId)"/> and are applied
	/// on the next <see cref="Update"/> tick to avoid changing phases in the middle of another system's frame logic.
	/// </para>
	/// <para>
	/// Phase instances are resolved from a <see cref="Erelia.Battle.Phase.Registry"/> (single source of truth).
	/// </para>
	/// </remarks>
	public sealed class BattleManager : MonoBehaviour
	{
		/// <summary>
		/// Phase to start when the battle begins.
		/// </summary>
		/// <remarks>
		/// If set to <see cref="PhaseId.None"/>, no phase is entered automatically.
		/// </remarks>
		[SerializeField] private PhaseId startPhase = PhaseId.Initialize;

		/// <summary>
		/// Registry containing all configured battle phases.
		/// </summary>
		/// <remarks>
		/// The manager resolves phase instances from this registry by calling
		/// <see cref="PhaseRegistry.TryGetPhase(PhaseId, out PhaseRoot)"/>.
		/// </remarks>
		[SerializeField] private PhaseRegistry phaseRegistry = new PhaseRegistry();
		/// <summary>
		/// Player controller used to forward phase input handlers.
		/// </summary>
		[SerializeField] private Erelia.Battle.Player.BattlePlayerController playerController;

		/// <summary>
		/// Currently active phase instance.
		/// </summary>
		/// <remarks>
		/// This is <c>null</c> until the first successful transition occurs.
		/// </remarks>
		private PhaseRoot currentPhase;

		/// <summary>
		/// Pending phase transition requested for the next update.
		/// </summary>
		/// <remarks>
		/// The transition is deferred to the next <see cref="Update"/> to ensure phase changes happen at a stable time.
		/// </remarks>
		private PhaseId? pendingPhase;

		/// <summary>
		/// Gets the identifier of the currently active phase.
		/// </summary>
		/// <remarks>
		/// Returns <see cref="PhaseId.None"/> if no phase is currently active.
		/// </remarks>
		public PhaseId CurrentPhaseId => currentPhase != null ? currentPhase.Id : PhaseId.None;

		/// <summary>
		/// Gets the currently active phase instance.
		/// </summary>
		/// <remarks>
		/// This can be <c>null</c> if the manager has not entered any phase yet.
		/// </remarks>
		public PhaseRoot CurrentPhase => currentPhase;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		/// <remarks>
		/// Enters <see cref="startPhase"/> immediately if it is not <see cref="PhaseId.None"/>.
		/// </remarks>
		private void Awake()
		{
			if (playerController == null)
			{
				Debug.LogWarning("[Erelia.Battle.BattleManager] Battle player controller is not assigned.", this);
			}

			// If a start phase is configured, enter it right away.
			if (startPhase != PhaseId.None)
			{
				TransitionTo(startPhase);
			}
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		/// <remarks>
		/// Applies any pending transition first, then ticks the active phase.
		/// </remarks>
		private void Update()
		{
			// Apply a deferred transition (if any) at the start of the frame.
			if (pendingPhase.HasValue)
			{
				// Capture and clear first to prevent re-entrancy issues if TransitionTo triggers another request.
				PhaseId next = pendingPhase.Value;
				pendingPhase = null;

				// Enter the requested phase immediately.
				TransitionTo(next);
			}

			// Tick the active phase, providing deltaTime for time-based logic.
			currentPhase?.Tick(this, Time.deltaTime);
		}

		/// <summary>
		/// Requests a transition to another phase on the next update tick.
		/// </summary>
		/// <param name="next">Target phase identifier.</param>
		/// <returns>
		/// <c>true</c> if the transition request was accepted and queued; otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// This method only schedules the transition; the transition is executed in <see cref="Update"/>.
		/// </remarks>
		public bool RequestTransition(PhaseId next)
		{
			// Reject invalid target.
			if (next == PhaseId.None)
			{
				Debug.LogWarning("[Erelia.Battle.BattleManager] Cannot transition to None.", this);
				return false;
			}

			// Reject if already in that phase.
			if (next == CurrentPhaseId)
			{
				return false;
			}

			// Reject if the registry is missing or does not contain the target phase.
			if (phaseRegistry == null || !phaseRegistry.TryGetPhase(next, out _))
			{
				Debug.LogWarning($"[Erelia.Battle.BattleManager] Phase {next} not registered.", this);
				return false;
			}

			// Queue the transition for the next frame.
			pendingPhase = next;
			return true;
		}

		/// <summary>
		/// Tries to resolve a phase instance by id.
		/// </summary>
		/// <param name="id">Phase identifier to resolve.</param>
		/// <param name="phase">Resolved phase instance if found; otherwise <c>null</c>.</param>
		/// <returns>
		/// <c>true</c> if the phase exists in the registry; otherwise <c>false</c>.
		/// </returns>
		public bool TryGetPhase(PhaseId id, out PhaseRoot phase)
		{
			// If the registry is missing, nothing can be resolved.
			if (phaseRegistry == null)
			{
				phase = null;
				return false;
			}

			// Delegate lookup to the registry (single source of truth).
			return phaseRegistry.TryGetPhase(id, out phase);
		}

		/// <summary>
		/// Immediately transitions to the specified phase.
		/// </summary>
		/// <param name="next">Target phase identifier.</param>
		/// <remarks>
		/// Calls <see cref="Erelia.Battle.Phase.Root.Exit(BattleManager)"/> on the current phase (if any),
		/// switches <see cref="currentPhase"/>, then calls <see cref="Erelia.Battle.Phase.Root.Enter(BattleManager)"/> on the target.
		/// </remarks>
		private void TransitionTo(PhaseId next)
		{
			// Ignore invalid target.
			if (next == PhaseId.None)
			{
				return;
			}

			// Resolve the target phase from the registry.
			if (phaseRegistry == null || !phaseRegistry.TryGetPhase(next, out PhaseRoot target))
			{
				Debug.LogWarning($"[Erelia.Battle.BattleManager] Phase {next} not registered.", this);
				return;
			}

			// If the resolved target is already active, do nothing.
			if (currentPhase == target)
			{
				return;
			}

			// Notify the current phase it is being exited.
			currentPhase?.Exit(this);

			// Activate the new phase.
			currentPhase = target;

			playerController.SetPhaseController(currentPhase);

			// Notify the new phase it is being entered.
			currentPhase.Enter(this);
		}
	}
}
