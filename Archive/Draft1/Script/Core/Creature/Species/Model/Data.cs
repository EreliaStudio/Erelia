using System;
using UnityEngine;

namespace Core.Creature.Species.Model
{
	[Serializable]
	public class Data
	{
		[SerializeField] private string familyName = "DefaultName";

		public string FamilyName => familyName;
	}
}
