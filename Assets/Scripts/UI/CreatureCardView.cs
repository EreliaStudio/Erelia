using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
public sealed class CreatureCardView : ExecuteAlwaysView, IPointerClickHandler
{
	private enum PortraitSide
	{
		Left,
		Right
	}

	private const string DefaultStaminaLabelFormat = "{1:0.##} sec";
	private static readonly Vector2 FramePadding = new Vector2(2f, 2f);
	private const float ContentPadding = 8f;
	private const float StaminaBarHeight = 20f;

	[SerializeField] private PortraitSide portraitSide = PortraitSide.Left;
	[SerializeField] private bool showStaminaBar = true;
	[SerializeField] private string defaultName = "Empty Slot";
	[SerializeField] private string staminaLabelFormat = DefaultStaminaLabelFormat;
	[SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);
	[SerializeField] private Color frameColor = new Color(1f, 1f, 1f, 0.08f);
	[SerializeField] private Color portraitBackgroundColor = new Color(0f, 0f, 0f, 0.25f);
	[SerializeField] private Color labelColor = Color.white;
	[SerializeField] private Sprite defaultPortrait;

	[SerializeField] private Image backgroundImage;
	[SerializeField] private Image frameImage;
	[SerializeField] private Image portraitBackgroundImage;
	[SerializeField] private Image portraitImage;
	[SerializeField] private TextMeshProUGUI nameLabel;
	[SerializeField] private ProgressBarView staminaBar;

	private BattleUnit boundUnit;
	private ObservableFloatResource subscribedTurnBar;
	private Color? backgroundColorOverride;

	public event Action<BattleUnit> LeftClicked;
	public event Action<BattleUnit> RightClicked;

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
		SubscribeToTurnBar();
	}

	private void OnDisable()
	{
		UnsubscribeFromTurnBar();
	}

	private void OnDestroy()
	{
		UnsubscribeFromTurnBar();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
		EnsureHierarchy(true);
		ApplySerializedState();
#endif
	}

	private void OnRectTransformDimensionsChange()
	{
		ApplySerializedState();
	}

	public void Bind(BattleUnit unit)
	{
		if (ReferenceEquals(boundUnit, unit))
		{
			RefreshBoundState();
			return;
		}

		UnsubscribeFromTurnBar();
		boundUnit = unit;
		SubscribeToTurnBar();
		RefreshBoundState();
	}

	public void SetFormat(string format)
	{
		staminaLabelFormat = string.IsNullOrWhiteSpace(format) ? DefaultStaminaLabelFormat : format;

		if (staminaBar != null)
		{
			staminaBar.SetLabelFormat(staminaLabelFormat);
		}
	}

	public void SetBackgroundColor(Color color)
	{
		backgroundColorOverride = color;
		if (backgroundImage != null)
		{
			backgroundImage.color = color;
		}
	}

	public void ClearBackgroundColorOverride()
	{
		backgroundColorOverride = null;
		if (backgroundImage != null)
		{
			backgroundImage.color = backgroundColor;
		}
	}

	public void ClearClickListeners()
	{
		LeftClicked = null;
		RightClicked = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData == null)
		{
			return;
		}

		switch (eventData.button)
		{
			case PointerEventData.InputButton.Left:
				LeftClicked?.Invoke(boundUnit);
				break;
			case PointerEventData.InputButton.Right:
				RightClicked?.Invoke(boundUnit);
				break;
		}
	}

	private void EnsureHierarchy(bool allowCreate)
	{
		backgroundImage = ResolveBackgroundImage(allowCreate);
		frameImage = ResolveFrameImage(allowCreate);
		portraitBackgroundImage = ResolvePortraitBackgroundImage(allowCreate);
		portraitImage = ResolvePortraitImage(allowCreate);
		nameLabel = ResolveNameLabel(allowCreate);
		staminaBar = ResolveStaminaBar(allowCreate);
	}

	private Image ResolveBackgroundImage(bool allowCreate)
	{
		if (backgroundImage != null)
		{
			return backgroundImage;
		}

		Transform child = transform.Find("Background");
		if (child != null && child.TryGetComponent(out Image existing))
		{
			ApplyImageDefaults(existing);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("Background", transform);
		UiViewUtility.Stretch(childObject.GetComponent<RectTransform>());
		Image image = childObject.AddComponent<Image>();
		ApplyImageDefaults(image);
		return image;
	}

	private Image ResolveFrameImage(bool allowCreate)
	{
		if (frameImage != null)
		{
			return frameImage;
		}

		Transform child = transform.Find("Frame");
		if (child != null && child.TryGetComponent(out Image existing))
		{
			ApplyImageDefaults(existing);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("Frame", transform);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		UiViewUtility.Stretch(rect);
		rect.offsetMin = FramePadding;
		rect.offsetMax = -FramePadding;
		Image image = childObject.AddComponent<Image>();
		ApplyImageDefaults(image);
		return image;
	}

	private Image ResolvePortraitBackgroundImage(bool allowCreate)
	{
		if (portraitBackgroundImage != null)
		{
			return portraitBackgroundImage;
		}

		Transform child = FindContentTransform("PortraitBackground");
		if (child != null && child.TryGetComponent(out Image existing))
		{
			ApplyImageDefaults(existing);
			MoveToFrame(child);
			return existing;
		}

		if (!allowCreate || frameImage == null)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("PortraitBackground", frameImage.transform);
		ApplyPortraitBackgroundLayout(childObject.GetComponent<RectTransform>());
		Image image = childObject.AddComponent<Image>();
		ApplyImageDefaults(image);
		return image;
	}

	private Image ResolvePortraitImage(bool allowCreate)
	{
		if (portraitImage != null)
		{
			return portraitImage;
		}

		Transform parent = portraitBackgroundImage != null ? portraitBackgroundImage.transform : FindContentTransform("PortraitBackground");
		if (parent != null)
		{
			Transform child = parent.Find("Portrait");
			if (child != null && child.TryGetComponent(out Image existing))
			{
				ApplyPortraitDefaults(existing);
				return existing;
			}
		}

		if (!allowCreate || portraitBackgroundImage == null)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("Portrait", portraitBackgroundImage.transform);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
		rect.pivot = new Vector2(0.5f, 0.5f);
		Image image = childObject.AddComponent<Image>();
		ApplyPortraitDefaults(image);
		return image;
	}

	private TextMeshProUGUI ResolveNameLabel(bool allowCreate)
	{
		if (nameLabel != null)
		{
			return nameLabel;
		}

		Transform child = FindContentTransform("NameLabel");
		if (child != null && child.TryGetComponent(out TextMeshProUGUI existing))
		{
			ApplyLabelDefaults(existing);
			MoveToFrame(child);
			return existing;
		}

		if (!allowCreate || frameImage == null)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("NameLabel", frameImage.transform);
		TextMeshProUGUI text = childObject.AddComponent<TextMeshProUGUI>();
		ApplyLabelDefaults(text);
		return text;
	}

	private ProgressBarView ResolveStaminaBar(bool allowCreate)
	{
		if (staminaBar != null)
		{
			return staminaBar;
		}

		Transform child = FindContentTransform("StaminaBar");
		if (child != null && child.TryGetComponent(out ProgressBarView existing))
		{
			existing.RefreshNow();
			MoveToFrame(child);
			return existing;
		}

		if (!allowCreate || frameImage == null)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("StaminaBar", frameImage.transform);
		ProgressBarView view = childObject.AddComponent<ProgressBarView>();
		view.RefreshNow();
		return view;
	}

	private void ApplySerializedState()
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = backgroundColorOverride ?? backgroundColor;
		}

		if (frameImage != null)
		{
			frameImage.color = frameColor;
			frameImage.rectTransform.offsetMin = FramePadding;
			frameImage.rectTransform.offsetMax = -FramePadding;
		}

		if (portraitBackgroundImage != null)
		{
			portraitBackgroundImage.color = portraitBackgroundColor;
			ApplyPortraitBackgroundLayout(portraitBackgroundImage.rectTransform);
		}

		if (nameLabel != null)
		{
			nameLabel.color = labelColor;
			ApplyNameLabelLayout(nameLabel.rectTransform);
			ApplyNameLabelAlignment(nameLabel);
		}

		if (staminaBar != null)
		{
			bool shouldShowStaminaBar = ShouldShowStaminaBar();
			staminaBar.gameObject.SetActive(shouldShowStaminaBar);
			staminaBar.SetLabelFormat(staminaLabelFormat);

			if (shouldShowStaminaBar)
			{
				ApplyStaminaBarLayout(staminaBar.GetComponent<RectTransform>());
			}
		}
	}

	private void SubscribeToTurnBar()
	{
		if (!isActiveAndEnabled || !showStaminaBar)
		{
			return;
		}

		ObservableFloatResource turnBar = boundUnit?.BattleAttributes?.TurnBar;
		if (turnBar == null || ReferenceEquals(subscribedTurnBar, turnBar))
		{
			return;
		}

		turnBar.Changed += HandleTurnBarChanged;
		subscribedTurnBar = turnBar;
	}

	private void UnsubscribeFromTurnBar()
	{
		if (subscribedTurnBar == null)
		{
			return;
		}

		subscribedTurnBar.Changed -= HandleTurnBarChanged;
		subscribedTurnBar = null;
	}

	private void HandleTurnBarChanged(ObservableFloatResource turnBar)
	{
		RefreshStaminaBar(turnBar);
	}

	private void RefreshBoundState()
	{
		EnsureHierarchy(true);

		if (portraitImage != null)
		{
			portraitImage.sprite = GetAvatar(boundUnit) ?? defaultPortrait;
			portraitImage.enabled = portraitImage.sprite != null;
		}

		if (nameLabel != null)
		{
			nameLabel.text = GetDisplayName(boundUnit) ?? defaultName;
		}

		RefreshStaminaBar(boundUnit?.BattleAttributes?.TurnBar);
	}

	private void RefreshStaminaBar(ObservableFloatResource turnBar)
	{
		if (staminaBar == null)
		{
			return;
		}

		bool shouldShowBar = ShouldShowStaminaBar();
		staminaBar.gameObject.SetActive(shouldShowBar);

		if (!shouldShowBar || turnBar == null)
		{
			staminaBar.SetValues(0f, 0f);
			return;
		}

		staminaBar.SetValues(Mathf.Max(0f, turnBar.Current), turnBar.Max);
	}

	private bool ShouldShowStaminaBar()
	{
		return showStaminaBar && boundUnit != null;
	}

	private Transform FindContentTransform(string childName)
	{
		if (frameImage != null)
		{
			Transform frameChild = frameImage.transform.Find(childName);
			if (frameChild != null)
			{
				return frameChild;
			}
		}

		return transform.Find(childName);
	}

	private void MoveToFrame(Transform child)
	{
		if (child == null || frameImage == null || child.parent == frameImage.transform)
		{
			return;
		}

		child.SetParent(frameImage.transform, false);
	}

	private static string GetDisplayName(BattleUnit unit)
	{
		if (unit?.SourceUnit == null)
		{
			return null;
		}

		if (unit.SourceUnit.TryGetForm(out CreatureForm form) && !string.IsNullOrWhiteSpace(form.DisplayName))
		{
			return form.DisplayName;
		}

		return unit.SourceUnit.Species != null ? unit.SourceUnit.Species.name : null;
	}

	private static Sprite GetAvatar(BattleUnit unit)
	{
		if (unit?.SourceUnit == null)
		{
			return null;
		}

		return unit.SourceUnit.TryGetForm(out CreatureForm form) ? form.Avatar : null;
	}

	private void ApplyPortraitBackgroundLayout(RectTransform rect)
	{
		float portraitSize = GetPortraitSize();

		if (portraitSide == PortraitSide.Left)
		{
			rect.anchorMin = new Vector2(0f, 0.5f);
			rect.anchorMax = new Vector2(0f, 0.5f);
			rect.pivot = new Vector2(0f, 0.5f);
			rect.anchoredPosition = new Vector2(ContentPadding, 0f);
		}
		else
		{
			rect.anchorMin = new Vector2(1f, 0.5f);
			rect.anchorMax = new Vector2(1f, 0.5f);
			rect.pivot = new Vector2(1f, 0.5f);
			rect.anchoredPosition = new Vector2(-ContentPadding, 0f);
		}

		rect.sizeDelta = new Vector2(portraitSize, portraitSize);
	}

	private void ApplyNameLabelLayout(RectTransform rect)
	{
		float portraitSize = GetPortraitSize();
		float portraitSpan = portraitSize + (ContentPadding * 2f);
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.pivot = new Vector2(0.5f, 0.5f);

		float bottomInset = ShouldShowStaminaBar() ? ContentPadding + StaminaBarHeight + 4f : ContentPadding;
		if (portraitSide == PortraitSide.Left)
		{
			rect.offsetMin = new Vector2(portraitSpan, bottomInset);
			rect.offsetMax = new Vector2(-ContentPadding, -ContentPadding);
		}
		else
		{
			rect.offsetMin = new Vector2(ContentPadding, bottomInset);
			rect.offsetMax = new Vector2(-portraitSpan, -ContentPadding);
		}
	}

	private void ApplyNameLabelAlignment(TextMeshProUGUI text)
	{
		text.alignment = portraitSide == PortraitSide.Left
			? TextAlignmentOptions.MidlineLeft
			: TextAlignmentOptions.MidlineRight;
	}

	private void ApplyStaminaBarLayout(RectTransform rect)
	{
		float portraitSize = GetPortraitSize();
		float portraitSpan = portraitSize + (ContentPadding * 2f);
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 0f);
		rect.pivot = new Vector2(0.5f, 0f);

		if (portraitSide == PortraitSide.Left)
		{
			rect.offsetMin = new Vector2(portraitSpan, ContentPadding);
			rect.offsetMax = new Vector2(-ContentPadding, ContentPadding + StaminaBarHeight);
		}
		else
		{
			rect.offsetMin = new Vector2(ContentPadding, ContentPadding);
			rect.offsetMax = new Vector2(-portraitSpan, ContentPadding + StaminaBarHeight);
		}
	}

	private float GetPortraitSize()
	{
		if (!TryGetComponent(out RectTransform rectTransform))
		{
			return 0f;
		}

		return Mathf.Max(0f, rectTransform.rect.height - (ContentPadding * 2f));
	}

	private static void ApplyImageDefaults(Image image)
	{
		image.raycastTarget = true;
		image.type = Image.Type.Simple;
		image.sprite = image.sprite != null ? image.sprite : UiViewUtility.GetDefaultSprite();
	}

	private static void ApplyPortraitDefaults(Image image)
	{
		ApplyImageDefaults(image);
		image.preserveAspect = true;
	}

	private static void ApplyLabelDefaults(TextMeshProUGUI text)
	{
		text.raycastTarget = false;
		text.alignment = TextAlignmentOptions.MidlineLeft;
		text.enableAutoSizing = true;
		text.fontSizeMin = 10f;
		text.fontSizeMax = 24f;
		text.fontSize = 16f;
		text.margin = Vector4.zero;

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
		UnsubscribeFromTurnBar();
		SubscribeToTurnBar();
		RefreshBoundState();
	}
#endif
}
