using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Initialize
{
	/// <summary>
	/// Initialization phase that prepares acceptable coordinates, units, and the battle timeline.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		private const float ReserveSideOffset = 2.5f;
		private const float ReserveHeight = 1f;

		[SerializeField] private Transform playerTeamRoot;
		[SerializeField] private Transform enemyTeamRoot;

		private bool pendingSetup;
		[System.NonSerialized] private Erelia.Battle.Board.Presenter boardPresenter;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Initialize;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && Orchestrator != null)
			{
				Orchestrator.RequestTransition(Erelia.Battle.Phase.Id.Placement);
			}
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
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

		private bool TrySetupBattleData()
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Core.Creature.Team playerTeam = Erelia.Core.Context.Instance.SystemData?.PlayerTeam;
			if (battleData == null || battleData.Board == null || battleData.EnemyTeam == null || playerTeam == null)
			{
				return false;
			}

			if (battleData.PhaseInfo == null)
			{
				battleData.PhaseInfo = new Erelia.Battle.Phase.Info();
			}

			battleData.PhaseInfo.Clear();
			battleData.ActiveUnit = null;

			if (!TrySetupAcceptableCoordinates(battleData))
			{
				return false;
			}

			if (!TrySetupUnits(battleData, playerTeam, battleData.EnemyTeam))
			{
				return false;
			}

			return true;
		}

		private bool TrySetupAcceptableCoordinates(Erelia.Battle.Data battleData)
		{
			if (!Erelia.Battle.Phase.Initialize.AcceptableCoordinateGenerator.TryGenerate(
					battleData.Board,
					out List<Vector3Int> acceptableCoordinates))
			{
				return false;
			}

			battleData.PhaseInfo.SetAcceptableCoordinates(acceptableCoordinates);
			battleData.PhaseInfo.ClearPlacementCoordinates();
			return true;
		}

		private bool TrySetupUnits(
			Erelia.Battle.Data battleData,
			Erelia.Core.Creature.Team playerTeam,
			Erelia.Core.Creature.Team enemyTeam)
		{
			DisposeExistingUnitViews(battleData);

			var presenters = new List<Erelia.Battle.Unit.Presenter>();

			if (!TryCreateTeamUnits(playerTeam, Erelia.Battle.Unit.Team.Player, presenters))
			{
				return false;
			}

			if (!TryCreateTeamUnits(enemyTeam, Erelia.Battle.Unit.Team.Enemy, presenters))
			{
				return false;
			}

			battleData.Timeline = new Erelia.Battle.Timeline.Model();
			battleData.Timeline.SetUnits(presenters);
			return presenters.Count > 0;
		}

		private bool TryCreateTeamUnits(
			Erelia.Core.Creature.Team team,
			Erelia.Battle.Unit.Team battleTeam,
			List<Erelia.Battle.Unit.Presenter> presenters)
		{
			Erelia.Core.Creature.Instance.Model[] slots = team?.Slots;
			if (slots == null)
			{
				return false;
			}

			int livingCount = CountCreatures(slots);
			int createdCount = 0;
			for (int i = 0; i < slots.Length; i++)
			{
				Erelia.Core.Creature.Instance.Model creature = slots[i];
				if (creature == null || creature.IsEmpty)
				{
					continue;
				}

				if (!TryCreatePresenter(creature, battleTeam, createdCount, livingCount, out Erelia.Battle.Unit.Presenter presenter))
				{
					return false;
				}

				presenters.Add(presenter);
				createdCount++;
			}

			return true;
		}

		private bool TryCreatePresenter(
			Erelia.Core.Creature.Instance.Model creature,
			Erelia.Battle.Unit.Team battleTeam,
			int teamIndex,
			int teamCount,
			out Erelia.Battle.Unit.Presenter presenter)
		{
			presenter = null;

			if (!TryResolveSpecies(creature, out Erelia.Core.Creature.Species species))
			{
				return false;
			}

			if (species.Prefab == null)
			{
				Debug.LogWarning($"[Erelia.Battle.Phase.Initialize.Root] Species '{species.DisplayName}' has no prefab.");
				return false;
			}

			GameObject worldObject = Object.Instantiate(species.Prefab, GetUnitParent(battleTeam));
			worldObject.name = species.DisplayName;

			Erelia.Core.Creature.Instance.Presenter creaturePresenter =
				worldObject.GetComponent<Erelia.Core.Creature.Instance.Presenter>() ??
				worldObject.GetComponentInChildren<Erelia.Core.Creature.Instance.Presenter>(true);
			creaturePresenter?.SetModel(creature);

			Erelia.Battle.Unit.ObjectView objectView =
				worldObject.GetComponent<Erelia.Battle.Unit.ObjectView>() ??
				worldObject.GetComponentInChildren<Erelia.Battle.Unit.ObjectView>(true);
			if (objectView == null)
			{
				objectView = worldObject.AddComponent<Erelia.Battle.Unit.ObjectView>();
			}

			Erelia.Core.Stats.Values liveStats = species.BaseStats + creature.BonusStats;
			var model = new Erelia.Battle.Unit.Model(creature, battleTeam, teamIndex, liveStats);
			presenter = new Erelia.Battle.Unit.Presenter(model, objectView);
			presenter.Stage(ResolveReserveWorldPosition(battleTeam, teamIndex, teamCount));
			return true;
		}

		private bool TryResolveSpecies(
			Erelia.Core.Creature.Instance.Model creature,
			out Erelia.Core.Creature.Species species)
		{
			species = null;

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry == null || creature == null)
			{
				return false;
			}

			if (registry.TryGet(creature.SpeciesId, out species) && species != null)
			{
				return true;
			}

			Debug.LogWarning(
				$"[Erelia.Battle.Phase.Initialize.Root] Failed to resolve creature species id {creature.SpeciesId}.");
			return false;
		}

		private void DisposeExistingUnitViews(Erelia.Battle.Data battleData)
		{
			IReadOnlyList<Erelia.Battle.Unit.Presenter> units = battleData?.Units;
			if (units == null)
			{
				return;
			}

			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter presenter = units[i];
				GameObject worldObject = presenter?.ObjectView != null
					? presenter.ObjectView.gameObject
					: null;
				presenter?.Dispose();
				if (worldObject == null)
				{
					continue;
				}

				if (Application.isPlaying)
				{
					Object.Destroy(worldObject);
				}
				else
				{
					Object.DestroyImmediate(worldObject);
				}
			}
		}

		private Vector3 ResolveReserveWorldPosition(
			Erelia.Battle.Unit.Team battleTeam,
			int teamIndex,
			int teamCount)
		{
			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			return Erelia.Battle.Unit.StagingPositionUtility.ResolveWorldPosition(
				ResolveBoardPresenter(),
				board,
				battleTeam,
				teamIndex,
				teamCount,
				ReserveSideOffset,
				ReserveHeight);
		}

		private Transform GetUnitParent(Erelia.Battle.Unit.Team battleTeam)
		{
			switch (battleTeam)
			{
				case Erelia.Battle.Unit.Team.Player:
					if (playerTeamRoot != null)
					{
						return playerTeamRoot;
					}
					break;

				case Erelia.Battle.Unit.Team.Enemy:
					if (enemyTeamRoot != null)
					{
						return enemyTeamRoot;
					}
					break;
			}

			Erelia.Battle.Board.Presenter activeBoardPresenter = ResolveBoardPresenter();
			return activeBoardPresenter != null ? activeBoardPresenter.transform : null;
		}

		private Erelia.Battle.Board.Presenter ResolveBoardPresenter()
		{
			if (boardPresenter == null)
			{
				boardPresenter = Object.FindFirstObjectByType<Erelia.Battle.Board.Presenter>();
			}

			return boardPresenter;
		}

		private static int CountCreatures(Erelia.Core.Creature.Instance.Model[] slots)
		{
			if (slots == null)
			{
				return 0;
			}

			int count = 0;
			for (int i = 0; i < slots.Length; i++)
			{
				if (slots[i] != null && !slots[i].IsEmpty)
				{
					count++;
				}
			}

			return count;
		}
	}
}
