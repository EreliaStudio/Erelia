using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.UI
{
	public sealed class BattleResultHud : MonoBehaviour
	{
		[SerializeField] private GameObject root;
		[SerializeField] private TMP_Text titleLabel;
		[SerializeField] private Button closeFightButton;
		[SerializeField] private RectTransform entryContainer;
		[SerializeField] private Erelia.Battle.UI.BattleResultEntryView entryTemplate;

		private readonly List<Erelia.Battle.UI.BattleResultEntryView> spawnedEntries =
			new List<Erelia.Battle.UI.BattleResultEntryView>();
		private bool isCloseButtonBound;

		public event Action CloseRequested;

		public void Show(string title, IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits)
		{
			EnsureCloseButtonBinding();

			if (titleLabel != null)
			{
				titleLabel.text = title ?? string.Empty;
			}

			RebuildEntries(playerUnits);
			if (closeFightButton != null)
			{
				closeFightButton.interactable = true;
				closeFightButton.Select();
			}

			if (root != null && !root.activeSelf)
			{
				root.SetActive(true);
			}
		}

		public void Hide()
		{
			ClearEntries();

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

		private void RebuildEntries(IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits)
		{
			ClearEntries();
			if (entryTemplate == null || entryContainer == null)
			{
				return;
			}

			entryTemplate.gameObject.SetActive(false);

			if (playerUnits == null)
			{
				return;
			}

			for (int i = 0; i < playerUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = playerUnits[i];
				if (unit == null)
				{
					continue;
				}

				Erelia.Battle.UI.BattleResultEntryView entry =
					Instantiate(entryTemplate, entryContainer, false);
				entry.gameObject.name = $"BattleResultEntry ({i + 1})";
				entry.gameObject.SetActive(true);
				entry.Apply(
					ResolveCreatureName(unit),
					BuildPlaceholderLine("Garbish stat A", i + 1),
					BuildPlaceholderLine("Garbish stat B", (i + 1) * 2));
				spawnedEntries.Add(entry);
			}
		}

		private void ClearEntries()
		{
			for (int i = 0; i < spawnedEntries.Count; i++)
			{
				Erelia.Battle.UI.BattleResultEntryView entry = spawnedEntries[i];
				if (entry == null)
				{
					continue;
				}

				if (Application.isPlaying)
				{
					Destroy(entry.gameObject);
				}
				else
				{
					DestroyImmediate(entry.gameObject);
				}
			}

			spawnedEntries.Clear();
		}

		private static string ResolveCreatureName(Erelia.Battle.Unit.Presenter unit)
		{
			if (unit == null)
			{
				return "Unknown creature";
			}

			if (!string.IsNullOrEmpty(unit.Creature?.DisplayName))
			{
				return unit.Creature.DisplayName;
			}

			return string.IsNullOrEmpty(unit.name) ? "Unknown creature" : unit.name;
		}

		private static string BuildPlaceholderLine(string label, int value)
		{
			return $"{label}: {value}";
		}
	}
}
