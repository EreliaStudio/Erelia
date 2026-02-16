using System;
using UnityEngine;

namespace Core.Creature.Model
{
	[Serializable]
	public class Definition
	{
		[SerializeField] private Species species = null;
		[SerializeField] private string nickName = "";

		public Species Species => species;
		public string NickName => nickName;
	}
}
