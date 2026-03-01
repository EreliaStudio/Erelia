using UnityEngine;

namespace Erelia.Core.Creature
{
	[CreateAssetMenu(menuName = "Creature/Species", fileName = "NewSpecies")]
	public sealed class Species : ScriptableObject
	{
		[SerializeField] private GameObject prefab;
		[SerializeField] private string displayName;
		[SerializeField] private int baseHp = 10;

		public GameObject Prefab => prefab;
		public string DisplayName => displayName;
		public int BaseHp => baseHp;
	}
}
