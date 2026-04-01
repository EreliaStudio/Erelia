using UnityEngine;
using UnityEngine.UI;

public class AbilityCardElementUI : MonoBehaviour
{
	[SerializeField] private Image iconImage;

	private Ability linkedAbility;

	public void Bind(Ability p_ability)
	{
		linkedAbility = p_ability;
		Refresh();
	}

	public void Clear()
	{
		linkedAbility = null;
		Refresh();
	}

	public void Refresh()
	{
		Sprite icon = linkedAbility != null ? linkedAbility.Icon : null;

		if (iconImage != null)
		{
			iconImage.sprite = icon;
			iconImage.enabled = icon != null;
		}
	}
}
