using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TamingProfile
{
	[SerializeReference]
	public List<FeatRequirement> Conditions = new List<FeatRequirement>();

	public bool HasConditions => Conditions != null && Conditions.Count > 0;

	public void EnsureInitialized()
	{
		Conditions ??= new List<FeatRequirement>();
	}
}