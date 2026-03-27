using UnityEngine;
using UnityEngine.Serialization;

namespace Erelia.Core.Creature
{
	[CreateAssetMenu(menuName = "Creature/Species", fileName = "NewSpecies")]
	public sealed class Species : ScriptableObject
	{
		[SerializeField] private Sprite icon;

		[FormerlySerializedAs("prefab")]
		[SerializeField] private GameObject unitPrefab;

		[SerializeField] private string displayName;

		[SerializeField] private Stats stats = new Stats(10, 5f, 6, 3);

		[SerializeField] private Erelia.Core.Creature.FeatBoard featBoard;

		public Sprite Icon => icon;

		public GameObject UnitPrefab => unitPrefab;

		public GameObject Prefab => UnitPrefab;

		public string DisplayName => displayName;

		public Stats Stats => stats ??= new Stats(10, 5f, 6, 3);

		public Erelia.Core.Creature.FeatBoard FeatBoard => featBoard;
	}
}
