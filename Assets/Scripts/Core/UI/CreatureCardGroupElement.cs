using UnityEngine;

namespace Erelia.Core.UI
{
	public class CreatureCardGroupElement : MonoBehaviour
	{
		[SerializeField] private CreatureCardElement[] cardElements =
			new CreatureCardElement[Erelia.Core.Creature.Team.DefaultSize];

		protected CreatureCardElement[] CardElements => cardElements;

		public virtual void PopulateCreatureCards(Erelia.Core.Creature.Team team)
		{
			Erelia.Core.Creature.Instance.Model[] slots = team?.Slots;
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

				Erelia.Core.Creature.Instance.Model creature =
					slots != null && i < slots.Length ? slots[i] : null;
				cardElements[i].LinkCreature(creature);
			}
		}

		public bool ContainsLinkedCreature(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null || cardElements == null)
			{
				return false;
			}

			for (int i = 0; i < cardElements.Length; i++)
			{
				if (cardElements[i] != null && ReferenceEquals(cardElements[i].LinkedCreature, creature))
				{
					return true;
				}
			}

			return false;
		}
	}
}
