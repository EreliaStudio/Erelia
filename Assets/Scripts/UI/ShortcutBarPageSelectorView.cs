using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[ExecuteAlways]
public sealed class ShortcutBarPageSelectorView : ExecuteAlwaysView
{
	private const float SelectorWidth = 24f;
	private const float ButtonHeight = 14f;
	private const float LabelHeight = 14f;
	private const float VerticalPadding = 1f;

	[SerializeField] private Button incrementButton;
	[SerializeField] private TextMeshProUGUI indexLabel;
	[SerializeField] private Button decrementButton;

	private int currentIndex;
	private int maxIndex;

	public event Action<int> IndexChanged;
	public int CurrentIndex => currentIndex;
	public int MaxIndex => maxIndex;

	private void Reset()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
	}

	private void Awake()
	{
		EnsureHierarchy(true);
	}

	private void OnEnable()
	{
		EnsureHierarchy(true);
		SubscribeButtons();
		Refresh();
	}

	private void OnDisable()
	{
		UnsubscribeButtons();
	}

	private void OnValidate()
	{
#if UNITY_EDITOR
		QueueEditorRefresh();
#else
		EnsureHierarchy(true);
		Refresh();
#endif
	}

	public void SetRange(int maximumIndex)
	{
		maxIndex = Mathf.Max(0, maximumIndex);
		SetIndex(currentIndex, notify: currentIndex > maxIndex);
		Refresh();
	}

	public void SetIndex(int index, bool notify = true)
	{
		int clampedIndex = Mathf.Clamp(index, 0, maxIndex);
		if (currentIndex == clampedIndex)
		{
			Refresh();
			return;
		}

		currentIndex = clampedIndex;
		Refresh();

		if (notify)
		{
			IndexChanged?.Invoke(currentIndex);
		}
	}

	[ContextMenu("Refresh")]
	public void RefreshNow()
	{
		EnsureHierarchy(true);
		ApplySerializedState();
		Refresh();
	}

	private void EnsureHierarchy(bool allowCreate)
	{
		incrementButton = ResolveButton(incrementButton, "Increment", allowCreate, "+");
		indexLabel = ResolveLabel(indexLabel, "IndexLabel", allowCreate);
		decrementButton = ResolveButton(decrementButton, "Decrement", allowCreate, "-");
	}

	private Button ResolveButton(Button current, string childName, bool allowCreate, string labelText)
	{
		if (current != null)
		{
			EnsureButtonLabel(current, labelText);
			return current;
		}

		Transform child = transform.Find(childName);
		if (child != null && child.TryGetComponent(out Button existing))
		{
			EnsureButtonLabel(existing, labelText);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject buttonObject = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
		buttonObject.transform.SetParent(transform, false);
		buttonObject.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
		ApplySelectorChildLayout(buttonObject.GetComponent<LayoutElement>(), ButtonHeight);
		Button button = buttonObject.GetComponent<Button>();
		EnsureButtonLabel(button, labelText);
		return button;
	}

	private TextMeshProUGUI ResolveLabel(TextMeshProUGUI current, string childName, bool allowCreate)
	{
		if (current != null)
		{
			ApplyLabelDefaults(current);
			return current;
		}

		Transform child = transform.Find(childName);
		if (child != null && child.TryGetComponent(out TextMeshProUGUI existing))
		{
			ApplyLabelDefaults(existing);
			return existing;
		}

		if (!allowCreate)
		{
			return null;
		}

		GameObject labelObject = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
		labelObject.transform.SetParent(transform, false);
		TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
		ApplyLabelDefaults(label);
		ApplySelectorChildLayout(labelObject.GetComponent<LayoutElement>(), LabelHeight);
		return label;
	}

	private void EnsureButtonLabel(Button button, string labelText)
	{
		if (button == null)
		{
			return;
		}

		Transform labelTransform = button.transform.Find("Label");
		TextMeshProUGUI label;
		if (labelTransform != null && labelTransform.TryGetComponent(out label))
		{
			ApplyLabelDefaults(label);
		}
		else
		{
			GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			labelObject.transform.SetParent(button.transform, false);
			label = labelObject.GetComponent<TextMeshProUGUI>();
			ApplyLabelDefaults(label);
			UiViewUtility.Stretch(label.rectTransform);
		}

		label.text = labelText;
	}

	private void ApplySerializedState()
	{
		ApplySelectorChildLayout(incrementButton != null ? incrementButton.GetComponent<LayoutElement>() : null, ButtonHeight);
		ApplySelectorChildLayout(indexLabel != null ? indexLabel.GetComponent<LayoutElement>() : null, LabelHeight);
		ApplySelectorChildLayout(decrementButton != null ? decrementButton.GetComponent<LayoutElement>() : null, ButtonHeight);
		ApplySelectorRootLayout();

		if (TryGetComponent(out VerticalLayoutGroup layoutGroup))
		{
			layoutGroup.enabled = true;
			layoutGroup.childControlWidth = true;
			layoutGroup.childControlHeight = true;
			layoutGroup.childForceExpandWidth = true;
			layoutGroup.childForceExpandHeight = true;
			return;
		}

		ApplyButtonRect(incrementButton, new Vector2(0.5f, 1f), new Vector2(0f, -VerticalPadding), ButtonHeight);
		ApplyLabelRect(indexLabel, new Vector2(0.5f, 0.5f), Vector2.zero, LabelHeight);
		ApplyButtonRect(decrementButton, new Vector2(0.5f, 0f), new Vector2(0f, VerticalPadding), ButtonHeight);
	}

	private void SubscribeButtons()
	{
		UnsubscribeButtons();
		if (incrementButton != null)
		{
			incrementButton.onClick.AddListener(Increment);
		}

		if (decrementButton != null)
		{
			decrementButton.onClick.AddListener(Decrement);
		}
	}

	private void UnsubscribeButtons()
	{
		if (incrementButton != null)
		{
			incrementButton.onClick.RemoveListener(Increment);
		}

		if (decrementButton != null)
		{
			decrementButton.onClick.RemoveListener(Decrement);
		}
	}

	private void Increment()
	{
		SetIndex(currentIndex + 1);
	}

	private void Decrement()
	{
		SetIndex(currentIndex - 1);
	}

	private void Refresh()
	{
		currentIndex = Mathf.Clamp(currentIndex, 0, maxIndex);

		if (indexLabel != null)
		{
			indexLabel.text = currentIndex.ToString();
		}

		if (incrementButton != null)
		{
			incrementButton.interactable = currentIndex < maxIndex;
		}

		if (decrementButton != null)
		{
			decrementButton.interactable = currentIndex > 0;
		}
	}

	private static void ApplyLabelDefaults(TextMeshProUGUI text)
	{
		text.raycastTarget = false;
		text.color = Color.white;
		text.alignment = TextAlignmentOptions.Center;
		text.enableAutoSizing = true;
		text.fontSizeMin = 10f;
		text.fontSizeMax = 20f;
		text.fontSize = 16f;
		text.margin = Vector4.zero;

		if (text.font == null)
		{
			text.font = TMP_Settings.defaultFontAsset;
		}
	}

	private static void ApplySelectorChildLayout(LayoutElement layoutElement, float height)
	{
		if (layoutElement == null)
		{
			return;
		}

		layoutElement.minWidth = SelectorWidth;
		layoutElement.preferredWidth = SelectorWidth;
		layoutElement.flexibleWidth = 0f;
		layoutElement.minHeight = height;
		layoutElement.preferredHeight = height;
		layoutElement.flexibleHeight = 0f;
	}

	private void ApplySelectorRootLayout()
	{
		if (!TryGetComponent(out RectTransform rectTransform))
		{
			return;
		}

		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, SelectorWidth);
	}

	private static void ApplyButtonRect(Button button, Vector2 anchor, Vector2 anchoredPosition, float height)
	{
		if (button == null)
		{
			return;
		}

		ApplyChildRect(button.GetComponent<RectTransform>(), anchor, anchoredPosition, height);
	}

	private static void ApplyLabelRect(TextMeshProUGUI label, Vector2 anchor, Vector2 anchoredPosition, float height)
	{
		if (label == null)
		{
			return;
		}

		ApplyChildRect(label.rectTransform, anchor, anchoredPosition, height);
	}

	private static void ApplyChildRect(RectTransform rectTransform, Vector2 anchor, Vector2 anchoredPosition, float height)
	{
		if (rectTransform == null)
		{
			return;
		}

		rectTransform.anchorMin = anchor;
		rectTransform.anchorMax = anchor;
		rectTransform.pivot = anchor;
		rectTransform.anchoredPosition = anchoredPosition;
		rectTransform.sizeDelta = new Vector2(SelectorWidth, height);
	}

#if UNITY_EDITOR
	protected override void OnEditorRefresh()
	{
		EnsureHierarchy(true);
		Refresh();
	}
#endif
}
