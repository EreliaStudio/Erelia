using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.UI
{
	public sealed class BattleResultEntryView : MonoBehaviour
	{
		private static readonly Color DefaultBackgroundColor = new Color(0.09f, 0.11f, 0.16f, 0.94f);
		private static readonly Color DefaultDescriptionColor = new Color(0.83f, 0.87f, 0.93f, 1f);
		private static Sprite defaultIconSprite;
		private RectTransform rowRoot;
		private RectTransform textColumnRoot;

		[SerializeField] private TMP_Text creatureNameText;
		[SerializeField] private TMP_Text detailLineAText;
		[SerializeField] private TMP_Text detailLineBText;
		[SerializeField] private Image iconImage;
		[SerializeField] private Erelia.Core.UI.ProgressBarView progressBar;

		private void Awake()
		{
			EnsureWidgetHierarchy();
		}

		public void Apply(Erelia.Battle.BattleResultEntry data)
		{
			EnsureWidgetHierarchy();

			if (creatureNameText != null)
			{
				creatureNameText.text = string.IsNullOrEmpty(data.Title) ? "Unknown feat" : data.Title;
				creatureNameText.color = Color.white;
			}

			if (detailLineAText != null)
			{
				detailLineAText.text = data.Description ?? string.Empty;
				detailLineAText.color = DefaultDescriptionColor;
			}

			if (detailLineBText != null)
			{
				detailLineBText.gameObject.SetActive(false);
			}

			if (iconImage != null)
			{
				iconImage.sprite = ResolveDefaultIconSprite();
				iconImage.color = data.AccentColor;
			}

			if (progressBar != null)
			{
				progressBar.SetColors(new Color(1f, 1f, 1f, 0.08f), data.AccentColor);
				progressBar.SetProgress(data.Progress01);
				progressBar.SetLabel(data.ProgressLabel);
			}
		}

		private void EnsureWidgetHierarchy()
		{
			ConfigureRootVisuals();
			EnsureStructuredLayout();
			creatureNameText ??= ResolveOrCreateText("Title", 0, 22f, FontStyles.Bold);
			detailLineAText ??= ResolveOrCreateText("Description", 1, 17f, FontStyles.Normal);

			if (detailLineBText != null)
			{
				detailLineBText.gameObject.SetActive(false);
			}

			iconImage ??= ResolveOrCreateIcon();
			progressBar ??= ResolveOrCreateProgressBar();
		}

		private void ConfigureRootVisuals()
		{
			if (!TryGetComponent(out VerticalLayoutGroup layoutGroup))
			{
				layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
			}

			layoutGroup.childAlignment = TextAnchor.UpperLeft;
			layoutGroup.childControlWidth = true;
			layoutGroup.childControlHeight = true;
			layoutGroup.childForceExpandWidth = true;
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.spacing = 8f;
			layoutGroup.padding = new RectOffset(16, 16, 14, 14);

			if (!TryGetComponent(out Image backgroundImage))
			{
				backgroundImage = gameObject.AddComponent<Image>();
			}

			backgroundImage.sprite = ResolveDefaultIconSprite();
			backgroundImage.type = Image.Type.Simple;
			backgroundImage.color = DefaultBackgroundColor;
			backgroundImage.raycastTarget = false;

			if (!TryGetComponent(out LayoutElement layoutElement))
			{
				layoutElement = gameObject.AddComponent<LayoutElement>();
			}

			layoutElement.preferredHeight = 164f;
			layoutElement.minHeight = 164f;
			layoutElement.flexibleHeight = 0f;
		}

		private void EnsureStructuredLayout()
		{
			rowRoot = ResolveOrCreateContainer("RowRoot", transform, 0);
			ConfigureRowRoot(rowRoot);

			textColumnRoot = ResolveOrCreateContainer("TextColumn", rowRoot, 1);
			ConfigureTextColumn(textColumnRoot);
		}

		private TMP_Text ResolveOrCreateText(string objectName, int siblingIndex, float fontSize, FontStyles fontStyle)
		{
			TMP_Text resolvedText = FindChildText(objectName);
			if (resolvedText == null)
			{
				resolvedText = objectName == "Title"
					? creatureNameText
					: objectName == "Description"
						? detailLineAText
						: detailLineBText;
			}

			if (resolvedText == null)
			{
				GameObject textObject = new GameObject(
					objectName,
					typeof(RectTransform),
					typeof(CanvasRenderer),
					typeof(TextMeshProUGUI));
				textObject.transform.SetParent(transform, false);
				resolvedText = textObject.GetComponent<TextMeshProUGUI>();
			}

			Transform textTransform = resolvedText.transform;
			textTransform.SetParent(textColumnRoot != null ? textColumnRoot : transform, false);
			textTransform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, textTransform.parent.childCount - 1));

			if (resolvedText is TextMeshProUGUI textMesh)
			{
				textMesh.font = TMP_Settings.defaultFontAsset;
				textMesh.fontSize = fontSize;
				textMesh.fontStyle = fontStyle;
				textMesh.textWrappingMode = TextWrappingModes.Normal;
				textMesh.alignment = TextAlignmentOptions.TopLeft;
				textMesh.overflowMode = TextOverflowModes.Ellipsis;
				textMesh.raycastTarget = false;
				textMesh.margin = Vector4.zero;
			}

			if (!resolvedText.TryGetComponent(out LayoutElement layoutElement))
			{
				layoutElement = resolvedText.gameObject.AddComponent<LayoutElement>();
			}

			layoutElement.minHeight = objectName == "Description" ? 48f : 28f;
			layoutElement.preferredHeight = layoutElement.minHeight;
			layoutElement.flexibleWidth = 1f;
			layoutElement.flexibleHeight = 0f;
			return resolvedText;
		}

		private Image ResolveOrCreateIcon()
		{
			Image resolvedImage = FindChildImage("FeatIcon");
			if (resolvedImage == null)
			{
				GameObject iconObject = new GameObject(
					"FeatIcon",
					typeof(RectTransform),
					typeof(CanvasRenderer),
					typeof(Image),
					typeof(LayoutElement));
				iconObject.transform.SetParent(rowRoot != null ? rowRoot : transform, false);
				iconObject.transform.SetSiblingIndex(0);
				resolvedImage = iconObject.GetComponent<Image>();
			}

			resolvedImage.transform.SetParent(rowRoot != null ? rowRoot : transform, false);
			resolvedImage.transform.SetSiblingIndex(0);
			resolvedImage.sprite = ResolveDefaultIconSprite();
			resolvedImage.raycastTarget = false;
			resolvedImage.preserveAspect = true;

			LayoutElement iconLayout = resolvedImage.GetComponent<LayoutElement>();
			iconLayout.preferredWidth = 56f;
			iconLayout.preferredHeight = 56f;
			iconLayout.minWidth = 56f;
			iconLayout.minHeight = 56f;
			iconLayout.flexibleWidth = 0f;
			iconLayout.flexibleHeight = 0f;
			return resolvedImage;
		}

		private Erelia.Core.UI.ProgressBarView ResolveOrCreateProgressBar()
		{
			Erelia.Core.UI.ProgressBarView resolvedProgressBar = FindChildProgressBar("ProgressBar");
			if (resolvedProgressBar == null)
			{
				GameObject progressObject = new GameObject(
					"ProgressBar",
					typeof(RectTransform),
					typeof(CanvasRenderer),
					typeof(Image),
					typeof(LayoutElement),
					typeof(Erelia.Core.UI.ProgressBarView));
				progressObject.transform.SetParent(transform, false);
				resolvedProgressBar = progressObject.GetComponent<Erelia.Core.UI.ProgressBarView>();
			}

			resolvedProgressBar.transform.SetParent(transform, false);
			resolvedProgressBar.transform.SetAsLastSibling();

			if (resolvedProgressBar.TryGetComponent(out LayoutElement layoutElement))
			{
				layoutElement.preferredHeight = 28f;
				layoutElement.minHeight = 28f;
				layoutElement.flexibleWidth = 1f;
				layoutElement.flexibleHeight = 0f;
			}

			return resolvedProgressBar;
		}

		private RectTransform ResolveOrCreateContainer(
			string objectName,
			Transform parent,
			int siblingIndex)
		{
			Transform existingTransform = parent != null ? parent.Find(objectName) : null;
			RectTransform rectTransform = existingTransform as RectTransform;
			if (rectTransform == null)
			{
				GameObject containerObject = new GameObject(
					objectName,
					typeof(RectTransform),
					typeof(LayoutElement));
				containerObject.transform.SetParent(parent, false);
				rectTransform = containerObject.GetComponent<RectTransform>();
			}

			rectTransform.SetParent(parent, false);
			rectTransform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, rectTransform.parent.childCount - 1));
			return rectTransform;
		}

		private static void ConfigureRowRoot(RectTransform rectTransform)
		{
			if (rectTransform == null)
			{
				return;
			}

			if (!rectTransform.TryGetComponent(out HorizontalLayoutGroup layoutGroup))
			{
				layoutGroup = rectTransform.gameObject.AddComponent<HorizontalLayoutGroup>();
			}

			layoutGroup.childAlignment = TextAnchor.UpperLeft;
			layoutGroup.childControlWidth = true;
			layoutGroup.childControlHeight = true;
			layoutGroup.childForceExpandWidth = false;
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.spacing = 14f;

			LayoutElement layoutElement = rectTransform.GetComponent<LayoutElement>();
			layoutElement.minHeight = 84f;
			layoutElement.preferredHeight = 84f;
			layoutElement.flexibleHeight = 0f;
			layoutElement.flexibleWidth = 1f;
		}

		private static void ConfigureTextColumn(RectTransform rectTransform)
		{
			if (rectTransform == null)
			{
				return;
			}

			if (!rectTransform.TryGetComponent(out VerticalLayoutGroup layoutGroup))
			{
				layoutGroup = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
			}

			layoutGroup.childAlignment = TextAnchor.UpperLeft;
			layoutGroup.childControlWidth = true;
			layoutGroup.childControlHeight = true;
			layoutGroup.childForceExpandWidth = true;
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.spacing = 6f;

			LayoutElement layoutElement = rectTransform.GetComponent<LayoutElement>();
			layoutElement.minWidth = 0f;
			layoutElement.preferredWidth = 0f;
			layoutElement.flexibleWidth = 1f;
			layoutElement.flexibleHeight = 0f;
		}

		private TMP_Text FindChildText(string childName)
		{
			Transform child = transform.Find(childName);
			return child != null ? child.GetComponent<TMP_Text>() : null;
		}

		private Image FindChildImage(string childName)
		{
			Transform child = transform.Find(childName);
			return child != null ? child.GetComponent<Image>() : null;
		}

		private Erelia.Core.UI.ProgressBarView FindChildProgressBar(string childName)
		{
			Transform child = transform.Find(childName);
			return child != null ? child.GetComponent<Erelia.Core.UI.ProgressBarView>() : null;
		}

		private static Sprite ResolveDefaultIconSprite()
		{
			if (defaultIconSprite != null)
			{
				return defaultIconSprite;
			}

			Texture2D texture = Texture2D.whiteTexture;
			defaultIconSprite = Sprite.Create(
				texture,
				new Rect(0f, 0f, texture.width, texture.height),
				new Vector2(0.5f, 0.5f),
				100f);
			defaultIconSprite.name = "BattleResultDefaultIcon";
			defaultIconSprite.hideFlags = HideFlags.HideAndDontSave;
			return defaultIconSprite;
		}
	}
}

