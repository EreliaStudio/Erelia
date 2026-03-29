using System;
using System.Collections.Generic;

[Serializable]
public class Player
{
	public List<Badge> ObtainedBadges = new List<Badge>();
	public CreatureUnit[] Team = new CreatureUnit[6];
};