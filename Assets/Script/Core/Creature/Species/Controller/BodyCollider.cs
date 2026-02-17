using System;
using UnityEngine;

namespace Core.Creature.Species.Controller
{
	[Serializable]
	public class BodyCollider
	{
		[SerializeField] private GameObject colliderPrefab = null;

		public GameObject ColliderPrefab => colliderPrefab;
	}
}
