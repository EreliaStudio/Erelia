using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
public sealed class AbilityShortcutView : ExecuteAlwaysView, IPointerClickHandler
{
	[SerializeField, Min(0f)] private float padding = 6f;
	[SerializeField] private Vector2 framePadding = new Vector2(2f, 2f);
	[SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.55f);
	[SerializeField] private Color frameColor = new Color(1f, 1f, 1f, 0.10f);
	[SerializeField] private Color iconColor = Color.white;
	[SerializeField] private Color labelColor = Color.white;
	[SerializeField] private Sprite defaultIcon;

	[SerializeField] private Image backgroundImage;
	[SerializeField] private Image frameImage;
	[SerializeField] private Image iconImage;
	[SerializeField] private TextMeshProUGUI shortcutLabel;
	[SerializeField] private TextMeshProUGUI costLabel;

	private Ability boundAbility;
	private int slotIndex;

	public event Action<int, Ability> Clicked;
	public Ability BoundAbility => boundAbility;
	public int SlotIndex => slotIndex;

	private void Reset()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void Awake()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void OnEnable()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
		RefreshBoundState();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
		EnsureHierarchy(true);
		ApplySerializedState();
		RefreshBoundState();
#endif
	}

	private void OnRectTransformDimensionsChange()
	{
		ApplySerializedState();
	}

	public void Bind(Ability ability, int index)
	{
		boundAbility = ability;
		slotIndex = Mathf.Max(0, index);
		RefreshBoundState();
	}

	public void ClearClickListeners()
	{
		Clicked = null;
	}

	public void ConfigureDefaultLayout(float defaultPadding, Vector2 defaultFramePadding)
	{
		padding = Mathf.Max(0f, defaultPadding);
		framePadding = new Vector2(Mathf.Max(0f, defaultFramePadding.x), Mathf.Max(0f, defaultFramePadding.y));
		ApplySerializedState();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData == null || eventData.button != PointerEventData.InputButton.Left || boundAbility == null)
		{
			return;
		}

		Clicked?.Invoke(slotIndex, boundAbility);
	}

	[ContextMenu("Refresh")]
	public void RefreshNow()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
		RefreshBoundState();
	}

	private void EnsureHierarchy(bool allowCreate)
	{
		backgroundImage = ResolveImage(backgroundImage, "Background", transform, allowCreate, ApplyImageDefaults);
		frameImage = ResolveImage(frameImage, "Frame", transform, allowCreate, ApplyImageDefaults);
		iconImage = ResolveIconImage(allowCreate);
		shortcutLabel = ResolveText(shortcutLabel, "ShortcutLabel", transform, allowCreate);
		costLabel = ResolveText(costLabel, "CostLabel", transform, allowCreate);
		DisableLegacyIconBackground();
	}

	private Image ResolveImage(Image current, string childName, Transform parent, bool allowCreate, Action<Image> applyDefaults)
	{
		if (current != null)
		{
			return current;
		}

		Transform child = parent.Find(childName);
		if (child != null && child.TryGetComponent(out Image existing))
		{
			applyDefaults(existing);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild(childName, parent);
		Image image = childObject.AddComponent<Image>();
		applyDefaults(image);
		return image;
	}

	private Image ResolveIconImage(bool allowCreate)
	{
		if (iconImage != null)
		{
			return iconImage;
		}

		Transform child = transform.Find("Icon");
		if (child != null && child.TryGetComponent(out Image existing))
		{
			ApplyIconDefaults(existing);
			return existing;
		}

		Transform legacyBackground = transform.Find("IconBackground");
		Transform legacyIcon = legacyBackground != null ? legacyBackground.Find("Icon") : null;
		if (legacyIcon != null && legacyIcon.TryGetComponent(out Image legacy))
		{
			legacyIcon.SetParent(transform, false);
			ApplyIconDefaults(legacy);
			return legacy;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("Icon", transform);
		Image image = childObject.AddComponent<Image>();
		ApplyIconDefaults(image);
		return image;
	}

	private TextMeshProUGUI ResolveText(TextMeshProUGUI current, string childName, Transform parent, bool allowCreate)
	{
		if (current != null)
		{
			return current;
		}

		Transform child = parent.Find(childName);
		if (child != null && child.TryGetComponent(out TextMeshProUGUI existing))
		{
			EnsureLabelFont(existing);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild(childName, parent);
		TextMeshProUGUI text = childObject.AddComponent<TextMeshProUGUI>();
		ApplyLabelDefaults(text);
		return text;
	}

	private void ApplySerializedState()
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = backgroundColor;
			UiViewUtility.Stretch(backgroundImage.rectTransform);
		}

		if (frameImage != null)
		{
			frameImage.color = frameColor;
			UiViewUtility.Stretch(frameImage.rectTransform);
			frameImage.rectTransform.offsetMin = framePadding;
			frameImage.rectTransform.offsetMax = -framePadding;
		}

		if (iconImage != null)
		{
			iconImage.color = iconColor;
			ApplyIconLayout(iconImage.rectTransform);
		}

		ApplyShortcutLabelLayout();
		ApplyCostLabelLayout();
	}

	private void DisableLegacyIconBackground()
	{
		Transform legacyBackground = transform.Find("IconBackground");
		if (legacyBackground == null)
		{
			return;
		}

		legacyBackground.gameObject.SetActive(false);
	}

	private void RefreshBoundState()
	{
		if (shortcutLabel != null)
		{
			shortcutLabel.text = (slotIndex + 1).ToString();
		}

		if (costLabel != null)
		{
			costLabel.text = BuildCostLabel(boundAbility);
		}

		if (iconImage != null)
		{
			iconImage.sprite = boundAbility != null && boundAbility.Icon != null ? boundAbility.Icon : defaultIcon;
			iconImage.enabled = iconImage.sprite != null;
		}
	}

	private static string BuildCostLabel(Ability ability)
	{
		if (ability?.Cost == null)
		{
			return string.Empty;
		}

		return $"AP {Mathf.Max(0, ability.Cost.Ability)} / MP {Mathf.Max(0, ability.Cost.Movement)}";
	}

	private void ApplyShortcutLabelLayout()
	{
		if (shortcutLabel == null)
		{
			return;
		}

		shortcutLabel.color = labelColor;
		shortcutLabel.alignment = TextAlignmentOptions.TopLeft;
		RectTransform rect = shortcutLabel.rectTransform;
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = new Vector2(padding, padding);
		rect.offsetMax = new Vector2(-padding, -padding);
	}

	private void ApplyCostLabelLayout()
	{
		if (costLabel == null)
		{
			return;
		}

		costLabel.color = labelColor;
		costLabel.alignment = TextAlignmentOptions.BottomRight;
		RectTransform rect = costLabel.rectTransform;
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = new Vector2(padding, padding);
		rect.offsetMax = new Vector2(-padding, -padding);
	}

	private void ApplyIconLayout(RectTransform rect)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = new Vector2(padding, padding);
		rect.offsetMax = new Vector2(-padding, -padding);
	}

	private static void ApplyImageDefaults(Image image)
	{
		image.raycastTarget = true;
		image.type = Image.Type.Simple;
		image.sprite = image.sprite != null ? image.sprite : UiViewUtility.GetDefaultSprite();
	}

	private static void ApplyIconDefaults(Image image)
	{
		image.raycastTarget = false;
		image.type = Image.Type.Simple;
		image.preserveAspect = true;
	}

	private static void ApplyLabelDefaults(TextMeshProUGUI text)
	{
		EnsureLabelFont(text);
		text.raycastTarget = false;
		text.enableAutoSizing = true;
		text.fontSizeMin = 8f;
		text.fontSizeMax = 16f;
		text.fontSize = 12f;
		text.margin = Vector4.zero;
	}

	private static void EnsureLabelFont(TextMeshProUGUI text)
	{
		if (text.font == null)
		{
			text.font = TMP_Settings.defaultFontAsset;
		}
	}

#if UNITY_EDITOR
	protected override void OnEditorRefresh()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
		RefreshBoundState();
	}
#endif
}
