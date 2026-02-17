using System.Collections.Generic;
using UnityEngine;

namespace Battle.Phase.Placement.View
{
	public class PlacedCreaturePanel : MonoBehaviour
	{
		[SerializeField] private RectTransform contentRoot = null;
		[SerializeField] private PlacedCreatureSlot slotPrefab = null;

		private readonly List<PlacedCreatureSlot> slotViews = new List<PlacedCreatureSlot>();
		private readonly List<int> placementMapping = new List<int>();
		private global::Battle.Phase.Placement.Phase phase;

		public void Bind(global::Battle.Phase.Placement.Phase placementPhase)
		{
			Unbind();

			phase = placementPhase;
			if (phase == null)
			{
				return;
			}

			phase.PlacementChanged += HandlePlacementChanged;
			RebuildSlots();
			Refresh();
		}

		public void Unbind()
		{
			if (phase != null)
			{
				phase.PlacementChanged -= HandlePlacementChanged;
				phase = null;
			}

			ClearSlots();
		}

		private void RebuildSlots()
		{
			ClearSlots();

			if (contentRoot == null || slotPrefab == null || phase == null || phase.PlayerPlacement == null)
			{
				return;
			}

			int maxPlacements = phase.PlayerPlacement.MaxPlacements;
			for (int i = 0; i < maxPlacements; i++)
			{
				var view = Instantiate(slotPrefab, contentRoot);
				view.Configure(null, false, null);
				slotViews.Add(view);
			}

			placementMapping.Clear();
			for (int i = 0; i < maxPlacements; i++)
			{
				placementMapping.Add(-1);
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
			placementMapping.Clear();
		}

		private void Refresh()
		{
			if (phase == null || phase.PlayerPlacement == null)
			{
				return;
			}

			var instances = phase.PlayerPlacement.Instances;
			placementMapping.Clear();
			for (int i = 0; i < slotViews.Count; i++)
			{
				placementMapping.Add(-1);
			}

			int filledCount = 0;
			for (int i = 0; i < instances.Count && filledCount < slotViews.Count; i++)
			{
				if (!instances[i].HasPlacement)
				{
					continue;
				}

				placementMapping[filledCount] = i;
				filledCount++;
			}

			for (int i = 0; i < slotViews.Count; i++)
			{
				int slotIndex = placementMapping[i];
				if (slotIndex < 0 || slotIndex >= instances.Count)
				{
					slotViews[i].Configure(null, false, null);
					continue;
				}

				var definition = instances[slotIndex].Source;
				Sprite icon = definition != null && definition.SpeciesDefinition != null && definition.SpeciesDefinition.Presenter != null
					? definition.SpeciesDefinition.Presenter.Icon
					: null;
				int placementIndex = i;
				slotViews[i].Configure(icon, true, () => HandleSlotClicked(placementIndex));
			}
		}

		private void HandlePlacementChanged()
		{
			Refresh();
		}

		private void HandleSlotClicked(int placementSlotIndex)
		{
			if (phase == null)
			{
				return;
			}

			if (placementSlotIndex < 0 || placementSlotIndex >= placementMapping.Count)
			{
				return;
			}

			int teamSlotIndex = placementMapping[placementSlotIndex];
			if (teamSlotIndex < 0)
			{
				return;
			}

			if (phase.TryClearSlot(teamSlotIndex))
			{
				Refresh();
			}
		}

		private void OnDisable()
		{
			Unbind();
		}
	}
}
