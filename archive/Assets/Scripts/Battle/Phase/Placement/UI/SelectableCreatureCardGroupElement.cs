using UnityEngine;

namespace Erelia.Battle.Phase.Placement.UI
{
	public sealed class SelectableCreatureCardGroupElement : Erelia.Core.UI.CreatureCardGroupElement
	{
		private Erelia.Battle.Unit.Presenter selectedUnit;

		public Erelia.Battle.Unit.Presenter SelectedUnit => selectedUnit;

		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitSelected>(OnUnitSelected);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitPlaced>(OnUnitPlaced);
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitSelected>(OnUnitSelected);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitPlaced>(OnUnitPlaced);
			selectedUnit = null;
		}

		public override void PopulateUnits(System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> units)
		{
			selectedUnit = null;
			base.PopulateUnits(units);
		}

		public Erelia.Battle.Unit.Presenter GetSelectedUnit()
		{
			return selectedUnit;
		}

		public bool TryGetSelectedUnit(out Erelia.Battle.Unit.Presenter unit)
		{
			unit = GetSelectedUnit();
			return unit != null;
		}

		private void OnUnitSelected(Erelia.Battle.Phase.Placement.Event.PlacementUnitSelected evt)
		{
			selectedUnit = evt.Unit != null && ContainsLinkedUnit(evt.Unit)
				? evt.Unit
				: null;
		}

		private void OnUnitPlaced(Erelia.Battle.Phase.Placement.Event.PlacementUnitPlaced evt)
		{
			if (ReferenceEquals(evt.Unit, selectedUnit))
			{
				selectedUnit = null;
			}
		}
	}
}
