using System;
using System.Collections.Generic;

[Serializable]
public class EncounterTier
{
	[Serializable]
	public class Entry
	{
		public EncounterTeam Team = new EncounterTeam();
		public int Weight = 1;
	};
	public List<Entry> WeightedTeams = new List<Entry>();
};
