using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.PlayerTurn
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		private const string PlayerHudObjectName = "PlayerHUD";
		private const string HealthPanelObjectName = "HealthPanel";
		private const string PaPanelObjectName = "PABar";
		private const string PmPanelObjectName = "PMPanel";
		private const string ActionShortcutBarObjectName = "ActionShortcutBar";

		[SerializeField] private GameObject hudRoot;
		[SerializeField] private GameObject playerHudRoot;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement playerCreatureCardGroup;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement enemyCreatureCardGroup;
		[SerializeField] private Button endTurnButton;
		[SerializeField] private TMP_Text statusText;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private Erelia.Battle.Player.BattlePlayerController playerController;
		[SerializeField] private Erelia.Core.UI.ProgressBarView healthPanel;
		[SerializeField] private Erelia.Core.UI.ProgressBarView apPanel;
		[SerializeField] private Erelia.Core.UI.ProgressBarView pmPanel;
		[SerializeField] private Erelia.Battle.UI.AttackShortcutBar attackShortcutBar;

		[System.NonSerialized] private Erelia.Battle.Orchestrator activeOrchestrator;
		[System.NonSerialized] private readonly List<Vector3Int> movementMaskCoordinates = new List<Vector3Int>();
		[System.NonSerialized] private Dictionary<Vector3Int, List<Vector3Int>> reachablePathsByCell =
			new Dictionary<Vector3Int, List<Vector3Int>>();
		[System.NonSerialized] private readonly List<Vector3Int> attackMaskCoordinates = new List<Vector3Int>();
		[System.NonSerialized] private readonly List<Vector3Int> areaOfEffectMaskCoordinates = new List<Vector3Int>();
		[System.NonSerialized] private Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter> targetableUnitsByCell =
			new Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter>();
		[System.NonSerialized] private Vector3Int areaOfEffectOriginCell;
		[System.NonSerialized] private int areaOfEffectRadius = -1;
		[System.NonSerialized] private bool hasAreaOfEffectOriginCell;
		[System.NonSerialized] private bool isMovementInProgress;
		[System.NonSerialized] private int selectedAttackIndex = -1;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.PlayerTurn;

		public override void Enter(Erelia.Battle.Orchestrator orchestrator)
		{
			activeOrchestrator = orchestrator;

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit == null || !activeUnit.IsAlive)
			{
				battleData?.ClearActiveUnit();
				orchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
				return;
			}

			if (activeUnit.Side == Erelia.Battle.Side.Enemy)
			{
				orchestrator?.RequestTransition(Erelia.Battle.Phase.Id.EnemyTurn);
				return;
			}

			selectedAttackIndex = -1;
			isMovementInProgress = false;
			PopulateHud(battleData);
			SetHudVisible(true);
			SetPlayerHudVisible(true);
			ResolveBoardPresenter();
			BindPlayerController();
			BindAttackShortcutBar(activeUnit);
			BindEndTurnButton();
			RefreshPlayerHud(activeUnit);
			RefreshSelectedAction(activeUnit);
		}

		public override void Exit(Erelia.Battle.Orchestrator orchestrator)
		{
			selectedAttackIndex = -1;
			ClearAreaOfEffect();
			ClearAttackRange();
			ClearMovementRange();
			isMovementInProgress = false;
			UnbindPlayerController();
			UnbindAttackShortcutBar();
			UnbindEndTurnButton();
			SetPlayerHudVisible(false);
			activeOrchestrator = null;
		}

		public override void Tick(Erelia.Battle.Orchestrator orchestrator, float deltaTime)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit == null)
			{
				return;
			}

			if (!activeUnit.IsAlive)
			{
				battleData.ClearActiveUnit();
				activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
				return;
			}

			RefreshPlayerHud(activeUnit);
			RefreshAreaOfEffectPreview(activeUnit);
		}

		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			if (isMovementInProgress)
			{
				return;
			}

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (battleData == null ||
				activeUnit == null ||
				!activeUnit.IsAlive ||
				activeUnit.Side != Erelia.Battle.Side.Player)
			{
				return;
			}

			if (TryGetSelectedAttack(activeUnit, out Erelia.Battle.Attack.Definition attack))
			{
				HandleAttackConfirm(controller, battleData, activeUnit, attack);
				return;
			}

			HandleMovementConfirm(controller, battleData, activeUnit);
		}

		public override void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
			if (isMovementInProgress)
			{
				return;
			}

			Erelia.Battle.Unit.Presenter activeUnit = Erelia.Core.Context.Instance.BattleData?.ActiveUnit;
			if (activeUnit == null)
			{
				return;
			}

			if (selectedAttackIndex >= 0)
			{
				selectedAttackIndex = -1;
				RefreshSelectedAction(activeUnit);
				return;
			}

			controller?.ClearSelection();
		}

		private void EndTurn()
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit != null)
			{
				activeUnit.EndTurn();
				battleData.ClearActiveUnit();
			}

			selectedAttackIndex = -1;
			activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
		}

		private void HandleMovementConfirm(
			Erelia.Battle.Player.BattlePlayerController controller,
			Erelia.Battle.Data battleData,
			Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (activeUnit.RemainingMovementPoints <= 0)
			{
				return;
			}

			if (!TryGetHoveredReachableCell(controller, out Vector3Int targetCell) ||
				!reachablePathsByCell.TryGetValue(targetCell, out List<Vector3Int> path) ||
				path == null ||
				path.Count == 0)
			{
				return;
			}

			ClearMovementRange();
			isMovementInProgress = true;
			SetStatus(BuildMovementStatus(activeUnit));
			RefreshEndTurnButtonInteractivity();
			RefreshAttackShortcutBar(activeUnit);
			activeUnit.MoveAlongPath(
				path,
				battleData.Board,
				ResolveBoardPresenter(),
				() => OnMovementCompleted(activeUnit, path.Count));
		}

		private void HandleAttackConfirm(
			Erelia.Battle.Player.BattlePlayerController controller,
			Erelia.Battle.Data battleData,
			Erelia.Battle.Unit.Presenter activeUnit,
			Erelia.Battle.Attack.Definition attack)
		{
			if (battleData == null ||
				!TryGetHoveredAttackRangeCell(controller, out Vector3Int castCell))
			{
				return;
			}

			if (!activeUnit.TryConsumeActionPoints(attack.ActionPointCost))
			{
				RefreshSelectedAction(activeUnit);
				SetStatus(BuildInsufficientActionPointsStatus(activeUnit, attack));
				return;
			}

			int affectedUnitCount = Erelia.Battle.Attack.TargetingUtility.ApplyAttack(
				battleData,
				activeUnit,
				attack,
				castCell);

			RefreshPlayerHud(activeUnit);
			RefreshSelectedAction(activeUnit);
			SetStatus(BuildAttackCastStatus(activeUnit, attack, castCell, affectedUnitCount));
		}

		private void OnAttackShortcutClicked(int index)
		{
			Erelia.Battle.Player.BattlePlayerController resolvedPlayerController = ResolvePlayerController();
			if (resolvedPlayerController != null)
			{
				resolvedPlayerController.RequestActionShortcut(index);
				return;
			}

			HandleActionSelected(index);
		}

		private void HandleActionSelected(int slotIndex)
		{
			if (isMovementInProgress)
			{
				return;
			}

			Erelia.Battle.Unit.Presenter activeUnit = Erelia.Core.Context.Instance.BattleData?.ActiveUnit;
			if (activeUnit == null)
			{
				return;
			}

			if (selectedAttackIndex == slotIndex)
			{
				selectedAttackIndex = -1;
				RefreshSelectedAction(activeUnit);
				return;
			}

			if (!TryGetAttackAt(activeUnit, slotIndex, out Erelia.Battle.Attack.Definition attack))
			{
				return;
			}

			if (!CanAffordAttack(activeUnit, attack))
			{
				selectedAttackIndex = -1;
				RefreshSelectedAction(activeUnit);
				SetStatus(BuildInsufficientActionPointsStatus(activeUnit, attack));
				return;
			}

			selectedAttackIndex = selectedAttackIndex == slotIndex ? -1 : slotIndex;
			RefreshSelectedAction(activeUnit);
		}

		private void RefreshSelectedAction(Erelia.Battle.Unit.Presenter activeUnit)
		{
			ValidateSelectedAttackIndex(activeUnit);
			RefreshAttackShortcutBar(activeUnit);
			RefreshActionRange(activeUnit);

			if (isMovementInProgress)
			{
				SetStatus(BuildMovementStatus(activeUnit));
				return;
			}

			SetStatus(BuildCurrentStatus(activeUnit));
		}

		private void RefreshActionRange(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (TryGetSelectedAttack(activeUnit, out Erelia.Battle.Attack.Definition attack))
			{
				RefreshAttackRange(activeUnit, attack);
				RefreshAreaOfEffectPreview(activeUnit, attack);
				return;
			}

			RefreshMovementRange(activeUnit);
		}

		private string BuildCurrentStatus(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (TryGetSelectedAttack(activeUnit, out Erelia.Battle.Attack.Definition attack))
			{
				return BuildAttackSelectionStatus(activeUnit, attack);
			}

			if (activeUnit != null &&
				(activeUnit.RemainingActionPoints < activeUnit.ActionPoints ||
				activeUnit.RemainingMovementPoints < activeUnit.MovementPoints))
			{
				return BuildResourceBudgetStatus(activeUnit);
			}

			return BuildTurnStatus(activeUnit, "Player turn");
		}

		private static string BuildTurnStatus(Erelia.Battle.Unit.Presenter activeUnit, string fallback)
		{
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return fallback;
			}

			return activeUnit.Creature.DisplayName + "'s turn";
		}

		private static string BuildMovementStatus(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return "Moving";
			}

			return activeUnit.Creature.DisplayName + " is moving";
		}

		private static string BuildResourceBudgetStatus(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (activeUnit == null)
			{
				return "No resources remaining";
			}

			string displayName = !string.IsNullOrEmpty(activeUnit.Creature?.DisplayName)
				? activeUnit.Creature.DisplayName
				: "Unit";
			return $"{displayName}: {activeUnit.RemainingActionPoints} AP / {activeUnit.RemainingMovementPoints} MP left";
		}

		private static string BuildAttackSelectionStatus(
			Erelia.Battle.Unit.Presenter activeUnit,
			Erelia.Battle.Attack.Definition attack)
		{
			string attackName = attack != null ? attack.DisplayName : "Attack";
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return attackName + " selected";
			}

			return $"{activeUnit.Creature.DisplayName}: {attackName} selected";
		}

		private static string BuildInsufficientActionPointsStatus(
			Erelia.Battle.Unit.Presenter activeUnit,
			Erelia.Battle.Attack.Definition attack)
		{
			string attackName = attack != null ? attack.DisplayName : "Attack";
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return $"Not enough AP for {attackName}";
			}

			return $"{activeUnit.Creature.DisplayName}: not enough AP for {attackName}";
		}

		private static string BuildAttackCastStatus(
			Erelia.Battle.Unit.Presenter activeUnit,
			Erelia.Battle.Attack.Definition attack,
			Vector3Int castCell,
			int affectedUnitCount)
		{
			string attackName = attack != null ? attack.DisplayName : "Attack";
			string affectedLabel = affectedUnitCount == 1
				? "1 unit in area"
				: $"{affectedUnitCount} units in area";
			string locationLabel = $"({castCell.x}, {castCell.y}, {castCell.z})";

			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return $"{attackName} cast at {locationLabel} - {affectedLabel}";
			}

			return $"{activeUnit.Creature.DisplayName}: {attackName} cast at {locationLabel} - {affectedLabel}";
		}

		private void PopulateHud(Erelia.Battle.Data battleData)
		{
			playerCreatureCardGroup?.PopulateUnits(battleData?.PlayerUnits);
			enemyCreatureCardGroup?.PopulateUnits(battleData?.EnemyUnits);
		}

		private void SetHudVisible(bool isVisible)
		{
			if (hudRoot == null || hudRoot.activeSelf == isVisible)
			{
				return;
			}

			hudRoot.SetActive(isVisible);
		}

		private void SetPlayerHudVisible(bool isVisible)
		{
			GameObject resolvedPlayerHudRoot = ResolvePlayerHudRoot();
			if (resolvedPlayerHudRoot == null || resolvedPlayerHudRoot.activeSelf == isVisible)
			{
				return;
			}

			resolvedPlayerHudRoot.SetActive(isVisible);
		}

		private void RefreshPlayerHud(Erelia.Battle.Unit.Presenter activeUnit)
		{
			UpdateProgressBar(ResolveHealthPanel(), activeUnit?.CurrentHealth ?? 0, activeUnit?.MaxHealth ?? 0);
			UpdateProgressBar(ResolveApPanel(), activeUnit?.RemainingActionPoints ?? 0, activeUnit?.ActionPoints ?? 0);
			UpdateProgressBar(ResolvePmPanel(), activeUnit?.RemainingMovementPoints ?? 0, activeUnit?.MovementPoints ?? 0);
		}

		private void SetStatus(string value)
		{
			if (statusText == null)
			{
				return;
			}

			bool hasValue = !string.IsNullOrEmpty(value);
			statusText.gameObject.SetActive(hasValue);
			statusText.text = hasValue ? value : string.Empty;
		}

		private void BindEndTurnButton()
		{
			if (endTurnButton == null)
			{
				return;
			}

			endTurnButton.onClick.RemoveListener(EndTurn);
			endTurnButton.onClick.AddListener(EndTurn);
			endTurnButton.gameObject.SetActive(true);
			RefreshEndTurnButtonInteractivity();
		}

		private void UnbindEndTurnButton()
		{
			if (endTurnButton == null)
			{
				return;
			}

			endTurnButton.onClick.RemoveListener(EndTurn);
			endTurnButton.interactable = false;
			endTurnButton.gameObject.SetActive(false);
		}

		private void BindAttackShortcutBar(Erelia.Battle.Unit.Presenter activeUnit)
		{
			Erelia.Battle.UI.AttackShortcutBar resolvedAttackShortcutBar = ResolveAttackShortcutBar();
			if (resolvedAttackShortcutBar == null)
			{
				return;
			}

			resolvedAttackShortcutBar.SlotClicked -= OnAttackShortcutClicked;
			resolvedAttackShortcutBar.SlotClicked += OnAttackShortcutClicked;
			resolvedAttackShortcutBar.SetInteractable(true);
			resolvedAttackShortcutBar.SetShortcutLabels(BuildShortcutLabels());
			resolvedAttackShortcutBar.SetAttacks(activeUnit != null ? activeUnit.Attacks : null);
			resolvedAttackShortcutBar.SetAvailableActionPoints(activeUnit != null ? activeUnit.RemainingActionPoints : 0);
			resolvedAttackShortcutBar.SetSelectedIndex(selectedAttackIndex);
		}

		private void UnbindAttackShortcutBar()
		{
			Erelia.Battle.UI.AttackShortcutBar resolvedAttackShortcutBar = ResolveAttackShortcutBar();
			if (resolvedAttackShortcutBar == null)
			{
				return;
			}

			resolvedAttackShortcutBar.SlotClicked -= OnAttackShortcutClicked;
			resolvedAttackShortcutBar.Clear();
		}

		private void RefreshAttackShortcutBar(Erelia.Battle.Unit.Presenter activeUnit)
		{
			Erelia.Battle.UI.AttackShortcutBar resolvedAttackShortcutBar = ResolveAttackShortcutBar();
			if (resolvedAttackShortcutBar == null)
			{
				return;
			}

			resolvedAttackShortcutBar.SetShortcutLabels(BuildShortcutLabels());
			resolvedAttackShortcutBar.SetAttacks(activeUnit != null ? activeUnit.Attacks : null);
			resolvedAttackShortcutBar.SetAvailableActionPoints(activeUnit != null ? activeUnit.RemainingActionPoints : 0);
			resolvedAttackShortcutBar.SetSelectedIndex(selectedAttackIndex);
			resolvedAttackShortcutBar.SetInteractable(!isMovementInProgress);
		}

		private void RefreshMovementRange(Erelia.Battle.Unit.Presenter activeUnit)
		{
			ClearAreaOfEffect(false);
			ClearAttackRange(false);
			ClearMovementRange(false);

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Board.Model board = battleData?.Board;
			if (battleData == null ||
				board == null ||
				activeUnit == null ||
				!activeUnit.IsAlive ||
				!activeUnit.IsPlaced ||
				activeUnit.RemainingMovementPoints <= 0)
			{
				ResolveBoardPresenter()?.RebuildMasks();
				return;
			}

			Erelia.Battle.MaskSpriteRegistry maskSpriteRegistry = Erelia.Battle.MaskSpriteRegistry.Instance;
			if (maskSpriteRegistry == null ||
				!maskSpriteRegistry.TryGetSprite(Erelia.Battle.Voxel.Mask.Type.MovementRange, out _))
			{
				Debug.LogWarning("[Erelia.Battle.Phase.PlayerTurn.Root] MovementRange mask sprite is missing from the mask sprite registry.");
			}

			reachablePathsByCell = Erelia.Battle.Board.MovementPathfinder.BuildReachablePaths(
				battleData,
				activeUnit);

			foreach (KeyValuePair<Vector3Int, List<Vector3Int>> entry in reachablePathsByCell)
			{
				Vector3Int coordinate = entry.Key;
				if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (cell == null)
				{
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.MovementRange);
				movementMaskCoordinates.Add(coordinate);
			}

			Erelia.Battle.Board.Presenter resolvedBoardPresenter = ResolveBoardPresenter();
			if (resolvedBoardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.PlayerTurn.Root] Cannot rebuild movement masks because the board presenter is missing.");
				return;
			}

			resolvedBoardPresenter.RebuildMasks();
		}

		private void ClearMovementRange()
		{
			ClearMovementRange(true);
		}

		private void ClearMovementRange(bool rebuildMasks)
		{
			if (movementMaskCoordinates.Count == 0)
			{
				reachablePathsByCell?.Clear();
				return;
			}

			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			if (board != null)
			{
				for (int i = 0; i < movementMaskCoordinates.Count; i++)
				{
					Vector3Int coordinate = movementMaskCoordinates[i];
					if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
					{
						continue;
					}

					Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
					cell?.RemoveMask(Erelia.Battle.Voxel.Mask.Type.MovementRange);
				}

				if (rebuildMasks)
				{
					ResolveBoardPresenter()?.RebuildMasks();
				}
			}

			movementMaskCoordinates.Clear();
			reachablePathsByCell?.Clear();
		}

		private void RefreshAttackRange(
			Erelia.Battle.Unit.Presenter activeUnit,
			Erelia.Battle.Attack.Definition attack)
		{
			ClearAreaOfEffect(false);
			ClearMovementRange(false);
			ClearAttackRange(false);

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Board.Model board = battleData?.Board;
			if (battleData == null ||
				board == null ||
				activeUnit == null ||
				attack == null ||
				!activeUnit.IsAlive ||
				!activeUnit.IsPlaced)
			{
				ResolveBoardPresenter()?.RebuildMasks();
				return;
			}

			Erelia.Battle.MaskSpriteRegistry maskSpriteRegistry = Erelia.Battle.MaskSpriteRegistry.Instance;
			if (maskSpriteRegistry == null ||
				!maskSpriteRegistry.TryGetSprite(Erelia.Battle.Voxel.Mask.Type.AttackRange, out _))
			{
				Debug.LogWarning("[Erelia.Battle.Phase.PlayerTurn.Root] AttackRange mask sprite is missing from the mask sprite registry.");
			}

			targetableUnitsByCell = Erelia.Battle.Attack.TargetingUtility.BuildTargetableUnits(
				battleData,
				activeUnit,
				attack);

			List<Vector3Int> targetableCoordinates = Erelia.Battle.Attack.TargetingUtility.BuildRangeCoordinates(
				battleData,
				activeUnit.Cell,
				attack.Range,
				attack.RangePattern);

			for (int i = 0; i < targetableCoordinates.Count; i++)
			{
				Vector3Int coordinate = targetableCoordinates[i];
				if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (cell == null)
				{
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.AttackRange);
				attackMaskCoordinates.Add(coordinate);
			}

			ResolveBoardPresenter()?.RebuildMasks();
		}

		private void ClearAttackRange()
		{
			ClearAttackRange(true);
		}

		private void ClearAttackRange(bool rebuildMasks)
		{
			if (attackMaskCoordinates.Count == 0)
			{
				targetableUnitsByCell?.Clear();
				return;
			}

			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			if (board != null)
			{
				for (int i = 0; i < attackMaskCoordinates.Count; i++)
				{
					Vector3Int coordinate = attackMaskCoordinates[i];
					if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
					{
						continue;
					}

					Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
					cell?.RemoveMask(Erelia.Battle.Voxel.Mask.Type.AttackRange);
				}

				if (rebuildMasks)
				{
					ResolveBoardPresenter()?.RebuildMasks();
				}
			}

			attackMaskCoordinates.Clear();
			targetableUnitsByCell?.Clear();
		}

		private void RefreshAreaOfEffectPreview(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (!TryGetSelectedAttack(activeUnit, out Erelia.Battle.Attack.Definition attack))
			{
				ClearAreaOfEffect();
				return;
			}

			RefreshAreaOfEffectPreview(activeUnit, attack);
		}

		private void RefreshAreaOfEffectPreview(
			Erelia.Battle.Unit.Presenter activeUnit,
			Erelia.Battle.Attack.Definition attack)
		{
			if (activeUnit == null ||
				attack == null ||
				!activeUnit.IsAlive ||
				!activeUnit.IsPlaced ||
				isMovementInProgress)
			{
				ClearAreaOfEffect();
				return;
			}

			Erelia.Battle.Player.BattlePlayerController resolvedPlayerController = ResolvePlayerController();
			if (!TryGetHoveredAttackRangeCell(resolvedPlayerController, out Vector3Int targetCell))
			{
				ClearAreaOfEffect();
				return;
			}

			if (hasAreaOfEffectOriginCell &&
				targetCell == areaOfEffectOriginCell &&
				areaOfEffectRadius == attack.AreaOfEffectRange)
			{
				return;
			}

			ClearAreaOfEffect(false);

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Board.Model board = battleData?.Board;
			if (battleData == null || board == null)
			{
				return;
			}

			Erelia.Battle.MaskSpriteRegistry maskSpriteRegistry = Erelia.Battle.MaskSpriteRegistry.Instance;
			if (maskSpriteRegistry == null ||
				!maskSpriteRegistry.TryGetSprite(Erelia.Battle.Voxel.Mask.Type.AreaOfEffect, out _))
			{
				Debug.LogWarning("[Erelia.Battle.Phase.PlayerTurn.Root] AreaOfEffect mask sprite is missing from the mask sprite registry.");
			}

			List<Vector3Int> impactedCoordinates = Erelia.Battle.Attack.TargetingUtility.BuildAreaOfEffectCoordinates(
				battleData,
				targetCell,
				attack.AreaOfEffectRange);

			for (int i = 0; i < impactedCoordinates.Count; i++)
			{
				Vector3Int coordinate = impactedCoordinates[i];
				if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (cell == null)
				{
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.AreaOfEffect);
				areaOfEffectMaskCoordinates.Add(coordinate);
			}

			areaOfEffectOriginCell = targetCell;
			areaOfEffectRadius = attack.AreaOfEffectRange;
			hasAreaOfEffectOriginCell = true;
			ResolveBoardPresenter()?.RebuildMasks();
		}

		private void ClearAreaOfEffect()
		{
			ClearAreaOfEffect(true);
		}

		private void ClearAreaOfEffect(bool rebuildMasks)
		{
			if (areaOfEffectMaskCoordinates.Count == 0 && !hasAreaOfEffectOriginCell)
			{
				areaOfEffectOriginCell = default;
				areaOfEffectRadius = -1;
				return;
			}

			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			if (board != null)
			{
				for (int i = 0; i < areaOfEffectMaskCoordinates.Count; i++)
				{
					Vector3Int coordinate = areaOfEffectMaskCoordinates[i];
					if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
					{
						continue;
					}

					Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
					cell?.RemoveMask(Erelia.Battle.Voxel.Mask.Type.AreaOfEffect);
				}

				if (rebuildMasks)
				{
					ResolveBoardPresenter()?.RebuildMasks();
				}
			}

			areaOfEffectMaskCoordinates.Clear();
			areaOfEffectOriginCell = default;
			areaOfEffectRadius = -1;
			hasAreaOfEffectOriginCell = false;
		}

		private void OnMovementCompleted(Erelia.Battle.Unit.Presenter movedUnit, int pathCost)
		{
			isMovementInProgress = false;
			if (movedUnit == null)
			{
				RefreshEndTurnButtonInteractivity();
				RefreshAttackShortcutBar(null);
				return;
			}

			if (!movedUnit.TryConsumeMovementPoints(pathCost))
			{
				Debug.LogWarning(
					$"[Erelia.Battle.Phase.PlayerTurn.Root] Failed to consume {pathCost} movement points for '{movedUnit.Creature?.DisplayName ?? movedUnit.name}'.");
			}

			RefreshPlayerHud(movedUnit);
			RefreshEndTurnButtonInteractivity();
			RefreshSelectedAction(movedUnit);
		}

		private bool TryGetHoveredReachableCell(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Vector3Int targetCell)
		{
			targetCell = default;
			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			if (controller == null || !controller.HasHoveredCell() || board == null)
			{
				return false;
			}

			targetCell = controller.HoveredCell();
			if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, targetCell))
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = board.Cells[targetCell.x, targetCell.y, targetCell.z];
			return cell != null && cell.HasMask(Erelia.Battle.Voxel.Mask.Type.MovementRange);
		}

		private bool TryGetHoveredAttackRangeCell(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Vector3Int targetCell)
		{
			targetCell = default;

			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			if (controller == null || !controller.HasHoveredCell() || board == null)
			{
				return false;
			}

			targetCell = controller.HoveredCell();
			if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, targetCell))
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = board.Cells[targetCell.x, targetCell.y, targetCell.z];
			return cell != null && cell.HasMask(Erelia.Battle.Voxel.Mask.Type.AttackRange);
		}

		private bool TryGetHoveredTargetableCell(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Vector3Int targetCell,
			out Erelia.Battle.Unit.Presenter targetUnit)
		{
			targetCell = default;
			targetUnit = null;

			return TryGetHoveredAttackRangeCell(controller, out targetCell) &&
				targetableUnitsByCell.TryGetValue(targetCell, out targetUnit) &&
				targetUnit != null;
		}

		private void ValidateSelectedAttackIndex(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (selectedAttackIndex < 0)
			{
				return;
			}

			if (!TryGetAttackAt(activeUnit, selectedAttackIndex, out Erelia.Battle.Attack.Definition attack) ||
				!CanAffordAttack(activeUnit, attack))
			{
				selectedAttackIndex = -1;
			}
		}

		private bool TryGetSelectedAttack(
			Erelia.Battle.Unit.Presenter activeUnit,
			out Erelia.Battle.Attack.Definition attack)
		{
			return TryGetAttackAt(activeUnit, selectedAttackIndex, out attack);
		}

		private static bool TryGetAttackAt(
			Erelia.Battle.Unit.Presenter activeUnit,
			int attackIndex,
			out Erelia.Battle.Attack.Definition attack)
		{
			attack = null;
			if (activeUnit == null || attackIndex < 0)
			{
				return false;
			}

			IReadOnlyList<Erelia.Battle.Attack.Definition> attacks = activeUnit.Attacks;
			if (attacks == null || attackIndex >= attacks.Count)
			{
				return false;
			}

			attack = attacks[attackIndex];
			return attack != null;
		}

		private Erelia.Battle.Board.Presenter ResolveBoardPresenter()
		{
			if (boardPresenter == null)
			{
				boardPresenter = Object.FindFirstObjectByType<Erelia.Battle.Board.Presenter>();
			}

			return boardPresenter;
		}

		private Erelia.Battle.Player.BattlePlayerController ResolvePlayerController()
		{
			if (playerController == null)
			{
				playerController = Object.FindFirstObjectByType<Erelia.Battle.Player.BattlePlayerController>();
			}

			return playerController;
		}

		private void BindPlayerController()
		{
			Erelia.Battle.Player.BattlePlayerController resolvedPlayerController = ResolvePlayerController();
			if (resolvedPlayerController == null)
			{
				return;
			}

			resolvedPlayerController.ActionSelected -= HandleActionSelected;
			resolvedPlayerController.ActionSelected += HandleActionSelected;
		}

		private void UnbindPlayerController()
		{
			Erelia.Battle.Player.BattlePlayerController resolvedPlayerController = ResolvePlayerController();
			if (resolvedPlayerController == null)
			{
				return;
			}

			resolvedPlayerController.ActionSelected -= HandleActionSelected;
		}

		private string[] BuildShortcutLabels()
		{
			var labels = new string[Erelia.Core.Creature.Instance.Model.MaxAttackCount];
			Erelia.Battle.Player.BattlePlayerController resolvedPlayerController = ResolvePlayerController();
			for (int i = 0; i < labels.Length; i++)
			{
				labels[i] = resolvedPlayerController != null
					? resolvedPlayerController.GetActionShortcutLabel(i)
					: (i + 1).ToString();
			}

			return labels;
		}

		private void RefreshEndTurnButtonInteractivity()
		{
			if (endTurnButton == null)
			{
				return;
			}

			endTurnButton.interactable = !isMovementInProgress;
		}

		private GameObject ResolvePlayerHudRoot()
		{
			if (playerHudRoot == null && hudRoot != null)
			{
				Transform playerHudTransform = hudRoot.transform.Find(PlayerHudObjectName);
				if (playerHudTransform != null)
				{
					playerHudRoot = playerHudTransform.gameObject;
				}
			}

			return playerHudRoot;
		}

		private Erelia.Battle.UI.AttackShortcutBar ResolveAttackShortcutBar()
		{
			if (attackShortcutBar == null)
			{
				GameObject resolvedPlayerHudRoot = ResolvePlayerHudRoot();
				if (resolvedPlayerHudRoot != null)
				{
					Transform shortcutBarTransform = resolvedPlayerHudRoot.transform.Find(ActionShortcutBarObjectName);
					if (shortcutBarTransform != null)
					{
						attackShortcutBar = shortcutBarTransform.GetComponent<Erelia.Battle.UI.AttackShortcutBar>();
					}

					if (attackShortcutBar == null)
					{
						attackShortcutBar = resolvedPlayerHudRoot.GetComponentInChildren<Erelia.Battle.UI.AttackShortcutBar>(true);
					}
				}
			}

			return attackShortcutBar;
		}

		private Erelia.Core.UI.ProgressBarView ResolveHealthPanel()
		{
			if (healthPanel == null)
			{
				healthPanel = ResolveProgressBar(HealthPanelObjectName);
			}

			return healthPanel;
		}

		private Erelia.Core.UI.ProgressBarView ResolveApPanel()
		{
			if (apPanel == null)
			{
				apPanel = ResolveProgressBar(PaPanelObjectName);
			}

			return apPanel;
		}

		private Erelia.Core.UI.ProgressBarView ResolvePmPanel()
		{
			if (pmPanel == null)
			{
				pmPanel = ResolveProgressBar(PmPanelObjectName);
			}

			return pmPanel;
		}

		private Erelia.Core.UI.ProgressBarView ResolveProgressBar(string panelObjectName)
		{
			GameObject resolvedPlayerHudRoot = ResolvePlayerHudRoot();
			if (resolvedPlayerHudRoot == null)
			{
				return null;
			}

			Transform panelTransform = resolvedPlayerHudRoot.transform.Find(panelObjectName);
			if (panelTransform != null)
			{
				return panelTransform.GetComponent<Erelia.Core.UI.ProgressBarView>();
			}

			Erelia.Core.UI.ProgressBarView[] progressBars =
				resolvedPlayerHudRoot.GetComponentsInChildren<Erelia.Core.UI.ProgressBarView>(true);
			for (int i = 0; i < progressBars.Length; i++)
			{
				Erelia.Core.UI.ProgressBarView progressBar = progressBars[i];
				if (progressBar != null && progressBar.gameObject.name == panelObjectName)
				{
					return progressBar;
				}
			}

			return null;
		}

		private static void UpdateProgressBar(
			Erelia.Core.UI.ProgressBarView progressBar,
			int currentValue,
			int maxValue)
		{
			if (progressBar == null)
			{
				return;
			}

			int clampedMaxValue = Mathf.Max(0, maxValue);
			int clampedCurrentValue = Mathf.Clamp(currentValue, 0, clampedMaxValue);
			float ratio01 = clampedMaxValue > 0
				? (float)clampedCurrentValue / clampedMaxValue
				: 0f;

			progressBar.SetProgress(ratio01);
			progressBar.SetLabel($"{clampedCurrentValue}/{clampedMaxValue}");
		}

		private static bool CanAffordAttack(
			Erelia.Battle.Unit.Presenter activeUnit,
			Erelia.Battle.Attack.Definition attack)
		{
			return activeUnit != null &&
				attack != null &&
				attack.ActionPointCost <= activeUnit.RemainingActionPoints;
		}
	}
}
