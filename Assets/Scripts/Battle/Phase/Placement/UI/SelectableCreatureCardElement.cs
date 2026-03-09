using UnityEngine;
using UnityEngine.EventSystems;

namespace Erelia.Battle.Phase.Placement.UI
{
	public class SelectableCreatureCardElement :
		Erelia.Core.UI.CreatureCardElement,
		IPointerClickHandler
	{
		[SerializeField] private Color idleColor = Color.white;
		[SerializeField] private Color selectedColor = Color.green;
		[SerializeField] private Color placedColor = Color.gray;

		private bool isSelected;
		private bool isPlaced;

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
			base.LinkCreature(model);

			isSelected = false;
			isPlaced = false;
			RefreshBackgroundColor();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (LinkedCreature == null)
			{
				return;
			}

			if (isPlaced == true)
			{
				return;
			}

			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected(LinkedCreature));
		}

		private void OnCreatureSelected(Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected evt)
		{
			if (isPlaced == true)
			{
				isSelected = false;
				RefreshBackgroundColor();
				return;
			}

			isSelected = ReferenceEquals(evt.Creature, LinkedCreature);
			RefreshBackgroundColor();
		}

		private void OnCreaturePlaced(Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced evt)
		{
			bool isThisCreature = ReferenceEquals(evt.Creature, LinkedCreature);

			if (isThisCreature == true)
			{
				isPlaced = true;
				isSelected = false;
			}
			else
			{
				isSelected = false;
			}

			RefreshBackgroundColor();
		}

		private void OnCreatureUnplaced(Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced evt)
		{
			if (ReferenceEquals(evt.Creature, LinkedCreature) == false)
			{
				return;
			}

			isPlaced = false;
			isSelected = false;
			RefreshBackgroundColor();
		}

		private void RefreshBackgroundColor()
		{
			if (isPlaced == true)
			{
				SetBackgroundColor(placedColor);
				return;
			}

			if (isSelected == true)
			{
				SetBackgroundColor(selectedColor);
				return;
			}

			SetBackgroundColor(idleColor);
		}
	}
}