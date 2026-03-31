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

	public CreatureForm GetForm()
	{
		if (Species == null)
		{
			throw new InvalidOperationException("CreatureUnit has no Species.");
		}

		if (string.IsNullOrEmpty(CurrentFormID))
		{
			throw new InvalidOperationException(
				$"CreatureUnit of species [{Species.name}] has no CurrentFormID.");
		}

		if (Species.Forms == null)
		{
			throw new InvalidOperationException(
				$"CreatureSpecies [{Species.name}] has no Forms dictionary.");
		}

		if (Species.Forms.TryGetValue(CurrentFormID, out CreatureForm form) == false)
		{
			throw new InvalidOperationException(
				$"CreatureSpecies [{Species.name}] does not contain form id [{CurrentFormID}].");
		}

		return form;
	}
};
