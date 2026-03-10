using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Initialize
{
	/// <summary>
		/// Initialization phase that prepares battle data.
	/// Resolves the enemy team, computes acceptable floor coordinates, partitions player/enemy placement areas,
	/// then transitions to the Placement phase.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		/// <summary>
		/// Whether initialization is still pending.
		/// </summary>
		private bool pendingSetup;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Initialize;

		/// <summary>
		/// Enters the initialize phase and prepares battle data.
		/// </summary>
		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			// Try to initialize battle data and request the next phase.
			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && Orchestrator != null)
			{
				Orchestrator.RequestTransition(Erelia.Battle.Phase.Id.Placement);
			}
		}

		/// <summary>
		/// Ticks the initialize phase until setup succeeds.
		/// </summary>
		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
			// Retry setup while it is still pending.
			if (!pendingSetup)
			{
				return;
			}

			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && Orchestrator != null)
			{
				Orchestrator.RequestTransition(Erelia.Battle.Phase.Id.Placement);
			}
		}

		/// <summary>
		/// Attempts to resolve battle data for the current encounter.
		/// </summary>
		private bool TrySetupBattleData()
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			if (battleData == null || battleData.Board == null)
			{
				return false;
			}

			if (battleData.PhaseInfo == null)
			{
				battleData.PhaseInfo = new Erelia.Battle.Phase.Info();
			}

			battleData.PhaseInfo.Clear();

			Erelia.Core.Creature.Team enemyTeam = ResolveEnemyTeam(battleData.EncounterTable);
			battleData.PhaseInfo.SetEnemyTeam(enemyTeam);

			if (!TrySetupPlacementAreas(battleData))
			{
				return false;
			}

			return true;
		}

		private bool TrySetupPlacementAreas(Erelia.Battle.Data battleData)
		{
			Erelia.Battle.Phase.Info phaseInfo = battleData.PhaseInfo;
			if (phaseInfo == null ||
				!Erelia.Battle.Phase.Initialize.PlacementListGenerator.TryGenerate(
					battleData.Board,
					Erelia.Battle.Phase.Initialize.PlacementMode.HalfBoard,
					out List<Vector3Int> playerCoordinates,
					out List<Vector3Int> enemyCoordinates))
			{
				return false;
			}

			phaseInfo.AddAcceptableCoordinates(playerCoordinates);
			phaseInfo.AddAcceptableCoordinates(enemyCoordinates);
			phaseInfo.AddPlayerPlacementCoordinates(playerCoordinates);
			phaseInfo.AddEnemyPlacementCoordinates(enemyCoordinates);
			return true;
		}

		private static Erelia.Core.Creature.Team ResolveEnemyTeam(Erelia.Core.Encounter.EncounterTable encounterTable)
		{
			Erelia.Core.Encounter.EncounterTable.TeamEntry[] entries = encounterTable?.Teams;
			if (entries == null || entries.Length == 0)
			{
				return null;
			}

			int selectedIndex = SelectEncounterTeamIndex(entries);
			if (TryLoadEncounterTeam(entries, selectedIndex, out Erelia.Core.Creature.Team selectedTeam))
			{
				return selectedTeam;
			}

			for (int i = 0; i < entries.Length; i++)
			{
				if (i == selectedIndex)
				{
					continue;
				}

				if (TryLoadEncounterTeam(entries, i, out Erelia.Core.Creature.Team fallbackTeam))
				{
					return fallbackTeam;
				}
			}

			return null;
		}

		private static int SelectEncounterTeamIndex(Erelia.Core.Encounter.EncounterTable.TeamEntry[] entries)
		{
			int totalWeight = 0;
			for (int i = 0; i < entries.Length; i++)
			{
				totalWeight += Mathf.Max(0, entries[i].Weight);
			}

			if (totalWeight <= 0)
			{
				return 0;
			}

			int roll = Random.Range(0, totalWeight);
			int cumulativeWeight = 0;
			for (int i = 0; i < entries.Length; i++)
			{
				cumulativeWeight += Mathf.Max(0, entries[i].Weight);
				if (roll < cumulativeWeight)
				{
					return i;
				}
			}

			return entries.Length - 1;
		}

		private static bool TryLoadEncounterTeam(
			Erelia.Core.Encounter.EncounterTable.TeamEntry[] entries,
			int index,
			out Erelia.Core.Creature.Team team)
		{
			team = null;

			if (entries == null || index < 0 || index >= entries.Length)
			{
				return false;
			}

			string teamPath = entries[index].TeamPath;
			if (string.IsNullOrEmpty(teamPath))
			{
				return false;
			}

			if (Erelia.Core.Utils.JsonIO.TryLoad(teamPath, out team))
			{
				team.NormalizeSlots();
				return true;
			}

			Debug.LogWarning($"[Erelia.Battle.Phase.Initialize.Root] Failed to load encounter team at '{teamPath}'.");
			return false;
		}

	}
}
