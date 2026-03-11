using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Core.UI
{
	public sealed class ProgressBarView : MonoBehaviour
	{
		private static Sprite defaultSprite;

		[SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);
		[SerializeField] private Color fillColor = Color.white;
		[SerializeField] private Vector2 autoFillInset = new Vector2(1f, 1f);

		private Image backgroundImage;
		private Image fillImage;
		private TMP_Text labelText;
		private float progress01 = 1f;
		private string label = string.Empty;

		public Canvas Canvas => GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();

		private void Reset()
		{
			EnsureVisualHierarchy();
			ApplyState();
		}

		private void Awake()
		{
			EnsureVisualHierarchy();
			ApplyState();
		}

		private void OnValidate()
		{
			EnsureVisualHierarchy();
			ApplyState();
		}

		private void OnRectTransformDimensionsChange()
		{
			ApplyProgress();
		}

		public void SetProgress(float ratio01)
		{
			progress01 = Mathf.Clamp01(ratio01);
			ApplyProgress();
		}

		public void SetColors(Color background, Color fill)
		{
			backgroundColor = background;
			fillColor = fill;
			ApplyColors();
		}

		public void SetFillColor(Color color)
		{
			fillColor = color;
			ApplyFillColor();
		}

		public void SetLabel(string value)
		{
			label = value ?? string.Empty;
			ApplyLabel();
		}

		private void EnsureVisualHierarchy()
		{
			backgroundImage = ResolveBackgroundImage();
			fillImage = ResolveFillImage();
			labelText = ResolveLabelText();
		}

		private Image ResolveBackgroundImage()
		{
			if (backgroundImage != null)
			{
				return backgroundImage;
			}

			Image existingImage = GetComponent<Image>();
			if (existingImage != null)
			{
				ApplyDefaultBackgroundStyle(existingImage);
				return existingImage;
			}

			Transform trackTransform = FindNamedChildRecursive(transform, "Track");
			if (trackTransform != null && trackTransform.TryGetComponent(out Image trackImage))
			{
				ApplyDefaultBackgroundStyle(trackImage);
				return trackImage;
			}

			RectTransform rootRect = transform as RectTransform;
			if (rootRect == null)
			{
				Debug.LogWarning("[Erelia.Core.UI.ProgressBarView] Progress bars require a RectTransform.", this);
				return null;
			}

			Image createdImage = gameObject.AddComponent<Image>();
			ApplyDefaultBackgroundStyle(createdImage);
			return createdImage;
		}

		private Image ResolveFillImage()
		{
			if (fillImage != null)
			{
				return fillImage;
			}

			Image existingFill = FindExistingFillImage();
			if (existingFill != null)
			{
				ApplyDefaultFillStyle(existingFill);
				return existingFill;
			}

			RectTransform fillParent = ResolveFillParent();
			if (fillParent == null)
			{
				Debug.LogWarning("[Erelia.Core.UI.ProgressBarView] Failed to resolve a parent for the fill image.", this);
				return null;
			}

			GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			fillObject.transform.SetParent(fillParent, false);

			RectTransform fillTransform = fillObject.transform as RectTransform;
			if (fillTransform != null)
			{
				fillTransform.anchorMin = Vector2.zero;
				fillTransform.anchorMax = Vector2.one;
				fillTransform.offsetMin = autoFillInset;
				fillTransform.offsetMax = -autoFillInset;
			}

			Image createdFill = fillObject.GetComponent<Image>();
			ApplyDefaultFillStyle(createdFill);
			return createdFill;
		}

		private Image FindExistingFillImage()
		{
			Transform namedChild = FindNamedChildRecursive(transform, "Fill");
			if (namedChild != null && namedChild.TryGetComponent(out Image namedFillImage))
			{
				return namedFillImage;
			}

			RectTransform fillParent = ResolveFillParent();
			if (fillParent == null)
			{
				return null;
			}

			for (int i = 0; i < fillParent.childCount; i++)
			{
				Transform child = fillParent.GetChild(i);
				if (child == null || child == backgroundImage?.transform)
				{
					continue;
				}

				if (child.TryGetComponent(out Image childImage))
				{
					return childImage;
				}
			}

			return null;
		}

		private TMP_Text ResolveLabelText()
		{
			if (labelText != null)
			{
				return labelText;
			}

			Transform namedChild = FindNamedChildRecursive(transform, "Label");
			if (namedChild != null && namedChild.TryGetComponent(out TMP_Text namedLabel))
			{
				ApplyExistingLabelStyle(namedLabel);
				return namedLabel;
			}

			TMP_Text existingLabel = GetComponentInChildren<TMP_Text>(true);
			if (existingLabel != null)
			{
				ApplyExistingLabelStyle(existingLabel);
				return existingLabel;
			}

			RectTransform rootRect = transform as RectTransform;
			if (rootRect == null)
			{
				Debug.LogWarning("[Erelia.Core.UI.ProgressBarView] Progress bars require a RectTransform to create a label.", this);
				return null;
			}

			GameObject labelObject = new GameObject(
				"Label",
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(TextMeshProUGUI));
			labelObject.transform.SetParent(transform, false);
			labelObject.transform.SetAsLastSibling();

			RectTransform labelRect = labelObject.transform as RectTransform;
			if (labelRect != null)
			{
				labelRect.anchorMin = Vector2.zero;
				labelRect.anchorMax = Vector2.one;
				labelRect.offsetMin = Vector2.zero;
				labelRect.offsetMax = Vector2.zero;
			}

			TextMeshProUGUI createdLabel = labelObject.GetComponent<TextMeshProUGUI>();
			ApplyCreatedLabelStyle(createdLabel);
			return createdLabel;
		}

		private RectTransform ResolveFillParent()
		{
			if (backgroundImage != null)
			{
				return backgroundImage.rectTransform;
			}

			return transform as RectTransform;
		}

		private static Transform FindNamedChildRecursive(Transform root, string childName)
		{
			if (root == null)
			{
				return null;
			}

			for (int i = 0; i < root.childCount; i++)
			{
				Transform child = root.GetChild(i);
				if (child == null)
				{
					continue;
				}

				if (child.name == childName)
				{
					return child;
				}

				Transform descendant = FindNamedChildRecursive(child, childName);
				if (descendant != null)
				{
					return descendant;
				}
			}

			return null;
		}

		private void ApplyState()
		{
			ApplyColors();
			ApplyProgress();
			ApplyLabel();
		}

		private void ApplyColors()
		{
			ApplyBackgroundColor();
			ApplyFillColor();
		}

		private void ApplyBackgroundColor()
		{
			if (backgroundImage == null)
			{
				return;
			}

			backgroundImage.color = backgroundColor;
		}

		private void ApplyFillColor()
		{
			if (fillImage == null)
			{
				return;
			}

			fillImage.color = fillColor;
		}

		private void ApplyProgress()
		{
			if (fillImage == null)
			{
				return;
			}

			RectTransform fillParent = ResolveFillParent();
			RectTransform fillRect = fillImage.rectTransform;
			if (fillParent == null || fillRect == null)
			{
				return;
			}

			float availableWidth = Mathf.Max(0f, fillParent.rect.width - (autoFillInset.x * 2f));
			float targetWidth = availableWidth * progress01;
			fillRect.anchorMin = new Vector2(0f, 0f);
			fillRect.anchorMax = new Vector2(0f, 1f);
			fillRect.pivot = new Vector2(0f, 0.5f);
			fillRect.anchoredPosition = new Vector2(autoFillInset.x, 0f);
			fillRect.sizeDelta = new Vector2(targetWidth, -(autoFillInset.y * 2f));
			fillImage.enabled = targetWidth > 0.001f;
		}

		private void ApplyLabel()
		{
			if (labelText == null)
			{
				return;
			}

			labelText.text = label;
		}

		private static void ApplyDefaultBackgroundStyle(Image image)
		{
			if (image == null)
			{
				return;
			}

			EnsureImageSprite(image);
			image.raycastTarget = false;
			image.type = ResolveImageType(image.sprite);
		}

		private static void ApplyDefaultFillStyle(Image image)
		{
			if (image == null)
			{
				return;
			}

			EnsureImageSprite(image);
			image.raycastTarget = false;
			image.type = ResolveImageType(image.sprite);
			image.fillAmount = 1f;
		}

		private static void ApplyExistingLabelStyle(TMP_Text text)
		{
			if (text == null)
			{
				return;
			}

			text.raycastTarget = false;

			if (text.font == null)
			{
				text.font = TMP_Settings.defaultFontAsset;
			}
		}

		private static void ApplyCreatedLabelStyle(TextMeshProUGUI text)
		{
			if (text == null)
			{
				return;
			}

			text.raycastTarget = false;
			text.alignment = TextAlignmentOptions.Center;
			text.enableAutoSizing = true;
			text.fontSizeMin = 6f;
			text.fontSizeMax = 18f;
			text.fontSize = 10f;
			text.color = Color.white;
			text.text = string.Empty;
			text.font = TMP_Settings.defaultFontAsset;
		}

		private static void EnsureImageSprite(Image image)
		{
			if (image == null || image.sprite != null)
			{
				return;
			}

			image.sprite = ResolveDefaultSprite();
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
			defaultSprite.name = "ProgressBarDefaultSprite";
			defaultSprite.hideFlags = HideFlags.HideAndDontSave;
			return defaultSprite;
		}

		private static Image.Type ResolveImageType(Sprite sprite)
		{
			return HasSliceBorder(sprite)
				? Image.Type.Sliced
				: Image.Type.Simple;
		}

		private static bool HasSliceBorder(Sprite sprite)
		{
			return sprite != null && sprite.border.sqrMagnitude > 0f;
		}
	}
}
