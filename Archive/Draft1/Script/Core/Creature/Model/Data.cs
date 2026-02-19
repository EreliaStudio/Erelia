using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Core.Creature.Model
{
	[Serializable]
	[MovedFrom(true, sourceNamespace: "Core.Creature.Model", sourceAssembly: "Assembly-CSharp", sourceClassName: "Definition")]
	public class Data
	{
		[SerializeField] private string nickName = "";

		public string NickName => nickName;
	}
}
