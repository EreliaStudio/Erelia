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
			Species.Forms.TryGetValue(CurrentFormID, out CreatureForm form) == false)
		{
			return false;
		}

		p_form = form;
		return true;
	}

	public IReadOnlyList<Ability> GetAbilities()
	{
		EnsureAbilityList();
		return Abilities;
	}

	public void SetAbilities(IReadOnlyList<Ability> p_abilities)
	{
		EnsureAbilityList();
		Abilities.Clear();

		AddAbilities(p_abilities);
	}

	public void InitializeDefaultAbilities()
	{
		EnsureAbilityList();
		AddAbilities(Species?.DefaultAbilities);
	}

	public bool HasAbility(Ability p_ability)
	{
		if (p_ability == null)
		{
			return false;
		}

		return ContainsAbility(Abilities, p_ability);
	}

	public void AddAbility(Ability p_ability)
	{
		if (p_ability == null)
		{
			return;
		}

		EnsureAbilityList();

		if (ContainsAbility(Abilities, p_ability) == false)
		{
			Abilities.Add(p_ability);
		}
	}

	public void AddAbilities(IReadOnlyList<Ability> p_abilities)
	{
		if (p_abilities == null)
		{
			return;
		}

		for (int index = 0; index < p_abilities.Count; index++)
		{
			AddAbility(p_abilities[index]);
		}
	}

	public void RemoveAbility(Ability p_ability)
	{
		if (p_ability == null || Abilities == null)
		{
			return;
		}

		for (int index = Abilities.Count - 1; index >= 0; index--)
		{
			if (Abilities[index] == p_ability)
			{
				Abilities.RemoveAt(index);
			}
		}
	}

	public void RemoveAbilities(IReadOnlyList<Ability> p_abilities)
	{
		if (p_abilities == null)
		{
			return;
		}

		for (int index = 0; index < p_abilities.Count; index++)
		{
			RemoveAbility(p_abilities[index]);
		}
	}

	private void EnsureAbilityList()
	{
		if (Abilities == null)
		{
			Abilities = new List<Ability>();
		}
	}

	private static bool ContainsAbility(IReadOnlyList<Ability> p_abilities, Ability p_ability)
	{
		if (p_abilities == null || p_ability == null)
		{
			return false;
		}

		for (int index = 0; index < p_abilities.Count; index++)
		{
			if (p_abilities[index] == p_ability)
			{
				return true;
			}
		}

		return false;
	}
}