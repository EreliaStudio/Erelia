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

	public CreatureUnit()
	{
		FeatProgressionService.InitializeCreatureUnit(this);
	}

	public bool HasForm(string p_formID)
	{
		if (Species == null || Species.Forms == null || string.IsNullOrEmpty(p_formID))
		{
			return false;
		}

		return Species.Forms.ContainsKey(p_formID);
	}

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

	public bool TryGetForm(out CreatureForm p_form)
	{
		p_form = null;

		if (Species == null ||
			string.IsNullOrEmpty(CurrentFormID) ||
			Species.Forms == null ||
			!Species.Forms.TryGetValue(CurrentFormID, out CreatureForm form))
		{
			return false;
		}

		p_form = form;
		return true;
	}
}
