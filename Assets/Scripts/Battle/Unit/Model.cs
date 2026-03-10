using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class Model
	{
		private static readonly Vector3Int UnplacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

		public Model(Erelia.Core.Creature.Instance.Model creature, Erelia.Battle.Side side)
		{
			Creature = creature;
			Side = side;
			Cell = UnplacedCell;
			IsPlaced = false;
		}

		public Erelia.Core.Creature.Instance.Model Creature { get; }
		public Erelia.Battle.Side Side { get; }
		public Vector3Int Cell { get; private set; }
		public bool IsPlaced { get; private set; }
		public string DisplayName => Creature != null ? Creature.DisplayName : string.Empty;

		public bool TryGetSpecies(out Erelia.Core.Creature.Species species)
		{
			species = null;

			if (Creature == null || Creature.IsEmpty)
			{
				return false;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			return registry != null &&
				registry.TryGet(Creature.SpeciesId, out species) &&
				species != null;
		}

		public void Place(Vector3Int cell)
		{
			Cell = cell;
			IsPlaced = true;
		}

		public void Unplace()
		{
			Cell = UnplacedCell;
			IsPlaced = false;
		}
	}
}
