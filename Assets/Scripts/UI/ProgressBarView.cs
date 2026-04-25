using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
public sealed class ProgressBarView : ExecuteAlwaysView
{
	[SerializeField] private string labelFormat = "Ratio {0:0.##} / Value {1:0.##} / MaxValue {2:0.##}";
	[SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);
	[SerializeField] private Color fillColor = new Color(0.85f, 0.85f, 0.85f, 1f);
	[SerializeField] private Vector2 padding = new Vector2(2f, 2f);
	[SerializeField, Min(0f)] private float maxValue = 1f;
	[SerializeField, Min(0f)] private float currentValue = 1f;

	[SerializeField] private Image backgroundImage;
	[SerializeField] private RectTransform fillArea;
	[SerializeField] private Image fillImage;
	[SerializeField] private TextMeshProUGUI label;

	private bool isRefreshing;

	public float CurrentValue => currentValue;
	public float MaxValue => maxValue;
	public float Ratio => ComputeRatio();

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
		ApplyFill();
	}

	public void SetMaxValue(float value)
	{
		maxValue = Mathf.Max(0f, value);
		currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
		ApplySerializedState();
	}

	public void SetCurrentValue(float value)
	{
		currentValue = Mathf.Clamp(value, 0f, Mathf.Max(0f, maxValue));
		ApplySerializedState();
	}

	public void SetValues(float value, float maximum)
	{
		maxValue = Mathf.Max(0f, maximum);
		currentValue = Mathf.Clamp(value, 0f, maxValue);
		ApplySerializedState();
	}

	public void SetLabelFormat(string format)
	{
		labelFormat = string.IsNullOrWhiteSpace(format) ? "{0:0.##}" : format;
		ApplyLabel();
	}

	public void SetColors(Color background, Color fill)
	{
		backgroundColor = background;
		fillColor = fill;
		ApplyColors();
	}

	public void SetPadding(Vector2 value)
	{
		padding = new Vector2(Mathf.Max(0f, value.x), Mathf.Max(0f, value.y));
		ApplyPadding();
		ApplyFill();
	}

	[ContextMenu("Refresh")]
	public void RefreshNow()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void EnsureHierarchy(bool allowCreate)
	{
		if (isRefreshing)
		{
			return;
		}

		backgroundImage = ResolveBackgroundImage(allowCreate);
		fillArea = ResolveFillArea(allowCreate);
		fillImage = ResolveFillImage(allowCreate);
		label = ResolveLabel(allowCreate);
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
			ApplyFillableImageDefaults(existing);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("Background", transform);
		UiViewUtility.Stretch(childObject.GetComponent<RectTransform>());
		Image image = childObject.AddComponent<Image>();
		ApplyFillableImageDefaults(image);
		return image;
	}

	private RectTransform ResolveFillArea(bool allowCreate)
	{
		if (fillArea != null)
		{
			return fillArea;
		}

		Transform child = transform.Find("FillArea");
		if (child != null && child is RectTransform existing)
		{
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("FillArea", transform);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		UiViewUtility.Stretch(rect);
		return rect;
	}

	private Image ResolveFillImage(bool allowCreate)
	{
		if (fillImage != null)
		{
			return fillImage;
		}

		if (fillArea != null)
		{
			Transform child = fillArea.Find("Fill");
			if (child != null && child.TryGetComponent(out Image existing))
			{
				ApplyFillableImageDefaults(existing);
				return existing;
			}
		}

		if (!allowCreate || fillArea == null)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("Fill", fillArea);
		RectTransform rect = childObject.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0f, 0f);
		rect.anchorMax = new Vector2(1f, 1f);
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
		rect.pivot = new Vector2(0f, 0.5f);
		Image image = childObject.AddComponent<Image>();
		ApplyFillableImageDefaults(image);
		return image;
	}

	private TextMeshProUGUI ResolveLabel(bool allowCreate)
	{
		if (label != null)
		{
			return label;
		}

		Transform child = transform.Find("Label");
		if (child != null && child.TryGetComponent(out TextMeshProUGUI existing))
		{
			EnsureLabelFont(existing);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject childObject = UiViewUtility.CreateChild("Label", transform);
		UiViewUtility.Stretch(childObject.GetComponent<RectTransform>());
		TextMeshProUGUI text = childObject.AddComponent<TextMeshProUGUI>();
		ApplyLabelDefaults(text);
		return text;
	}

	private void ApplySerializedState()
	{
		if (isRefreshing)
		{
			return;
		}

		isRefreshing = true;
		try
		{
			padding.x = Mathf.Max(0f, padding.x);
			padding.y = Mathf.Max(0f, padding.y);
			maxValue = Mathf.Max(0f, maxValue);
			currentValue = Mathf.Clamp(currentValue, 0f, maxValue);

			ApplyColors();
			ApplyPadding();
			ApplyFill();
			ApplyLabel();
		}
		finally
		{
			isRefreshing = false;
		}
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

	private void ApplyPadding()
	{
		if (fillArea == null)
		{
			return;
		}

		fillArea.offsetMin = padding;
		fillArea.offsetMax = -padding;
	}

	private void ApplyFill()
	{
		if (fillImage == null)
		{
			return;
		}

		float ratio = ComputeRatio();
		RectTransform fillRect = fillImage.rectTransform;
		fillRect.anchorMin = new Vector2(0f, 0f);
		fillRect.anchorMax = new Vector2(ratio, 1f);
		fillRect.offsetMin = Vector2.zero;
		fillRect.offsetMax = Vector2.zero;
		fillRect.pivot = new Vector2(0f, 0.5f);
		fillImage.enabled = ratio > 0f;
	}

	private void ApplyLabel()
	{
		if (label == null)
		{
			return;
		}

		label.text = BuildLabel();
	}

	private float ComputeRatio()
	{
		if (maxValue <= 0f)
		{
			return 0f;
		}

		return Mathf.Clamp01(currentValue / maxValue);
	}

	private string BuildLabel()
	{
		string format = string.IsNullOrWhiteSpace(labelFormat) ? "{0:0.##}" : labelFormat;

		try
		{
			return string.Format(CultureInfo.InvariantCulture, format, ComputeRatio(), currentValue, maxValue, maxValue - currentValue);
		}
		catch (System.FormatException)
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				"Ratio {0:0.##} / Value {1:0.##} / MaxValue {2:0.##} / Remaining {3:0.##}",
				ComputeRatio(), currentValue, maxValue, maxValue - currentValue);
		}
	}

	private static void ApplyFillableImageDefaults(Image image)
	{
		image.raycastTarget = false;
		image.type = Image.Type.Simple;
		image.sprite = image.sprite != null ? image.sprite : UiViewUtility.GetDefaultSprite();
	}

	private static void ApplyLabelDefaults(TextMeshProUGUI text)
	{
		EnsureLabelFont(text);
		text.raycastTarget = false;
		text.alignment = TextAlignmentOptions.Center;
		text.enableAutoSizing = true;
		text.fontSizeMin = 8f;
		text.fontSizeMax = 24f;
		text.fontSize = 14f;
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
	}
#endif
}
