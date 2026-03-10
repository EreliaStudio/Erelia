using UnityEngine;

namespace Erelia.Core.UI
{
	public class CreatureCardGroupElement : MonoBehaviour
	{
		[SerializeField] private CreatureCardElement[] cardElements =
			new CreatureCardElement[Erelia.Core.Creature.Team.DefaultSize];

		protected CreatureCardElement[] CardElements => cardElements;

		public virtual void PopulateUnits(System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> units)
		{
			if (cardElements == null)
			{
				return;
			}

			for (int i = 0; i < cardElements.Length; i++)
			{
				if (cardElements[i] == null)
				{
					continue;
				}

				Erelia.Battle.Unit.Presenter unit =
					units != null && i < units.Count ? units[i] : null;
				cardElements[i].LinkUnit(unit);
			}
		}

		public bool ContainsLinkedUnit(Erelia.Battle.Unit.Presenter unit)
		{
			if (unit == null || cardElements == null)
			{
				return false;
			}

			for (int i = 0; i < cardElements.Length; i++)
			{
				if (cardElements[i] != null && ReferenceEquals(cardElements[i].LinkedUnit, unit))
				{
					return true;
				}
			}

			return false;
		}
	}
}
