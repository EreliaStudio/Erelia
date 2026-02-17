using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Placement.UI
{
	public class TeamPlacementPanel : MonoBehaviour
	{
		[SerializeField] private RectTransform contentRoot = null;
		[SerializeField] private TeamPlacementSlotView slotPrefab = null;
		[SerializeField] private global::Battle.Phase.Placement.View.PlacedCreaturePanel placedCreaturePanel = null;
		[SerializeField] private Button validateButton = null;
		[SerializeField] private bool hideWhenUnbound = true;

		private readonly List<TeamPlacementSlotView> slotViews = new List<TeamPlacementSlotView>();
		private global::Battle.Phase.Placement.Phase phase;

		public void Bind(global::Battle.Phase.Placement.Phase placementPhase)
		{
			Unbind();

			phase = placementPhase;
			if (phase == null)
			{
				if (hideWhenUnbound)
				{
					gameObject.SetActive(false);
				}

				return;
			}

			phase.ActiveSlotChanged += HandleActiveSlotChanged;
			phase.PlacementChanged += HandlePlacementChanged;

			RebuildSlots();
			if (placedCreaturePanel != null)
			{
				placedCreaturePanel.Bind(phase);
			}
			Refresh();
			BindValidateButton();

			if (hideWhenUnbound)
			{
				gameObject.SetActive(true);
			}
		}

		public void Unbind()
		{
			if (phase != null)
			{
				phase.ActiveSlotChanged -= HandleActiveSlotChanged;
				phase.PlacementChanged -= HandlePlacementChanged;
				phase = null;
			}

			UnbindValidateButton();
			ClearSlots();
			if (placedCreaturePanel != null)
			{
				placedCreaturePanel.Unbind();
			}

			if (hideWhenUnbound)
			{
				gameObject.SetActive(false);
			}
		}

		private void RebuildSlots()
		{
			ClearSlots();

			if (contentRoot == null || slotPrefab == null || phase == null || phase.PlayerPlacement == null)
			{
				return;
			}

			var placement = phase.PlayerPlacement;
			for (int i = 0; i < placement.SlotCount; i++)
			{
				var instance = placement.Instances[i];
				var view = Instantiate(slotPrefab, contentRoot);
				view.Initialize(instance.Source, i, HandleSlotClicked);
				slotViews.Add(view);
			}
		}

		private void ClearSlots()
		{
			for (int i = 0; i < slotViews.Count; i++)
			{
				if (slotViews[i] != null)
				{
					Destroy(slotViews[i].gameObject);
				}
			}

			slotViews.Clear();
		}

		private void Refresh()
		{
			if (phase == null || phase.PlayerPlacement == null)
			{
				return;
			}

			int activeIndex = phase.ActiveSlotIndex;
			var instances = phase.PlayerPlacement.Instances;
			for (int i = 0; i < slotViews.Count && i < instances.Count; i++)
			{
				bool isSelected = i == activeIndex;
				bool isPlaced = instances[i].HasPlacement;
				slotViews[i].SetState(isSelected, isPlaced);
			}

			UpdatePlacementControls();
		}

		private void HandleSlotClicked(int slotIndex)
		{
			if (phase == null)
			{
				return;
			}

			if (phase.TrySetActiveSlot(slotIndex))
			{
				Refresh();
			}
		}

		private void HandleActiveSlotChanged(int slotIndex)
		{
			Refresh();
		}

		private void HandlePlacementChanged()
		{
			Refresh();
		}

		private void BindValidateButton()
		{
			if (validateButton == null)
			{
				return;
			}

			validateButton.onClick.RemoveAllListeners();
			validateButton.onClick.AddListener(HandleValidateClicked);
			UpdatePlacementControls();
		}

		private void UnbindValidateButton()
		{
			if (validateButton == null)
			{
				return;
			}

			validateButton.onClick.RemoveAllListeners();
		}

		private void HandleValidateClicked()
		{
			if (phase == null)
			{
				return;
			}

			phase.TryValidatePlacement();
		}

		private void UpdatePlacementControls()
		{
			if (phase == null || phase.PlayerPlacement == null)
			{
				if (validateButton != null)
				{
					validateButton.interactable = false;
				}

				return;
			}

			int placed = phase.PlayerPlacement.PlacedCount;
			int max = phase.PlayerPlacement.MaxPlacements;

			if (validateButton != null)
			{
				validateButton.interactable = placed > 0;
			}
		}

		private void OnDisable()
		{
			Unbind();
		}
	}
}
