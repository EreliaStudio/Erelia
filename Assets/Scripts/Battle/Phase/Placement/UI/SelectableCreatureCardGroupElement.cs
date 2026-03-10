using UnityEngine;

namespace Erelia.Battle.Phase.Placement.UI
{
	public sealed class SelectableCreatureCardGroupElement : Erelia.Battle.Phase.Core.UI.CreatureCardGroupElement
	{
		private Erelia.Battle.Unit.Presenter selectedUnit;

		public Erelia.Battle.Unit.Presenter SelectedUnit => selectedUnit;

		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnCreaturePlaced);
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnCreaturePlaced);
			selectedUnit = null;
		}

		public override void PopulateUnits(System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> units)
		{
			selectedUnit = null;
			base.PopulateUnits(units);
		}

		public override void ClearUnits()
		{
			selectedUnit = null;
			base.ClearUnits();
		}

		public Erelia.Battle.Unit.Presenter GetSelectedUnit()
		{
			return selectedUnit;
		}

		public bool TryGetSelectedUnit(out Erelia.Battle.Unit.Presenter unit)
		{
			unit = selectedUnit;
			return unit != null;
		}

		public Erelia.Core.Creature.Instance.Model GetSelectedCreature()
		{
			return selectedUnit?.Model?.Creature;
		}

		private void OnCreatureSelected(Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected evt)
		{
			selectedUnit = ContainsLinkedUnit(evt?.Unit) ? evt.Unit : null;
		}

		private void OnCreaturePlaced(Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced evt)
		{
			if (ReferenceEquals(evt?.Unit, selectedUnit))
			{
				selectedUnit = null;
			}
		}
	}
}
