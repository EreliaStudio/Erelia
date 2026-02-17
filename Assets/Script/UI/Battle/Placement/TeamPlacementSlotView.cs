using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI.Battle.Placement
{
	public class TeamPlacementSlotView : MonoBehaviour
	{
		[SerializeField] private Image background = null;
		[SerializeField] private Image icon = null;
		[SerializeField] private TMP_Text nameLabel = null;
		[SerializeField] private Button button = null;
 
		[SerializeField] private Color defaultColor = new Color(0.12f, 0.12f, 0.12f, 0.75f);
		[SerializeField] private Color selectedColor = new Color(0.15f, 0.5f, 0.2f, 0.9f);
		[SerializeField] private Color placedColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

		public int SlotIndex { get; private set; }

		public void Initialize(Core.Creature.Definition definition, int slotIndex, Action<int> onClick)
		{
			SlotIndex = slotIndex;
			bool hasCreature = definition != null && definition.SpeciesDefinition != null;

			if (button != null)
			{
				button.onClick.RemoveAllListeners();
				button.interactable = hasCreature;
				if (onClick != null && hasCreature)
				{
					button.onClick.AddListener(() => onClick(slotIndex));
				}
			}

			if (nameLabel != null)
			{
				nameLabel.text = hasCreature ? definition.DisplayName : "-----";
			}

			if (icon != null)
			{
				Sprite sprite = hasCreature &&
					definition.SpeciesDefinition.Presenter != null
					? definition.SpeciesDefinition.Presenter.Icon
					: null;
				icon.sprite = sprite;
				icon.enabled = sprite != null;
			}

			SetState(false, false);
		}

		public void SetState(bool isSelected, bool isPlaced)
		{
			if (background != null)
			{
				if (isSelected)
				{
					background.color = selectedColor;
				}
				else if (isPlaced)
				{
					background.color = placedColor;
				}
				else
				{
					background.color = defaultColor;
				}
			}
		}
	}
}
