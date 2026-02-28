using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	public sealed class BattleManager : MonoBehaviour
	{
		[SerializeField] private BattlePhaseId startPhase = BattlePhaseId.Initialize;
		[SerializeField] private Erelia.Battle.BattlePhaseRegistry phaseRegistry = new Erelia.Battle.BattlePhaseRegistry();

		private readonly Dictionary<BattlePhaseId, BattlePhase> phaseLookup = new Dictionary<BattlePhaseId, BattlePhase>();
		private BattlePhase currentPhase;
		private BattlePhaseId? pendingPhase;

		public BattlePhaseId CurrentPhaseId => currentPhase != null ? currentPhase.Id : BattlePhaseId.None;
		public BattlePhase CurrentPhase => currentPhase;

		private void Awake()
		{
			RebuildPhaseLookup();

			if (startPhase != BattlePhaseId.None)
			{
				TransitionTo(startPhase);
			}
		}

		private void Update()
		{
			if (pendingPhase.HasValue)
			{
				BattlePhaseId next = pendingPhase.Value;
				pendingPhase = null;
				TransitionTo(next);
			}

			currentPhase?.Tick(this, Time.deltaTime);
		}

		public void RebuildPhaseLookup()
		{
			phaseLookup.Clear();

			Erelia.Battle.BattlePhaseRegistry registry = phaseRegistry;
			if (registry == null)
			{
				Debug.LogWarning("[Erelia.Battle.BattleManager] Phase registry is missing.", this);
				return;
			}

			BattlePhase[] phases = registry.GetAllPhases();
			for (int i = 0; i < phases.Length; i++)
			{
				BattlePhase phase = phases[i];
				if (phase == null)
				{
					continue;
				}

				if (phaseLookup.ContainsKey(phase.Id))
				{
					Debug.LogWarning($"[Erelia.Battle.BattleManager] Duplicate phase id {phase.Id}.", this);
					continue;
				}

				phaseLookup.Add(phase.Id, phase);
			}
		}

		public bool RequestTransition(BattlePhaseId next)
		{
			if (next == BattlePhaseId.None)
			{
				Debug.LogWarning("[Erelia.Battle.BattleManager] Cannot transition to None.", this);
				return false;
			}

			if (next == CurrentPhaseId)
			{
				return false;
			}

			if (!phaseLookup.ContainsKey(next))
			{
				Debug.LogWarning($"[Erelia.Battle.BattleManager] Phase {next} not registered.", this);
				return false;
			}

			pendingPhase = next;
			return true;
		}

		public bool TryGetPhase(BattlePhaseId id, out BattlePhase phase)
		{
			return phaseLookup.TryGetValue(id, out phase);
		}

		private void TransitionTo(BattlePhaseId next)
		{
			if (next == BattlePhaseId.None)
			{
				return;
			}

			if (!phaseLookup.TryGetValue(next, out BattlePhase target))
			{
				Debug.LogWarning($"[Erelia.Battle.BattleManager] Phase {next} not registered.", this);
				return;
			}

			if (currentPhase == target)
			{
				return;
			}

			currentPhase?.Exit(this);
			currentPhase = target;
			currentPhase.Enter(this);
		}
	}
}
