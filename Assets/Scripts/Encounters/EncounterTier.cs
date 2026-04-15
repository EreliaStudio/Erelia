using System;
using System.Collections.Generic;

[Serializable]
public class EncounterTier
{
	[Serializable]
	public class Entry
	{
		public string DisplayName = "new team";
		public EncounterUnit[] Team = new EncounterUnit[GameRule.TeamMemberCount];
		public int Weight = 1;
	}

	public List<Entry> WeightedTeams = new List<Entry>();
}
