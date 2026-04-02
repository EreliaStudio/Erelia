using UnityEngine;
using UnityEngine.UI;

public sealed class AbilityCardElementUI : MonoBehaviour
{
	[SerializeField] private Image iconImage;

	public void Bind(Ability p_ability)
	{
		Apply(p_ability != null ? p_ability.Icon : null);
	}

	public void Clear()
	{
		Apply(null);
	}

	private void Apply(Sprite p_icon)
	{
		iconImage ??= GetComponent<Image>();
		iconImage.sprite = p_icon;
		iconImage.enabled = p_icon != null;
	}
}
