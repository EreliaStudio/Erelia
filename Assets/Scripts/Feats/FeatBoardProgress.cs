using System;
using System.Collections.Generic;

[Serializable]
public class FeatBoardProgress
{
	public List<FeatNode> ExhaustedNodes = new List<FeatNode>();
	public List<FeatNodeProgress> ActiveNodeProgress = new List<FeatNodeProgress>();

	private void CompleteNode(FeatNodeProgress nodeProgress)
	{
		//Check for repeat time of the node. If its reach, remove it from the active node
		//Add its neighbour node if not already added to the list of active node
	}

	public void Register(FeatRequirement.EventBase featEvent)
	{
		foreach (var nodeProgress in ActiveNodeProgress)
		{
			nodeProgress.Register(featEvent);
		}
	}

	public void ApplyCompletedNode(CreatureUnit creatureUnit)
	{
		foreach (var nodeProgress in ActiveNodeProgress)
		{
			if (nodeProgress.IsCompleted == true)
			{
				foreach (var reward in nodeProgress.Node.Rewards)
				{	
					reward.Apply(creatureUnit);
				}
				CompleteNode(nodeProgress);
			}
		}
	}
}
