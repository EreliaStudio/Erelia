using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[ExecuteAlways]
public sealed class CreatureCardView : MonoBehaviour
{
	private static Sprite defaultSprite;

	private const string StaminaLabelFormat = "{1} sec";
	private static readonly Vector2 PortraitWidth = new Vector2(72f, 0f);
	private static readonly Vector2 FramePadding = new Vector2(2f, 2f);

	[SerializeField] private bool showStaminaBar = true;
	[SerializeField] private string defaultName = "Empty Slot";
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
#if UNITY_EDITOR
	private bool editorRefreshQueued;
#endif

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

		GameObject childObject = CreateChild("Background", transform);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		Stretch(rect);
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

		GameObject childObject = CreateChild("Frame", transform);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		Stretch(rect);
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

		GameObject childObject = CreateChild("PortraitBackground", frameImage.transform);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		ApplyPortraitBackgroundLayout(rect);
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

		GameObject childObject = CreateChild("Portrait", portraitBackgroundImage.transform);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.offsetMin = new Vector2(6f, 6f);
		rect.offsetMax = new Vector2(-6f, -6f);
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

		GameObject childObject = CreateChild("NameLabel", frameImage.transform);
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

		GameObject childObject = CreateChild("StaminaBar", frameImage.transform);
		ProgressBarView view = childObject.AddComponent<ProgressBarView>();
		view.RefreshNow();
		return view;
	}

	private void ApplySerializedState()
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = backgroundColor;
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
		}

		if (staminaBar != null)
		{
			staminaBar.gameObject.SetActive(showStaminaBar);
			staminaBar.SetLabelFormat(StaminaLabelFormat);

			if (showStaminaBar)
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

		if (!showStaminaBar)
		{
			staminaBar.SetValues(0f, 0f);
			return;
		}

		if (turnBar == null)
		{
			staminaBar.SetValues(0f, 0f);
			return;
		}

		float max = turnBar.Max;
		float remainingTime = Mathf.Max(0f, max - turnBar.Current);
		staminaBar.SetValues(remainingTime, max);
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

		Transform rootChild = transform.Find(childName);
		return rootChild;
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

		try
		{
			CreatureForm form = unit.SourceUnit.GetForm();
			if (form != null && !string.IsNullOrWhiteSpace(form.DisplayName))
			{
				return form.DisplayName;
			}
		}
		catch
		{
		}

		return unit.SourceUnit.Species != null ? unit.SourceUnit.Species.name : null;
	}

	private static Sprite GetAvatar(BattleUnit unit)
	{
		if (unit?.SourceUnit == null)
		{
			return null;
		}

		try
		{
			CreatureForm form = unit.SourceUnit.GetForm();
			return form?.Avatar;
		}
		catch
		{
			return null;
		}
	}

	private static void ApplyPortraitBackgroundLayout(RectTransform rect)
	{
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(0f, 1f);
		rect.pivot = new Vector2(0f, 0.5f);
		rect.anchoredPosition = Vector2.zero;
		rect.sizeDelta = PortraitWidth;
	}

	private void ApplyNameLabelLayout(RectTransform rect)
	{
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = new Vector2(82f, showStaminaBar ? 32f : 8f);
		rect.offsetMax = new Vector2(-8f, -8f);
	}

	private static void ApplyStaminaBarLayout(RectTransform rect)
	{
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 0f);
		rect.pivot = new Vector2(0.5f, 0f);
		rect.offsetMin = new Vector2(82f, 8f);
		rect.offsetMax = new Vector2(-8f, 28f);
	}

	private static GameObject CreateChild(string childName, Transform parent)
	{
		GameObject child = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer));
		child.transform.SetParent(parent, false);
		return child;
	}

	private static void Stretch(RectTransform rectTransform)
	{
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
	}

	private static void ApplyImageDefaults(Image image)
	{
		image.raycastTarget = false;
		image.type = Image.Type.Simple;
		image.sprite = image.sprite != null ? image.sprite : GetDefaultSprite();
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

	private static Sprite GetDefaultSprite()
	{
		if (defaultSprite != null)
		{
			return defaultSprite;
		}

		defaultSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
		defaultSprite.name = "CreatureCardDefaultSprite";
		defaultSprite.hideFlags = HideFlags.HideAndDontSave;
		return defaultSprite;
	}

#if UNITY_EDITOR
	private void QueueEditorRefresh()
	{
		if (editorRefreshQueued)
		{
			return;
		}

		editorRefreshQueued = true;
		EditorApplication.delayCall += ApplyEditorRefresh;
	}

	private void ApplyEditorRefresh()
	{
		editorRefreshQueued = false;

		if (this == null)
		{
			return;
		}

		EnsureHierarchy(true);
		ApplySerializedState();
		UnsubscribeFromTurnBar();
		SubscribeToTurnBar();
		RefreshBoundState();
	}
#endif
}
