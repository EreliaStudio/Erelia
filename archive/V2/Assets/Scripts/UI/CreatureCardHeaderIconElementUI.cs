using UnityEngine;
using UnityEngine.UI;

public sealed class CreatureCardHeaderIconElementUI : MonoBehaviour
{
	[SerializeField] private Image iconImage;
	[SerializeField] private Sprite emptyIcon;

	private void Awake()
	{
		iconImage ??= GetComponent<Image>();
		if (emptyIcon == null && iconImage != null)
		{
			emptyIcon = iconImage.sprite;
		}
	}

	public void Bind(CreatureUnit p_value)
	{
		iconImage ??= GetComponent<Image>();
		if (emptyIcon == null && iconImage != null)
		{
			emptyIcon = iconImage.sprite;
		}

		if (p_value?.Species == null)
		{
			Apply(emptyIcon);
			return;
		}

		CreatureForm creatureForm = p_value.GetForm();
		Apply(creatureForm.Icon != null ? creatureForm.Icon : emptyIcon);
	}

	public void Clear()
	{
		iconImage ??= GetComponent<Image>();
		Apply(emptyIcon);
	}

	private void Apply(Sprite p_sprite)
	{
		iconImage.sprite = p_sprite;
		iconImage.enabled = p_sprite != null;
	}
}
