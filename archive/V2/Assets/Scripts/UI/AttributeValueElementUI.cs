using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class AttributeValueElementUI : ObservableValue<int>.Listener
{
	[SerializeField] private Image iconImage;
	[SerializeField] private TMP_Text valueLabel;
	[SerializeField] private Sprite iconSprite;

	private Sprite fallbackIconSprite;

	private void Awake()
	{
		ResolveReferences();
		CacheFallbackIcon();
		ApplyIcon();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		ResolveReferences();
		CacheFallbackIcon();
		ApplyIcon();
	}
#endif

	protected override void ReactToEdition(int p_value)
	{
		ResolveReferences();
		ApplyIcon();
		valueLabel.text = p_value.ToString();
	}

	protected override void ClearRenderedValue()
	{
		ResolveReferences();
		ApplyIcon();
		valueLabel.text = string.Empty;
	}

	private void ResolveReferences()
	{
		iconImage ??= GetComponentInChildren<Image>(true);
		valueLabel ??= GetComponentInChildren<TMP_Text>(true);
	}

	private void CacheFallbackIcon()
	{
		if (fallbackIconSprite == null && iconImage != null)
		{
			fallbackIconSprite = iconImage.sprite;
		}
	}

	private void ApplyIcon()
	{
		if (iconImage == null)
		{
			return;
		}

		Sprite sprite = iconSprite != null ? iconSprite : fallbackIconSprite;
		iconImage.sprite = sprite;
		iconImage.enabled = sprite != null;
	}
}
