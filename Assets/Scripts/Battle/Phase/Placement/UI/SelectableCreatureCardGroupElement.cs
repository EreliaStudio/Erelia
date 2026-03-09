using UnityEngine;

namespace Erelia.Battle.Phase.Placement.UI
{
	public sealed class SelectableCreatureCardGroupElement : MonoBehaviour
	{
		[SerializeField] private SelectableCreatureCardElement[] cardElements =
			new SelectableCreatureCardElement[Erelia.Core.Creature.Team.DefaultSize];

		private Erelia.Core.Creature.Instance.Model selectedCreature;

		public Erelia.Core.Creature.Instance.Model SelectedCreature => selectedCreature;

		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnCreaturePlaced);
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnCreaturePlaced);
			selectedCreature = null;
		}

		public void PopulateCreatureCards(Erelia.Core.Creature.Team team)
		{
			selectedCreature = null;

			Erelia.Core.Creature.Instance.Model[] slots = team?.Slots;
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

		public Erelia.Core.Creature.Instance.Model GetSelectedCreature()
		{
			return selectedCreature;
		}

		public bool TryGetSelectedCreature(out Erelia.Core.Creature.Instance.Model creature)
		{
			creature = selectedCreature;
			return creature != null;
		}

		private void OnCreatureSelected(Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected evt)
		{
			selectedCreature = ContainsCreature(evt.Creature) ? evt.Creature : null;
		}

		private void OnCreaturePlaced(Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced evt)
		{
			if (ReferenceEquals(evt.Creature, selectedCreature))
			{
				selectedCreature = null;
			}
		}

		private bool ContainsCreature(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null)
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
