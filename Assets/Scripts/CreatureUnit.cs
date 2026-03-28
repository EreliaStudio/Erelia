using System.Collections.Generic;

public class CreatureUnit
{
	public CreatureSpecies Species = null;
	public Attributes Attributes = new Attributes();
	public List<Ability> AvailableAbilities = new List<Ability>();
	public FeatBoardProgress FeatBoardProgress = new FeatBoardProgress();
};
