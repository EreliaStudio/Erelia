using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Phase.Placement.UI
{
	public class PlacedCreatureSlot : MonoBehaviour
	{
		[SerializeField] private Image icon = null;
		[SerializeField] private Button button = null;
		[SerializeField] private Sprite undefinedIcon = null;

		public void Configure(Sprite sprite, bool isFilled, Action onClick)
		{
			if (icon != null)
			{
				icon.sprite = isFilled ? sprite : undefinedIcon;
				icon.enabled = icon.sprite != null;
			}

			if (button != null)
			{
				button.onClick.RemoveAllListeners();
				button.interactable = isFilled && onClick != null;
				if (button.interactable)
				{
					button.onClick.AddListener(() => onClick());
				}
			}
		}
	}
}
