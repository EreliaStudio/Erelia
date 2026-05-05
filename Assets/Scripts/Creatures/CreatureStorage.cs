using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CreatureStorage
{
	[SerializeReference]
	private List<CreatureUnit> storedCreatures = new List<CreatureUnit>();

	public IReadOnlyList<CreatureUnit> StoredCreatures => storedCreatures;

	public int Count => storedCreatures?.Count ?? 0;

	public void Add(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null)
		{
			return;
		}

		EnsureInitialized();
		storedCreatures.Add(p_creatureUnit);
	}

	public bool Remove(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null)
		{
			return false;
		}

		EnsureInitialized();
		return storedCreatures.Remove(p_creatureUnit);
	}

	public bool Contains(CreatureUnit p_creatureUnit)
	{
		if (p_creatureUnit == null)
		{
			return false;
		}

		EnsureInitialized();
		return storedCreatures.Contains(p_creatureUnit);
	}

	public CreatureUnit GetAt(int p_index)
	{
		EnsureInitialized();

		if (p_index < 0 || p_index >= storedCreatures.Count)
		{
			return null;
		}

		return storedCreatures[p_index];
	}

	public void Clear()
	{
		EnsureInitialized();
		storedCreatures.Clear();
	}

	public CreatureStorage Clone()
	{
		CreatureStorage clone = new CreatureStorage();

		EnsureInitialized();
		for (int index = 0; index < storedCreatures.Count; index++)
		{
			if (storedCreatures[index] != null)
			{
				clone.Add(storedCreatures[index]);
			}
		}

		return clone;
	}

	private void EnsureInitialized()
	{
		storedCreatures ??= new List<CreatureUnit>();
	}
}