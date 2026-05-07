using System;

public static partial class EventCenter
{
	public static event Action<CreatureUnit, int> FeatProgressUpdated;
	public static event Action<CreatureUnit, FeatNode> FeatNodeUnlocked;
	public static event Action<CreatureUnit, FeatReward> FeatRewardUnlocked;

	public static void EmitFeatProgressUpdated(CreatureUnit p_unit, int p_completedNodeCount)
	{
		FeatProgressUpdated?.Invoke(p_unit, p_completedNodeCount);
	}

	public static void EmitFeatNodeUnlocked(CreatureUnit p_unit, FeatNode p_node)
	{
		FeatNodeUnlocked?.Invoke(p_unit, p_node);
	}

	public static void EmitFeatRewardUnlocked(CreatureUnit p_unit, FeatReward p_reward)
	{
		FeatRewardUnlocked?.Invoke(p_unit, p_reward);
	}
}
