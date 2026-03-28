using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FeatNode
{
	public string Id = Guid.NewGuid().ToString();
	public string DisplayName = "New Feat";
	public Vector2 Position;

	public FeatNodeKind Kind = FeatNodeKind.StatsBonus;
	public Sprite Icon;

	public List<string> NeighbourNodeIds = new List<string>();

	public int NumberOfRepeatTime = 0;

	[SerializeReference]
	public List<FeatRequirement> Requirements = new List<FeatRequirement>();

	[SerializeReference]
	public List<FeatReward> Rewards = new List<FeatReward>();
}