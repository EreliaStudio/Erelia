using UnityEngine;
using UnityEngine.EventSystems;

namespace Erelia.Battle.Phase.Placement.UI
{
	public class SelectableCreatureCardElement :
		Erelia.Core.UI.CreatureCardElement,
		IPointerClickHandler
	{
		[SerializeField] private Color selectedColor = Color.green;

		private bool isSelected;

		protected override void OnEnable()
		{
			base.OnEnable();
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitSelected>(OnUnitSelected);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitPlaced>(OnUnitPlaced);
			RefreshBackgroundColor();
		}

		protected override void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitSelected>(OnUnitSelected);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementUnitPlaced>(OnUnitPlaced);
			base.OnDisable();
		}

		public override void LinkUnit(Erelia.Battle.Unit.Presenter presenter)
		{
			isSelected = false;
			base.LinkUnit(presenter);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (LinkedUnit == null)
			{
				return;
			}

			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementUnitSelected(LinkedUnit));
		}

		public override void ApplySnapshot(Erelia.Battle.Unit.Snapshot snapshot)
		{
			if (snapshot.IsPlaced)
			{
				isSelected = false;
			}

			base.ApplySnapshot(snapshot);
		}

		private void OnUnitSelected(Erelia.Battle.Phase.Placement.Event.PlacementUnitSelected evt)
		{
			isSelected = evt.Unit != null && ReferenceEquals(evt.Unit, LinkedUnit);
			RefreshBackgroundColor();
		}

		private void OnUnitPlaced(Erelia.Battle.Phase.Placement.Event.PlacementUnitPlaced evt)
		{
			if (!ReferenceEquals(evt?.Unit, LinkedUnit))
			{
				return;
			}

			isSelected = false;
			RefreshBackgroundColor();
		}

		protected override bool TryGetOverrideBackgroundColor(out Color color)
		{
			if (isSelected == true)
			{
				color = selectedColor;
				return true;
			}

			color = default;
			return false;
		}
	}
}
