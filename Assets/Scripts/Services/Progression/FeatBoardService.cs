using System.Collections.Generic;

public sealed class FeatBoardService
{
	private readonly GameContext gameContext;
	private readonly Dictionary<CreatureUnit, List<FeatRequirement.EventBase>> battleEventsByUnit = new();

	private BattleContext activeBattleContext;

	public FeatBoardService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public void Initialize()
	{
		EventCenter.BattleStarted += OnBattleStarted;
		EventCenter.BattleFeatEventOccurred += OnBattleFeatEventOccurred;
		EventCenter.BattleAbilityResolved += OnBattleAbilityResolved;
		EventCenter.BattleTurnEnded += OnBattleTurnEnded;
		EventCenter.BattleResolved += OnBattleResolved;
	}

	public void Shutdown()
	{
		EventCenter.BattleStarted -= OnBattleStarted;
		EventCenter.BattleFeatEventOccurred -= OnBattleFeatEventOccurred;
		EventCenter.BattleAbilityResolved -= OnBattleAbilityResolved;
		EventCenter.BattleTurnEnded -= OnBattleTurnEnded;
		EventCenter.BattleResolved -= OnBattleResolved;

		activeBattleContext = null;
		battleEventsByUnit.Clear();
	}

	private void OnBattleStarted(BattleContext p_battleContext)
	{
		activeBattleContext = p_battleContext;
		battleEventsByUnit.Clear();

		if (activeBattleContext?.PlayerUnits == null)
		{
			return;
		}

		for (int index = 0; index < activeBattleContext.PlayerUnits.Count; index++)
		{
			CreatureUnit unit = activeBattleContext.PlayerUnits[index]?.SourceUnit;
			if (unit != null && !battleEventsByUnit.ContainsKey(unit))
			{
				battleEventsByUnit.Add(unit, new List<FeatRequirement.EventBase>());
			}
		}
	}

	private void OnBattleFeatEventOccurred(BattleUnit p_unit, FeatRequirement.EventBase p_featEvent)
	{
		if (p_unit == null ||
			p_unit.Side != BattleSide.Player ||
			p_unit.SourceUnit == null ||
			p_featEvent == null)
		{
			return;
		}

		GetOrCreateBattleEventList(p_unit.SourceUnit).Add(p_featEvent);
	}

	private void OnBattleAbilityResolved(BattleContext p_battleContext, BattleUnit p_sourceUnit)
	{
		if (!ReferenceEquals(p_battleContext, activeBattleContext) ||
			p_sourceUnit?.SourceUnit == null ||
			p_sourceUnit.Side != BattleSide.Player)
		{
			return;
		}

		ResetTransientRequirementProgress(p_sourceUnit.SourceUnit, FeatRequirement.Scope.Action);
	}

	private void OnBattleTurnEnded(BattleContext p_battleContext, BattleUnit p_unit)
	{
		if (!ReferenceEquals(p_battleContext, activeBattleContext) ||
			p_unit?.SourceUnit == null ||
			p_unit.Side != BattleSide.Player)
		{
			return;
		}

		ResetTransientRequirementProgress(p_unit.SourceUnit, FeatRequirement.Scope.Turn);
	}

	private void OnBattleResolved(BattleContext p_battleContext, BattleSide p_winner)
	{
		if (!ReferenceEquals(p_battleContext, activeBattleContext))
		{
			return;
		}

		bool playerWon = p_winner == BattleSide.Player;
		IReadOnlyList<BattleUnit> playerUnits = activeBattleContext?.PlayerUnits;
		if (playerUnits != null)
		{
			for (int index = 0; index < playerUnits.Count; index++)
			{
				BattleUnit battleUnit = playerUnits[index];
				if (battleUnit?.SourceUnit == null)
				{
					continue;
				}

				List<FeatRequirement.EventBase> featEvents = GetOrCreateBattleEventList(battleUnit.SourceUnit);
				if (playerWon)
				{
					featEvents.Add(new WinBattleCountRequirement.Event { UnitSurvived = !battleUnit.IsDefeated });
				}

				int completedNodeCount = RegisterFightEvents(battleUnit.SourceUnit, featEvents, true);
				if (completedNodeCount > 0)
				{
					EventCenter.EmitFeatProgressUpdated(battleUnit.SourceUnit, completedNodeCount);
				}
			}
		}

		activeBattleContext = null;
		battleEventsByUnit.Clear();
	}

	private List<FeatRequirement.EventBase> GetOrCreateBattleEventList(CreatureUnit p_unit)
	{
		if (!battleEventsByUnit.TryGetValue(p_unit, out List<FeatRequirement.EventBase> featEvents))
		{
			featEvents = new List<FeatRequirement.EventBase>();
			battleEventsByUnit[p_unit] = featEvents;
		}

		return featEvents;
	}

	public static FeatBoard InitializeCreatureUnit(CreatureUnit p_unit)
	{
		if (p_unit == null)
		{
			return null;
		}

		if (p_unit.Species == null)
		{
			p_unit.CurrentFormID = string.Empty;
			return null;
		}

		EnsureValidCurrentForm(p_unit);

		if (p_unit.Species.FeatBoard == null)
		{
			return null;
		}

		UnlockRootNode(p_unit, p_unit.Species.FeatBoard);
		return p_unit.Species.FeatBoard;
	}

	public static void ApplyProgress(CreatureUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		FeatBoard featBoard = InitializeCreatureUnit(p_unit);
		if (p_unit.Species == null)
		{
			p_unit.Attributes = new Attributes();
			p_unit.Abilities = new List<Ability>();
			p_unit.PermanentPassives = new List<Status>();
			p_unit.CurrentFormID = string.Empty;
			return;
		}

		p_unit.Attributes = new Attributes(p_unit.Species.Attributes);
		p_unit.Abilities = new List<Ability>();
		p_unit.AddAbilities(p_unit.Species.DefaultAbilities);
		p_unit.PermanentPassives = new List<Status>();

		if (featBoard == null || p_unit.FeatBoardProgress?.NodeProgress == null)
		{
			return;
		}

		for (int progressIndex = 0; progressIndex < p_unit.FeatBoardProgress.NodeProgress.Count; progressIndex++)
		{
			FeatNodeProgress nodeProgress = p_unit.FeatBoardProgress.NodeProgress[progressIndex];
			if (nodeProgress == null)
			{
				continue;
			}

			FeatNode node = featBoard.GetNode(nodeProgress.NodeId);
			if (node == null || nodeProgress.CompletionCount <= 0 || node.Rewards == null)
			{
				continue;
			}

			for (int completionIndex = 0; completionIndex < nodeProgress.CompletionCount; completionIndex++)
			{
				for (int rewardIndex = 0; rewardIndex < node.Rewards.Count; rewardIndex++)
				{
					FeatReward reward = node.Rewards[rewardIndex];
					if (reward == null)
					{
						continue;
					}

					reward.Apply(p_unit);
				}
			}
		}
	}

	public static List<FeatNode> GetReachableNodes(CreatureUnit p_unit)
	{
		List<FeatNode> reachableNodes = new List<FeatNode>();
		FeatBoard featBoard = GetBoard(p_unit);
		if (featBoard == null || featBoard.Nodes == null)
		{
			return reachableNodes;
		}

		for (int index = 0; index < featBoard.Nodes.Count; index++)
		{
			FeatNode node = featBoard.Nodes[index];
			if (node != null && IsNodeReachable(p_unit, node))
			{
				reachableNodes.Add(node);
			}
		}

		return reachableNodes;
	}

	public static bool IsNodeReachable(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatBoard featBoard = GetBoard(p_unit);
		if (featBoard == null || p_node == null)
		{
			return false;
		}

		FeatNodeProgress progress = FindNodeProgress(p_unit, p_node);
		if (progress != null && progress.IsExhausted(p_node))
		{
			return false;
		}

		if (p_node.Kind == FeatNodeKind.Form && IsFormNodeLockedByCurrentTier(p_unit, p_node))
		{
			return false;
		}

		if (featBoard.IsRootNode(p_node))
		{
			return true;
		}

		for (int index = 0; index < p_node.NeighbourNodeIds.Count; index++)
		{
			string neighbourNodeId = p_node.NeighbourNodeIds[index];
			if (GetCompletionCount(p_unit, neighbourNodeId) > 0)
			{
				return true;
			}
		}

		return false;
	}

	public static bool IsNodeCompleted(CreatureUnit p_unit, FeatNode p_node)
	{
		return GetCompletionCount(p_unit, p_node) > 0;
	}

	public static bool IsNodeExhausted(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatNodeProgress progress = FindNodeProgress(p_unit, p_node);
		return progress != null && progress.IsExhausted(p_node);
	}

	public static int GetCompletionCount(CreatureUnit p_unit, FeatNode p_node)
	{
		return p_node == null ? 0 : GetCompletionCount(p_unit, p_node.Id);
	}

	public static int GetCompletionCount(CreatureUnit p_unit, string p_nodeId)
	{
		FeatNodeProgress progress = FindNodeProgress(p_unit, p_nodeId);
		return progress != null ? progress.CompletionCount : 0;
	}

	public static FeatNodeProgress FindNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		return p_node == null ? null : FindNodeProgress(p_unit, p_node.Id);
	}

	public static FeatNodeProgress FindNodeProgress(CreatureUnit p_unit, string p_nodeId)
	{
		if (p_unit?.FeatBoardProgress == null || string.IsNullOrWhiteSpace(p_nodeId))
		{
			return null;
		}

		return p_unit.FeatBoardProgress.FindProgress(p_nodeId);
	}

	public static bool CompleteNodeOnce(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatBoard featBoard = GetBoard(p_unit);
		if (featBoard == null || p_node == null || !IsNodeReachable(p_unit, p_node))
		{
			return false;
		}

		FeatNodeProgress progress = p_unit.FeatBoardProgress.GetOrCreateProgress(p_node);
		if (!TryCompleteNodeProgress(p_node, progress))
		{
			return false;
		}

		ApplyProgress(p_unit);
		return true;
	}

	public static int RegisterEvent(CreatureUnit p_unit, FeatRequirement.EventBase p_featEvent)
	{
		return RegisterEvent(p_unit, p_featEvent, true);
	}

	public static int RegisterFightEvents(
		CreatureUnit p_unit,
		IReadOnlyList<FeatRequirement.EventBase> p_featEvents,
		bool p_includeTransientRequirements = true)
	{
		if (p_unit == null)
		{
			return 0;
		}

		ResetTransientRequirementProgress(p_unit);

		if (p_featEvents == null || p_featEvents.Count == 0)
		{
			return 0;
		}

		int completionCount = RegisterEvents(p_unit, p_featEvents, p_includeTransientRequirements);

		ResetTransientRequirementProgress(p_unit);
		return completionCount;
	}

	public static void ResetTransientRequirementProgress(CreatureUnit p_unit)
	{
		if (p_unit?.FeatBoardProgress?.NodeProgress == null)
		{
			return;
		}

		for (int index = 0; index < p_unit.FeatBoardProgress.NodeProgress.Count; index++)
		{
			p_unit.FeatBoardProgress.NodeProgress[index]?.ResetTransientRequirementProgress();
		}
	}

	public static void ResetTransientRequirementProgress(CreatureUnit p_unit, FeatRequirement.Scope p_scope)
	{
		if (p_unit?.FeatBoardProgress?.NodeProgress == null)
		{
			return;
		}

		for (int nodeIndex = 0; nodeIndex < p_unit.FeatBoardProgress.NodeProgress.Count; nodeIndex++)
		{
			FeatNodeProgress nodeProgress = p_unit.FeatBoardProgress.NodeProgress[nodeIndex];
			if (nodeProgress?.RequirementProgress == null)
			{
				continue;
			}

			for (int requirementIndex = 0; requirementIndex < nodeProgress.RequirementProgress.Count; requirementIndex++)
			{
				FeatRequirementProgress requirementProgress = nodeProgress.RequirementProgress[requirementIndex];
				if (requirementProgress?.Requirement == null ||
					requirementProgress.Requirement.RequirementScope != p_scope)
				{
					continue;
				}

				requirementProgress.CurrentProgress = 0f;
			}
		}
	}

	private static int RegisterEvent(CreatureUnit p_unit, FeatRequirement.EventBase p_featEvent, bool p_includeTransientRequirements)
	{
		return RegisterEvents(
			p_unit,
			p_featEvent != null ? new[] { p_featEvent } : null,
			p_includeTransientRequirements);
	}

	private static int RegisterEvents(
		CreatureUnit p_unit,
		IReadOnlyList<FeatRequirement.EventBase> p_featEvents,
		bool p_includeTransientRequirements)
	{
		FeatBoard featBoard = GetBoard(p_unit);
		if (featBoard == null || p_featEvents == null || p_featEvents.Count == 0)
		{
			return 0;
		}

		List<FeatNode> reachableNodes = GetReachableNodes(p_unit);
		int completionCount = 0;

		for (int index = 0; index < reachableNodes.Count; index++)
		{
			FeatNode node = reachableNodes[index];
			if (node == null)
			{
				continue;
			}

			FeatNodeProgress progress = p_unit.FeatBoardProgress.GetOrCreateProgress(node);
			if (progress == null || progress.IsExhausted(node) || !progress.HasRequirements)
			{
				continue;
			}

			progress.RegisterEvents(p_featEvents, p_includeTransientRequirements);
			if (progress.IsCompleted && TryCompleteNodeProgress(node, progress))
			{
				completionCount++;
			}
		}

		if (completionCount > 0)
		{
			ApplyProgress(p_unit);
		}

		return completionCount;
	}

	public static void ResetNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatBoard featBoard = GetBoard(p_unit);
		if (featBoard == null || p_node == null)
		{
			return;
		}

		if (featBoard.IsRootNode(p_node))
		{
			FeatNodeProgress rootProgress = p_unit.FeatBoardProgress.GetOrCreateProgress(p_node);
			if (rootProgress != null)
			{
				rootProgress.CompletionCount = 1;
				rootProgress.ResetRequirementProgress();
			}
		}
		else
		{
			p_unit.FeatBoardProgress.RemoveProgress(p_node.Id);
		}

		ApplyProgress(p_unit);
	}

	public static void ClearAllProgress(CreatureUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		p_unit.FeatBoardProgress = new FeatBoardProgress();
		ApplyProgress(p_unit);
	}

	public static int GetCurrentFormTier(CreatureUnit p_unit)
	{
		if (p_unit == null || p_unit.Species == null)
		{
			return -1;
		}

		InitializeCreatureUnit(p_unit);
		return p_unit.GetForm().Tier;
	}

	public static bool IsFormNodeLockedByCurrentTier(CreatureUnit p_unit, FeatNode p_node)
	{
		if (p_unit == null || p_node == null || p_node.Rewards == null)
		{
			return false;
		}

		int currentTier = GetCurrentFormTier(p_unit);

		for (int index = 0; index < p_node.Rewards.Count; index++)
		{
			if (p_node.Rewards[index] is not ChangeFormReward changeFormReward)
			{
				continue;
			}

			if (string.IsNullOrWhiteSpace(changeFormReward.FormKey))
			{
				continue;
			}

			if (p_unit.Species.Forms.TryGetValue(changeFormReward.FormKey, out CreatureForm targetForm) &&
				targetForm != null &&
				targetForm.Tier <= currentTier)
			{
				return true;
			}
		}

		return false;
	}

	private static FeatBoard GetBoard(CreatureUnit p_unit)
	{
		return InitializeCreatureUnit(p_unit);
	}

	private static bool TryCompleteNodeProgress(FeatNode p_node, FeatNodeProgress p_progress)
	{
		if (p_node == null || p_progress == null || p_progress.IsExhausted(p_node))
		{
			return false;
		}

		p_progress.Complete();
		return true;
	}

	private static void EnsureValidCurrentForm(CreatureUnit p_unit)
	{
		if (p_unit.Species == null || p_unit.Species.Forms == null || p_unit.Species.Forms.Count == 0)
		{
			p_unit.CurrentFormID = string.Empty;
			return;
		}

		if (p_unit.HasForm(p_unit.CurrentFormID))
		{
			return;
		}

		foreach (string formId in p_unit.Species.Forms.Keys)
		{
			p_unit.CurrentFormID = formId ?? string.Empty;
			return;
		}

		p_unit.CurrentFormID = string.Empty;
	}

	private static void UnlockRootNode(CreatureUnit p_unit, FeatBoard p_featBoard)
	{
		if (p_unit == null || p_featBoard == null)
		{
			return;
		}

		FeatNode rootNode = p_featBoard.GetRootNode();
		if (rootNode == null)
		{
			return;
		}

		FeatNodeProgress rootProgress = p_unit.FeatBoardProgress.GetOrCreateProgress(rootNode);
		if (rootProgress == null || rootProgress.CompletionCount > 0)
		{
			return;
		}

		rootProgress.CompletionCount = 1;
		rootProgress.ResetRequirementProgress();
	}
}
