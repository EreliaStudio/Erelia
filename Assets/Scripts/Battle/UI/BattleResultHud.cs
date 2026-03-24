using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.UI
{
	public sealed class BattleResultHud : MonoBehaviour
	{
		[SerializeField] private GameObject root;
		[SerializeField] private TMP_Text titleLabel;
		[SerializeField] private TMP_Text bodyLabel;
		[SerializeField] private Button closeFightButton;
		private bool isCloseButtonBound;

		public event Action CloseRequested;

		public void Show(string title, IReadOnlyList<Erelia.Battle.BattleResultCreatureData> creatureResults)
		{
			if (root != null && !root.activeSelf)
			{
				root.SetActive(true);
			}

			EnsureCloseButtonBinding();

			if (titleLabel != null)
			{
				titleLabel.text = title ?? string.Empty;
			}

			if (bodyLabel != null)
			{
				bodyLabel.text = BuildBodyText(creatureResults);
			}

			RefreshLayout();

			if (closeFightButton != null)
			{
				closeFightButton.interactable = true;
				closeFightButton.Select();
			}
		}

		public void Hide()
		{
			if (titleLabel != null)
			{
				titleLabel.text = string.Empty;
			}

			if (bodyLabel != null)
			{
				bodyLabel.text = string.Empty;
			}

			if (root != null && root.activeSelf)
			{
				root.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			if (closeFightButton != null && isCloseButtonBound)
			{
				closeFightButton.onClick.RemoveListener(HandleCloseFightClicked);
				isCloseButtonBound = false;
			}
		}

		private void EnsureCloseButtonBinding()
		{
			if (closeFightButton == null || isCloseButtonBound)
			{
				return;
			}

			closeFightButton.onClick.RemoveListener(HandleCloseFightClicked);
			closeFightButton.onClick.AddListener(HandleCloseFightClicked);
			isCloseButtonBound = true;
		}

		private void HandleCloseFightClicked()
		{
			if (closeFightButton != null)
			{
				closeFightButton.interactable = false;
			}

			CloseRequested?.Invoke();
		}

		private void RefreshLayout()
		{
			Canvas.ForceUpdateCanvases();
			if (root != null &&
				root.transform is RectTransform rootTransform &&
				rootTransform.gameObject.activeInHierarchy)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(rootTransform);
			}
			Canvas.ForceUpdateCanvases();
		}

		private static string BuildBodyText(IReadOnlyList<Erelia.Battle.BattleResultCreatureData> creatureResults)
		{
			if (creatureResults == null || creatureResults.Count == 0)
			{
				return "Creature and feat results will appear here once this panel is wired to the final interaction flow.";
			}

			var builder = new StringBuilder(256);
			int shownCreatureCount = 0;
			for (int i = 0; i < creatureResults.Count; i++)
			{
				Erelia.Battle.BattleResultCreatureData creatureData = creatureResults[i];
				if (!creatureData.HasCreature)
				{
					continue;
				}

				if (shownCreatureCount > 0)
				{
					builder.AppendLine();
					builder.AppendLine();
				}

				shownCreatureCount++;
				builder.Append(ResolveCreatureName(creatureData));

				IReadOnlyList<Erelia.Battle.BattleResultEntryData> entries = creatureData.Entries;
				if (entries == null || entries.Count == 0)
				{
					builder.AppendLine();
					builder.Append("No feat progress recorded.");
					continue;
				}

				for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
				{
					Erelia.Battle.BattleResultEntryData entry = entries[entryIndex];
					builder.AppendLine();
					builder.Append("- ");
					builder.Append(string.IsNullOrWhiteSpace(entry.Title) ? "Feat" : entry.Title);

					if (!string.IsNullOrWhiteSpace(entry.ProgressLabel))
					{
						builder.Append(" (");
						builder.Append(entry.ProgressLabel);
						builder.Append(')');
					}

					if (string.IsNullOrWhiteSpace(entry.Description))
					{
						continue;
					}

					builder.AppendLine();
					builder.Append(entry.Description);
				}
			}

			return shownCreatureCount > 0
				? builder.ToString()
				: "Creature and feat results will appear here once this panel is wired to the final interaction flow.";
		}

		private static string ResolveCreatureName(Erelia.Battle.BattleResultCreatureData creatureData)
		{
			if (!string.IsNullOrWhiteSpace(creatureData.CreatureName))
			{
				return creatureData.CreatureName;
			}

			return $"Creature {creatureData.SlotIndex + 1}";
		}
	}
}
