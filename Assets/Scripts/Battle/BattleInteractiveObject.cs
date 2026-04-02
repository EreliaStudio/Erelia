using System;
using System.Collections.Generic;

[Serializable]
public class BattleInteractiveObject : BattleObject
{
	public InteractionObject InteractionObject;
	public List<string> Tags = new List<string>();
	public Duration RemainingDuration = new Duration();

	public bool HasAnyTag(HashSet<string> p_tags)
	{
		if (Tags == null || Tags.Count == 0 || p_tags == null || p_tags.Count == 0)
		{
			return false;
		}

		for (int index = 0; index < Tags.Count; index++)
		{
			if (p_tags.Contains(Tags[index]))
			{
				return true;
			}
		}

		return false;
	}
}
