using System.Collections.Generic;

public static class CreatureUnitFeatProgressUtility
{
	public static List<FeatNode> GetReachableNodes(CreatureUnit p_unit)
	{
		return FeatBoardService.GetReachableNodes(p_unit);
	}

	public static bool IsNodeReachable(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatBoardService.IsNodeReachable(p_unit, p_node);
	}

	public static bool IsNodeCompleted(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatBoardService.IsNodeCompleted(p_unit, p_node);
	}

	public static bool IsNodeExhausted(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatBoardService.IsNodeExhausted(p_unit, p_node);
	}

	public static int GetCompletionCount(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatBoardService.GetCompletionCount(p_unit, p_node);
	}

	public static FeatNodeProgress FindNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatBoardService.FindNodeProgress(p_unit, p_node);
	}

	public static void CompleteNodeOnce(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatBoardService.CompleteNodeOnce(p_unit, p_node);
	}

	public static void ResetNodeProgress(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatBoardService.ResetNodeProgress(p_unit, p_node);
	}

	public static void ClearAllProgress(CreatureUnit p_unit)
	{
		FeatBoardService.ClearAllProgress(p_unit);
	}

	public static int GetHighestUnlockedFormTier(CreatureUnit p_unit)
	{
		return FeatBoardService.GetCurrentFormTier(p_unit);
	}

	public static bool IsFormNodeLockedByCurrentTier(CreatureUnit p_unit, FeatNode p_node)
	{
		return FeatBoardService.IsFormNodeLockedByCurrentTier(p_unit, p_node);
	}
}
