using UnityEngine;
using UnityEngine.EventSystems;

namespace Erelia.Battle.Phase.Placement.UI
{
	public class SelectableCreatureCardElement :
		Erelia.Battle.Phase.Core.UI.CreatureCardElement,
		IPointerClickHandler
	{
		[SerializeField] private Color selectedColor = Color.green;

		private bool isSelected;

		protected override void OnEnable()
		{
			base.OnEnable();
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			RefreshBackgroundColor();
		}

		protected override void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			base.OnDisable();
		}

		public override void LinkUnit(Erelia.Battle.Unit.Presenter unitPresenter)
		{
			isSelected = false;
			base.LinkUnit(unitPresenter);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (LinkedUnit == null)
			{
				return;
			}

			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected(LinkedUnit));
		}

		private void OnCreatureSelected(Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected evt)
		{
			isSelected = evt?.Unit != null && ReferenceEquals(evt.Unit, LinkedUnit);
			RefreshBackgroundColor();
		}

		protected override void HandlePlacementCreaturePlaced(
			Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced evt)
		{
			bool isThisCreature = ReferenceEquals(evt?.Unit, LinkedUnit);

			if (isThisCreature)
			{
				isSelected = false;
				SetPlaced(true);
			}
			else
			{
				isSelected = false;
				RefreshBackgroundColor();
			}
		}

		protected override void HandlePlacementCreatureUnplaced(
			Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced evt)
		{
			if (!ReferenceEquals(evt?.Unit, LinkedUnit))
			{
				return;
			}

			isSelected = false;
			SetPlaced(false);
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
