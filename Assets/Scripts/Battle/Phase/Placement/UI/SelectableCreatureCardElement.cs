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

		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnCreaturePlaced);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced>(OnCreatureUnplaced);

			RefreshBackgroundColor();
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected>(OnCreatureSelected);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced>(OnCreaturePlaced);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced>(OnCreatureUnplaced);
		}

		public override void LinkCreature(Erelia.Core.Creature.Instance.Model model)
		{
			isSelected = false;
			base.LinkCreature(model);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected(LinkedCreature));
		}

		private void OnCreatureSelected(Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected evt)
		{
			isSelected = evt.Creature != null && ReferenceEquals(evt.Creature, LinkedCreature);
			RefreshBackgroundColor();
		}

		private void OnCreaturePlaced(Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced evt)
		{
			bool isThisCreature = ReferenceEquals(evt.Creature, LinkedCreature);

			if (isThisCreature == true)
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

		private void OnCreatureUnplaced(Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced evt)
		{
			if (ReferenceEquals(evt.Creature, LinkedCreature) == false)
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
