using System.Collections.Generic;

public static class CreatureUnitFeatProgressUtility
{
	public static List<FeatNode> GetReachableNodes(CreatureUnit p_unit)
	{
		return FeatProgressionService.GetReachableNodes(p_unit);
	}

	public static bool IsNodeReachable(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatProgressionService.IsNodeReachable(p_unit, p_node);
	}

	public static bool IsNodeCompleted(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatProgressionService.IsNodeCompleted(p_unit, p_node);
	}

	public static bool IsNodeExhausted(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatProgressionService.IsNodeExhausted(p_unit, p_node);
	}

	public static int GetCompletionCount(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatProgressionService.GetCompletionCount(p_unit, p_node);
	}

	public static FeatNodeProgress FindNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatProgressionService.FindNodeProgress(p_unit, p_node);
	}

	public static void CompleteNodeOnce(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatProgressionService.CompleteNodeOnce(p_unit, p_node);
	}

	public static void ResetNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatProgressionService.ResetNodeProgress(p_unit, p_node);
	}

	public static void ClearAllProgress(CreatureUnit p_unit)
	{
		FeatProgressionService.ClearAllProgress(p_unit);
	}

	public static int GetHighestUnlockedFormTier(CreatureUnit p_unit)
	{
		return FeatProgressionService.GetCurrentFormTier(p_unit);
	}

	public static bool IsFormNodeLockedByCurrentTier(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatProgressionService.IsFormNodeLockedByCurrentTier(p_unit, p_node);
	}
}
