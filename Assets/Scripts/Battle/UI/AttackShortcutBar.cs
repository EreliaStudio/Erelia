using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Erelia.Battle.UI
{
	[DisallowMultipleComponent]
	public sealed class AttackShortcutBar : MonoBehaviour
	{
#pragma warning disable 0649
		[Serializable]
		private struct SlotBinding
		{
			public Button Button;
			public TMP_Text Label;
			public Image Icon;
		}
#pragma warning restore 0649

		[SerializeField] private SlotBinding[] slots =
			new SlotBinding[Erelia.Core.Creature.Instance.Model.MaxAttackCount];
		[SerializeField] private Color availableButtonColor = new Color(1f, 1f, 1f, 0.95f);
		[SerializeField] private Color selectedButtonColor = new Color(0.95f, 0.84f, 0.4f, 1f);
		[SerializeField] private Color disabledButtonColor = new Color(0.4f, 0.4f, 0.4f, 0.55f);
		[SerializeField] private Color availableLabelColor = Color.black;
		[SerializeField] private Color selectedLabelColor = Color.black;
		[SerializeField] private Color disabledLabelColor = new Color(0.85f, 0.85f, 0.85f, 0.75f);

		private readonly UnityAction[] clickHandlers =
			new UnityAction[Erelia.Core.Creature.Instance.Model.MaxAttackCount];

		private IReadOnlyList<Erelia.Battle.Attack.Definition> attacks;
		private string[] shortcutLabels = CreateDefaultShortcutLabels();
		private int selectedIndex = -1;
		private bool isInteractable = true;

		public event Action<int> SlotClicked;

		private void Awake()
		{
			EnsureSlotArrayLength();
			BindButtons();
			Refresh();
		}

		private void OnEnable()
		{
			Refresh();
		}

		private void OnDestroy()
		{
			UnbindButtons();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			EnsureSlotArrayLength();

			if (!Application.isPlaying)
			{
				Refresh();
			}
		}
#endif

		public void SetAttacks(IReadOnlyList<Erelia.Battle.Attack.Definition> values)
		{
			attacks = values;
			Refresh();
		}

		public void SetSelectedIndex(int index)
		{
			selectedIndex = index;
			Refresh();
		}

		public void SetShortcutLabels(IReadOnlyList<string> values)
		{
			shortcutLabels = CreateDefaultShortcutLabels();
			if (values != null)
			{
				for (int i = 0; i < shortcutLabels.Length && i < values.Count; i++)
				{
					if (!string.IsNullOrWhiteSpace(values[i]))
					{
						shortcutLabels[i] = values[i];
					}
				}
			}

			Refresh();
		}

		public void SetInteractable(bool value)
		{
			isInteractable = value;
			Refresh();
		}

		public void Clear()
		{
			attacks = null;
			selectedIndex = -1;
			isInteractable = false;
			Refresh();
		}

		private void BindButtons()
		{
			for (int i = 0; i < slots.Length; i++)
			{
				Button button = slots[i].Button;
				if (button == null)
				{
					continue;
				}

				if (clickHandlers[i] != null)
				{
					button.onClick.RemoveListener(clickHandlers[i]);
				}

				int slotIndex = i;
				clickHandlers[i] = () => SlotClicked?.Invoke(slotIndex);
				button.onClick.AddListener(clickHandlers[i]);
			}
		}

		private void UnbindButtons()
		{
			for (int i = 0; i < slots.Length; i++)
			{
				Button button = slots[i].Button;
				UnityAction clickHandler = clickHandlers[i];
				if (button == null || clickHandler == null)
				{
					continue;
				}

				button.onClick.RemoveListener(clickHandler);
				clickHandlers[i] = null;
			}
		}

		private void Refresh()
		{
			EnsureSlotArrayLength();

			for (int i = 0; i < slots.Length; i++)
			{
				RefreshSlot(i);
			}
		}

		private void RefreshSlot(int index)
		{
			SlotBinding slot = slots[index];
			Button button = slot.Button;
			if (button == null)
			{
				return;
			}

			Erelia.Battle.Attack.Definition attack =
				attacks != null && index < attacks.Count ? attacks[index] : null;
			bool hasAttack = attack != null;
			bool isSelected = hasAttack && index == selectedIndex;
			button.interactable = isInteractable && hasAttack;

			string labelValue = ResolveShortcutLabel(index);
			if (slot.Label != null)
			{
				slot.Label.text = labelValue;
				slot.Label.color = !hasAttack
					? disabledLabelColor
					: isSelected
						? selectedLabelColor
						: availableLabelColor;
			}

			if (slot.Icon != null)
			{
				slot.Icon.sprite = hasAttack ? attack.Icon : null;
				slot.Icon.enabled = hasAttack && attack.Icon != null;
			}

			Color buttonColor = !hasAttack
				? disabledButtonColor
				: isSelected
					? selectedButtonColor
					: availableButtonColor;
			ApplyButtonColors(button, buttonColor);
		}

		private void ApplyButtonColors(Button button, Color baseColor)
		{
			ColorBlock colors = button.colors;
			colors.normalColor = baseColor;
			colors.selectedColor = baseColor;
			colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.12f);
			colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.12f);
			colors.disabledColor = disabledButtonColor;
			button.colors = colors;
		}

		private void EnsureSlotArrayLength()
		{
			if (slots != null && slots.Length == Erelia.Core.Creature.Instance.Model.MaxAttackCount)
			{
				return;
			}

			SlotBinding[] resized = new SlotBinding[Erelia.Core.Creature.Instance.Model.MaxAttackCount];
			if (slots != null)
			{
				Array.Copy(slots, resized, Mathf.Min(slots.Length, resized.Length));
			}

			slots = resized;
		}

		private string ResolveShortcutLabel(int index)
		{
			if (shortcutLabels != null &&
				index >= 0 &&
				index < shortcutLabels.Length &&
				!string.IsNullOrWhiteSpace(shortcutLabels[index]))
			{
				return shortcutLabels[index];
			}

			return (index + 1).ToString();
		}

		private static string[] CreateDefaultShortcutLabels()
		{
			var labels = new string[Erelia.Core.Creature.Instance.Model.MaxAttackCount];
			for (int i = 0; i < labels.Length; i++)
			{
				labels[i] = (i + 1).ToString();
			}

			return labels;
		}
	}
}
