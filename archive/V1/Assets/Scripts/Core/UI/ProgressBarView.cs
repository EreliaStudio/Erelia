using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Erelia.Core.UI
{
	public sealed class ProgressBarView : MonoBehaviour
	{
		private static Sprite defaultSprite;

		[SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);
		[SerializeField] private Color fillColor = Color.white;
		[FormerlySerializedAs("autoFillInset")]
		[SerializeField] private Vector2 fillPadding = new Vector2(1f, 1f);

		private Image backgroundImage;
		private Image fillImage;
		private TMP_Text labelText;
		private float progress01 = 1f;
		private string label = string.Empty;
		private bool isApplyingProgress;
#if UNITY_EDITOR
		private bool editorApplyStateQueued;
#endif

		public Canvas Canvas => GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();

		private void Reset()
		{
			EnsureVisualHierarchy(true);
			ApplyState();
		}

		private void Awake()
		{
			EnsureVisualHierarchy(true);
			ApplyState();
		}

		private void OnValidate()
		{
#if UNITY_EDITOR
			QueueEditorApplyState();
#else
			EnsureVisualHierarchy(true);
			ApplyState();
#endif
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

		private void EnsureVisualHierarchy(bool allowCreate)
		{
			backgroundImage = ResolveBackgroundImage(allowCreate);
			fillImage = ResolveFillImage(allowCreate);
			labelText = ResolveLabelText(allowCreate);
		}

		private Image ResolveBackgroundImage(bool allowCreate)
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

			if (!allowCreate)
			{
				return null;
			}

			Image createdImage = gameObject.AddComponent<Image>();
			ApplyDefaultBackgroundStyle(createdImage);
			return createdImage;
		}

		private Image ResolveFillImage(bool allowCreate)
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

			if (!allowCreate)
			{
				return null;
			}

			GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			fillObject.transform.SetParent(fillParent, false);

			RectTransform fillTransform = fillObject.transform as RectTransform;
			if (fillTransform != null)
			{
				fillTransform.anchorMin = Vector2.zero;
				fillTransform.anchorMax = Vector2.one;
				fillTransform.offsetMin = fillPadding;
				fillTransform.offsetMax = -fillPadding;
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

		private TMP_Text ResolveLabelText(bool allowCreate)
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

			if (!allowCreate)
			{
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
			if (fillImage == null || isApplyingProgress)
			{
				return;
			}

			isApplyingProgress = true;
			try
			{
				RectTransform fillParent = ResolveFillParent();
				RectTransform fillRect = fillImage.rectTransform;
				if (fillParent == null || fillRect == null)
				{
					return;
				}

				float availableWidth = Mathf.Max(0f, fillParent.rect.width - (fillPadding.x * 2f));
				float targetWidth = availableWidth * progress01;
				SetRectTransformVector(fillRect, fillRect.anchorMin, new Vector2(0f, 0f), value => fillRect.anchorMin = value);
				SetRectTransformVector(fillRect, fillRect.anchorMax, new Vector2(0f, 1f), value => fillRect.anchorMax = value);
				SetRectTransformVector(fillRect, fillRect.pivot, new Vector2(0f, 0.5f), value => fillRect.pivot = value);
				SetRectTransformVector(fillRect, fillRect.anchoredPosition, new Vector2(fillPadding.x, 0f), value => fillRect.anchoredPosition = value);
				SetRectTransformVector(fillRect, fillRect.sizeDelta, new Vector2(targetWidth, -(fillPadding.y * 2f)), value => fillRect.sizeDelta = value);

				bool shouldShowFill = targetWidth > 0.001f;
				if (fillImage.enabled != shouldShowFill)
				{
					fillImage.enabled = shouldShowFill;
				}
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

		private static void SetRectTransformVector(
			RectTransform rectTransform,
			Vector2 currentValue,
			Vector2 targetValue,
			System.Action<Vector2> setter)
		{
			if (rectTransform == null || setter == null || Approximately(currentValue, targetValue))
			{
				return;
			}

			setter(targetValue);
		}

		private static bool Approximately(Vector2 left, Vector2 right)
		{
			return Mathf.Approximately(left.x, right.x) &&
				Mathf.Approximately(left.y, right.y);
		}

#if UNITY_EDITOR
		private void QueueEditorApplyState()
		{
			if (editorApplyStateQueued)
			{
				return;
			}

			editorApplyStateQueued = true;
			EditorApplication.delayCall += ApplyStateFromEditor;
		}

		private void ApplyStateFromEditor()
		{
			editorApplyStateQueued = false;
			if (this == null)
			{
				return;
			}

			EnsureVisualHierarchy(false);
			ApplyState();
		}
#endif
	}
}
