using UnityEngine;

namespace Erelia.Battle.Phase.Placement.UI
{
	public sealed class SelectableCreatureCardGroupElement : Erelia.Core.UI.CreatureCardGroupElement
	{
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

		public override void PopulateCreatureCards(Erelia.Core.Creature.Team team)
		{
			selectedCreature = null;
			base.PopulateCreatureCards(team);
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
	}
}
