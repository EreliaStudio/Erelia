using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class AbilityCardElementUI :
	MonoBehaviour,
	IPointerEnterHandler,
	IPointerExitHandler
{
	[SerializeField] private Image iconImage;
	[SerializeField] private TMP_Text shortcutLabel;
	[SerializeField] private bool createShortcutLabelIfMissing = true;

	private Ability linkedAbility;
	private int slotIndex = -1;
	private bool showShortcutLabel;

	public event Action<AbilityCardElementUI, Ability, int> Hovered;
	public event Action<AbilityCardElementUI> HoverEnded;

	private void Awake()
	{
		ResolveReferences();
		ApplyState();
	}

	public void Bind(Ability p_ability)
	{
		Bind(p_ability, -1, false);
	}

	public void Bind(Ability p_ability, int p_slotIndex, bool p_showShortcutLabel)
	{
		linkedAbility = p_ability;
		slotIndex = p_slotIndex;
		showShortcutLabel = p_showShortcutLabel;
		ApplyState();
	}

	public void Clear()
	{
		Bind(null, -1, false);
	}

	public void OnPointerEnter(PointerEventData p_eventData)
	{
		if (linkedAbility == null)
		{
			return;
		}

		Hovered?.Invoke(this, linkedAbility, slotIndex);
	}

	public void OnPointerExit(PointerEventData p_eventData)
	{
		HoverEnded?.Invoke(this);
	}

	private void ApplyState()
	{
		ResolveReferences();

		Sprite icon = linkedAbility != null ? linkedAbility.Icon : null;
		iconImage.sprite = icon;
		iconImage.enabled = icon != null;

		if (shortcutLabel == null)
		{
			return;
		}

		bool shouldShowShortcut = showShortcutLabel && slotIndex >= 0;
		shortcutLabel.gameObject.SetActive(shouldShowShortcut);
		shortcutLabel.text = shouldShowShortcut
			? (slotIndex + 1).ToString()
			: string.Empty;
	}

	private void ResolveReferences()
	{
		iconImage ??= GetComponent<Image>();
		shortcutLabel ??= GetComponentInChildren<TMP_Text>(true);

		if (shortcutLabel == null && createShortcutLabelIfMissing)
		{
			shortcutLabel = CreateShortcutLabel();
		}
	}

	private TMP_Text CreateShortcutLabel()
	{
		GameObject labelObject = new GameObject("ShortcutLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		labelObject.layer = gameObject.layer;
		labelObject.transform.SetParent(transform, false);
		labelObject.transform.SetAsLastSibling();

		RectTransform rectTransform = (RectTransform) labelObject.transform;
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(0f, 1f);
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.anchoredPosition = new Vector2(4f, -4f);
		rectTransform.sizeDelta = new Vector2(20f, 12f);

		TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
		text.font = TMP_Settings.defaultFontAsset;
		text.fontSize = 10f;
		text.alignment = TextAlignmentOptions.TopLeft;
		text.color = Color.white;
		text.raycastTarget = false;
		text.enableAutoSizing = false;
		text.textWrappingMode = TextWrappingModes.NoWrap;
		text.text = string.Empty;
		labelObject.SetActive(false);
		return text;
	}
}
