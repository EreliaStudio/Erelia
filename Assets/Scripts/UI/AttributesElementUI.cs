using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AttributesElementUI : MonoBehaviour
{
	[SerializeField] private ProgressBarElementUI healthBarElementUI;
	[SerializeField] private ProgressBarElementUI actionPointsBarElementUI;
	[SerializeField] private ProgressBarElementUI movementPointsBarElementUI;
	[SerializeField] private Color progressBarBackgroundColor = new Color(0f, 0f, 0f, 0.22f);
	[SerializeField] private Color healthBarFillColor = new Color(0.83f, 0.25f, 0.25f, 1f);
	[SerializeField] private Color actionPointsBarFillColor = new Color(0.89f, 0.67f, 0.2f, 1f);
	[SerializeField] private Color movementPointsBarFillColor = new Color(0.27f, 0.62f, 0.89f, 1f);
	[SerializeField] private Image attackIconImage;
	[SerializeField] private TMP_Text attackValueLabel;
	[SerializeField] private Image armorIconImage;
	[SerializeField] private TMP_Text armorValueLabel;
	[SerializeField] private Image magicIconImage;
	[SerializeField] private TMP_Text magicValueLabel;
	[SerializeField] private Image resistanceIconImage;
	[SerializeField] private TMP_Text resistanceValueLabel;
	[SerializeField] private Sprite attackIconSprite;
	[SerializeField] private Sprite armorIconSprite;
	[SerializeField] private Sprite magicIconSprite;
	[SerializeField] private Sprite resistanceIconSprite;

	private BattleUnit linkedBattleUnit;

	private void Awake()
	{
		ApplyVisualConfiguration();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		ApplyVisualConfiguration();
	}
#endif

	public void Bind(BattleUnit p_battleUnit)
	{
		linkedBattleUnit = p_battleUnit;
		ApplyVisualConfiguration();
		Refresh();
	}

	public void Clear()
	{
		linkedBattleUnit = null;
		Refresh();
	}

	public void Refresh()
	{
		if (linkedBattleUnit == null)
		{
			ClearBar(healthBarElementUI);
			ClearBar(actionPointsBarElementUI);
			ClearBar(movementPointsBarElementUI);
			ClearStatValue(attackValueLabel);
			ClearStatValue(armorValueLabel);
			ClearStatValue(magicValueLabel);
			ClearStatValue(resistanceValueLabel);
			return;
		}

		Attributes sourceAttributes = linkedBattleUnit.SourceUnit != null
			? linkedBattleUnit.SourceUnit.Attributes
			: null;

		ApplyBar(
			healthBarElementUI,
			"HP",
			linkedBattleUnit.CurrentHealth,
			linkedBattleUnit.MaxHealth);
		ApplyBar(
			actionPointsBarElementUI,
			"AP",
			linkedBattleUnit.CurrentActionPoints,
			linkedBattleUnit.MaxActionPoints);
		ApplyBar(
			movementPointsBarElementUI,
			"MP",
			linkedBattleUnit.CurrentMovementPoints,
			linkedBattleUnit.MaxMovementPoints);

		ApplyStatValue(attackIconImage, attackValueLabel, attackIconSprite, sourceAttributes != null ? sourceAttributes.Attack : 0, sourceAttributes != null);
		ApplyStatValue(armorIconImage, armorValueLabel, armorIconSprite, sourceAttributes != null ? sourceAttributes.Armor : 0, sourceAttributes != null);
		ApplyStatValue(magicIconImage, magicValueLabel, magicIconSprite, sourceAttributes != null ? sourceAttributes.Magic : 0, sourceAttributes != null);
		ApplyStatValue(resistanceIconImage, resistanceValueLabel, resistanceIconSprite, sourceAttributes != null ? sourceAttributes.Resistance : 0, sourceAttributes != null);
	}

	private void ApplyVisualConfiguration()
	{
		ApplyBarColors(healthBarElementUI, healthBarFillColor);
		ApplyBarColors(actionPointsBarElementUI, actionPointsBarFillColor);
		ApplyBarColors(movementPointsBarElementUI, movementPointsBarFillColor);
		ApplyStatIcon(attackIconImage, attackIconSprite);
		ApplyStatIcon(armorIconImage, armorIconSprite);
		ApplyStatIcon(magicIconImage, magicIconSprite);
		ApplyStatIcon(resistanceIconImage, resistanceIconSprite);
	}

	private static void ClearBar(ProgressBarElementUI p_progressBarElementUI)
	{
		if (p_progressBarElementUI == null)
		{
			return;
		}

		p_progressBarElementUI.Clear();
	}

	private static void ApplyBar(
		ProgressBarElementUI p_progressBarElementUI,
		string p_labelPrefix,
		int p_currentValue,
		int p_maxValue)
	{
		if (p_progressBarElementUI == null)
		{
			return;
		}

		int safeCurrentValue = Mathf.Clamp(p_currentValue, 0, Mathf.Max(0, p_maxValue));
		float ratio = p_maxValue > 0
			? (float)safeCurrentValue / p_maxValue
			: 0f;

		p_progressBarElementUI.SetProgress(ratio);
		p_progressBarElementUI.SetLabel($"{p_labelPrefix} {safeCurrentValue} / {Mathf.Max(0, p_maxValue)}");
	}

	private void ApplyBarColors(ProgressBarElementUI p_progressBarElementUI, Color p_fillColor)
	{
		if (p_progressBarElementUI == null)
		{
			return;
		}

		p_progressBarElementUI.SetColors(progressBarBackgroundColor, p_fillColor);
	}

	private static void ApplyStatValue(Image p_iconImage, TMP_Text p_text, Sprite p_iconSprite, int p_value, bool p_hasValue)
	{
		ApplyStatIcon(p_iconImage, p_iconSprite);
		SetText(p_text, p_hasValue ? p_value.ToString() : string.Empty);
	}

	private static void ClearStatValue(TMP_Text p_text)
	{
		SetText(p_text, string.Empty);
	}

	private static void ApplyStatIcon(Image p_iconImage, Sprite p_sprite)
	{
		if (p_iconImage == null)
		{
			return;
		}

		p_iconImage.sprite = p_sprite;
		p_iconImage.enabled = p_sprite != null;
	}

	private static void SetText(TMP_Text p_text, string p_value)
	{
		if (p_text == null)
		{
			return;
		}

		p_text.text = p_value ?? string.Empty;
	}
}

public sealed partial class ProgressBarElementUI : MonoBehaviour
{
	private static Sprite defaultSprite;

	[SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.4f);
	[SerializeField] private Color fillColor = Color.white;
	[SerializeField] private Vector2 fillPadding = new Vector2(2f, 2f);
	[SerializeField] private Image backgroundImage;
	[SerializeField] private Image fillImage;
	[SerializeField] private TMP_Text labelText;
	[SerializeField] private string previewLabel = "Value";
	[SerializeField, Range(0f, 1f)] private float previewProgress01 = 1f;

	private float progress01 = 1f;
	private string label = string.Empty;
	private bool isApplyingProgress;
#if UNITY_EDITOR
	private bool isEditorRefreshQueued;
#endif

	private void Reset()
	{
		EnsureVisualHierarchy(true);
		ApplyPreviewState();
	}

	private void Awake()
	{
		EnsureVisualHierarchy(true);
		ApplyState();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		QueueEditorRefresh();
#endif
	}

	private void OnRectTransformDimensionsChange()
	{
		ApplyProgress();
	}

	public void SetProgress(float p_ratio01)
	{
		progress01 = Mathf.Clamp01(p_ratio01);
		ApplyProgress();
	}

	public void SetLabel(string p_value)
	{
		label = p_value ?? string.Empty;
		ApplyLabel();
	}

	public void SetColors(Color p_backgroundColor, Color p_fillColor)
	{
		backgroundColor = p_backgroundColor;
		fillColor = p_fillColor;
		ApplyColors();
	}

	public void Clear()
	{
		progress01 = 0f;
		label = string.Empty;
		ApplyState();
	}

	public void RebuildVisualHierarchy()
	{
		EnsureVisualHierarchy(true);
		ApplyPreviewState();
	}

#if UNITY_EDITOR
	private void OnDisable()
	{
		if (isEditorRefreshQueued)
		{
			EditorApplication.delayCall -= ApplyDeferredEditorRefresh;
			isEditorRefreshQueued = false;
		}
	}

	private void QueueEditorRefresh()
	{
		if (isEditorRefreshQueued)
		{
			return;
		}

		isEditorRefreshQueued = true;
		EditorApplication.delayCall += ApplyDeferredEditorRefresh;
	}

	private void ApplyDeferredEditorRefresh()
	{
		EditorApplication.delayCall -= ApplyDeferredEditorRefresh;
		isEditorRefreshQueued = false;

		if (this == null || EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		EnsureVisualHierarchy(CanCreateEditorVisualHierarchy());
		ApplyPreviewState();
	}

	private bool CanCreateEditorVisualHierarchy()
	{
		return EditorUtility.IsPersistent(gameObject) == false;
	}
#endif

	private void EnsureVisualHierarchy(bool p_allowCreate)
	{
		backgroundImage = ResolveBackgroundImage(p_allowCreate);
		fillImage = ResolveFillImage(p_allowCreate);
		labelText = ResolveLabelText(p_allowCreate);
	}

	private void ApplyPreviewState()
	{
		progress01 = previewProgress01;
		label = previewLabel ?? string.Empty;
		ApplyState();
	}

	private void ApplyState()
	{
		ApplyColors();
		ApplyProgress();
		ApplyLabel();
	}

	private void ApplyColors()
	{
		if (backgroundImage != null)
		{
			backgroundImage.color = backgroundColor;
		}

		if (fillImage != null)
		{
			fillImage.color = fillColor;
		}
	}

	private void ApplyProgress()
	{
		if (fillImage == null || isApplyingProgress)
		{
			return;
		}

		RectTransform fillParent = backgroundImage != null
			? backgroundImage.rectTransform
			: transform as RectTransform;
		RectTransform fillRectTransform = fillImage.rectTransform;

		if (fillParent == null || fillRectTransform == null)
		{
			return;
		}

		isApplyingProgress = true;

		try
		{
			float availableWidth = Mathf.Max(0f, fillParent.rect.width - (fillPadding.x * 2f));
			float targetWidth = availableWidth * progress01;

			fillRectTransform.anchorMin = new Vector2(0f, 0f);
			fillRectTransform.anchorMax = new Vector2(0f, 1f);
			fillRectTransform.pivot = new Vector2(0f, 0.5f);
			fillRectTransform.anchoredPosition = new Vector2(fillPadding.x, 0f);
			fillRectTransform.sizeDelta = new Vector2(targetWidth, -(fillPadding.y * 2f));
		}
		finally
		{
			isApplyingProgress = false;
		}
	}

	private void ApplyLabel()
	{
		if (labelText == null)
		{
			return;
		}

		labelText.text = label;
	}

	private Image ResolveBackgroundImage(bool p_allowCreate)
	{
		if (backgroundImage != null)
		{
			ApplyBackgroundStyle(backgroundImage);
			return backgroundImage;
		}

		Image existingImage = GetComponent<Image>();
		if (existingImage != null)
		{
			ApplyBackgroundStyle(existingImage);
			return existingImage;
		}

		if (p_allowCreate == false)
		{
			return null;
		}

		Image createdImage = gameObject.AddComponent<Image>();
		ApplyBackgroundStyle(createdImage);
		return createdImage;
	}

	private Image ResolveFillImage(bool p_allowCreate)
	{
		if (fillImage != null)
		{
			ApplyFillStyle(fillImage);
			return fillImage;
		}

		Transform namedFillTransform = transform.Find("Fill");
		if (namedFillTransform != null && namedFillTransform.TryGetComponent(out Image namedFillImage))
		{
			ApplyFillStyle(namedFillImage);
			return namedFillImage;
		}

		if (p_allowCreate == false)
		{
			return null;
		}

		GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		fillObject.layer = gameObject.layer;
		fillObject.transform.SetParent(transform, false);

		Image createdFillImage = fillObject.GetComponent<Image>();
		ApplyFillStyle(createdFillImage);
		return createdFillImage;
	}

	private TMP_Text ResolveLabelText(bool p_allowCreate)
	{
		if (labelText != null)
		{
			ApplyLabelStyle(labelText);
			return labelText;
		}

		Transform namedLabelTransform = transform.Find("Label");
		if (namedLabelTransform != null && namedLabelTransform.TryGetComponent(out TMP_Text namedLabelText))
		{
			ApplyLabelStyle(namedLabelText);
			return namedLabelText;
		}

		if (p_allowCreate == false || TMP_Settings.defaultFontAsset == null)
		{
			return null;
		}

		GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		labelObject.layer = gameObject.layer;
		labelObject.transform.SetParent(transform, false);
		labelObject.transform.SetAsLastSibling();

		RectTransform labelRectTransform = (RectTransform) labelObject.transform;
		labelRectTransform.anchorMin = Vector2.zero;
		labelRectTransform.anchorMax = Vector2.one;
		labelRectTransform.offsetMin = Vector2.zero;
		labelRectTransform.offsetMax = Vector2.zero;

		TextMeshProUGUI createdLabelText = labelObject.GetComponent<TextMeshProUGUI>();
		ApplyLabelStyle(createdLabelText);
		return createdLabelText;
	}

	private static void ApplyBackgroundStyle(Image p_image)
	{
		if (p_image == null)
		{
			return;
		}

		p_image.sprite = ResolveDefaultSprite();
		p_image.type = Image.Type.Simple;
		p_image.raycastTarget = false;
	}

	private static void ApplyFillStyle(Image p_image)
	{
		if (p_image == null)
		{
			return;
		}

		p_image.sprite = ResolveDefaultSprite();
		p_image.type = Image.Type.Simple;
		p_image.raycastTarget = false;
		p_image.fillAmount = 1f;
	}

	private static void ApplyLabelStyle(TMP_Text p_text)
	{
		if (p_text == null)
		{
			return;
		}

		p_text.font = TMP_Settings.defaultFontAsset;
		p_text.fontSize = 7f;
		p_text.alignment = TextAlignmentOptions.Center;
		p_text.color = Color.white;
		p_text.enableAutoSizing = true;
		p_text.fontSizeMin = 5f;
		p_text.fontSizeMax = 8f;
		p_text.textWrappingMode = TextWrappingModes.NoWrap;
		p_text.raycastTarget = false;
	}

	private static Sprite ResolveDefaultSprite()
	{
		if (defaultSprite != null)
		{
			return defaultSprite;
		}

		Texture2D texture = Texture2D.whiteTexture;
		defaultSprite = Sprite.Create(
			texture,
			new Rect(0f, 0f, texture.width, texture.height),
			new Vector2(0.5f, 0.5f),
			100f);
		defaultSprite.name = "ProgressBarElementUIDefaultSprite";
		defaultSprite.hideFlags = HideFlags.HideAndDontSave;
		return defaultSprite;
	}
}
