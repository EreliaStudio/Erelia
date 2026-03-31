using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FeatNode
{
	public string DisplayName = "New Feat";
	public Vector2 Position;

	public FeatNodeKind Kind = FeatNodeKind.StatsBonus;
	public Sprite Icon;

	[SerializeReference]
	public List<FeatNode> NeighbourNodes = new List<FeatNode>();

	public int NumberOfRepeatTime = 0;
	public int FormTier = 0;

	[SerializeReference]
	public List<FeatRequirement> Requirements = new List<FeatRequirement>();

	[SerializeReference]
	public List<FeatReward> Rewards = new List<FeatReward>();
}
