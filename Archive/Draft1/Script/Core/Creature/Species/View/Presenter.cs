using System;
using UnityEngine;

namespace Core.Creature.Species.View
{
	[Serializable]
	public class Presenter
	{
		[SerializeField] private GameObject modelPrefab = null;
		[SerializeField] private Sprite icon = null;

		public GameObject ModelPrefab => modelPrefab;
		public Sprite Icon => icon;
	}
}
