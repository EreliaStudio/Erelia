using System;
using System.Collections.Generic;

[Serializable]
public class CreatureUnit
{
	public CreatureSpecies Species = null;
	public string CurrentFormID = string.Empty;
	public Attributes Attributes = new Attributes();
	public List<Ability> Abilities = new List<Ability>();
	public List<Status> PermanentPassives = new List<Status>();
	public FeatBoardProgress FeatBoardProgress = new FeatBoardProgress();
};
