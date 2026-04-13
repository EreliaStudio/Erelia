using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class ProgressBarElementUI : MonoBehaviour
{
	private static Sprite defaultSprite;

	[SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.4f);
	[SerializeField] private Color fillColor = Color.white;
	[SerializeField] private Vector2 fillPadding = new Vector2(2f, 2f);
	[SerializeField] private Image backgroundImage;
	[SerializeField] private Image fillImage;
	[SerializeField] private TMP_Text labelText;

	private float progress01 = 1f;
	private string label = string.Empty;
	private bool isApplyingProgress;

	private void Reset()
	{
		RebuildVisualHierarchy();
	}

	private void Awake()
	{
		ResolveReferences();
		ApplyStyle();
		ApplyState();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode == false)
		{
			ResolveReferences();
			ApplyStyle();
			ApplyPreviewState();
		}
	}
#endif

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
		ResolveReferences();
		label = p_value ?? string.Empty;
		labelText.text = label;
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
#if UNITY_EDITOR
		backgroundImage ??= GetComponent<Image>() ?? gameObject.AddComponent<Image>();
		fillImage ??= ResolveOrCreateFillImage();
		labelText ??= ResolveOrCreateLabelText();
#endif
		ApplyStyle();
		ApplyPreviewState();
	}

	private void ApplyPreviewState()
	{
		progress01 = 0.75f;
		label = "PreviewState";
		ApplyState();
	}

	private void ApplyState()
	{
		ApplyColors();
		ApplyProgress();
		labelText.text = label;
	}

	private void ApplyStyle()
	{
		ResolveReferences();
		ApplyBackgroundStyle(backgroundImage);
		ApplyFillStyle(fillImage);
		ApplyLabelStyle(labelText);
	}

	private void ApplyColors()
	{
		ResolveReferences();
		backgroundImage.color = backgroundColor;
		fillImage.color = fillColor;
	}

	private void ApplyProgress()
	{
		ResolveReferences();

		if (isApplyingProgress)
		{
			return;
		}

		RectTransform backgroundRectTransform = backgroundImage.rectTransform;
		RectTransform fillRectTransform = fillImage.rectTransform;
		float availableWidth = Mathf.Max(0f, backgroundRectTransform.rect.width - (fillPadding.x * 2f));

		isApplyingProgress = true;

		try
		{
			fillRectTransform.anchorMin = new Vector2(0f, 0f);
			fillRectTransform.anchorMax = new Vector2(0f, 1f);
			fillRectTransform.pivot = new Vector2(0f, 0.5f);
			fillRectTransform.anchoredPosition = new Vector2(fillPadding.x, 0f);
			fillRectTransform.sizeDelta = new Vector2(availableWidth * progress01, -(fillPadding.y * 2f));
		}
		finally
		{
			isApplyingProgress = false;
		}
	}

	private void ResolveReferences()
	{
		backgroundImage ??= GetComponent<Image>();
		labelText ??= GetComponentInChildren<TMP_Text>(true);

		if (fillImage != null)
		{
			return;
		}

		for (int index = 0; index < transform.childCount; index++)
		{
			Image image = transform.GetChild(index).GetComponent<Image>();
			if (image != null)
			{
				fillImage = image;
				return;
			}
		}
	}

#if UNITY_EDITOR
	private Image ResolveOrCreateFillImage()
	{
		Transform fillTransform = transform.Find("Fill");
		if (fillTransform == null)
		{
			GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			fillObject.layer = gameObject.layer;
			fillObject.transform.SetParent(transform, false);
			fillTransform = fillObject.transform;
		}

		return fillTransform.GetComponent<Image>();
	}

	private TMP_Text ResolveOrCreateLabelText()
	{
		Transform labelTransform = transform.Find("Label");
		if (labelTransform == null)
		{
			GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			labelObject.layer = gameObject.layer;
			labelObject.transform.SetParent(transform, false);
			labelObject.transform.SetAsLastSibling();

			RectTransform labelRectTransform = (RectTransform) labelObject.transform;
			labelRectTransform.anchorMin = Vector2.zero;
			labelRectTransform.anchorMax = Vector2.one;
			labelRectTransform.offsetMin = Vector2.zero;
			labelRectTransform.offsetMax = Vector2.zero;

			labelTransform = labelObject.transform;
		}

		return labelTransform.GetComponent<TMP_Text>();
	}
#endif

	private static void ApplyBackgroundStyle(Image p_image)
	{
		p_image.sprite = ResolveDefaultSprite();
		p_image.type = Image.Type.Simple;
		p_image.raycastTarget = false;
	}

	private static void ApplyFillStyle(Image p_image)
	{
		p_image.sprite = ResolveDefaultSprite();
		p_image.type = Image.Type.Simple;
		p_image.raycastTarget = false;
	}

	private static void ApplyLabelStyle(TMP_Text p_text)
	{
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
		if (defaultSprite == null)
		{
			Texture2D texture = Texture2D.whiteTexture;
			defaultSprite = Sprite.Create(
				texture,
				new Rect(0f, 0f, texture.width, texture.height),
				new Vector2(0.5f, 0.5f),
				100f);
			defaultSprite.name = "ProgressBarElementUIDefaultSprite";
			defaultSprite.hideFlags = HideFlags.HideAndDontSave;
		}

		return defaultSprite;
	}
}
