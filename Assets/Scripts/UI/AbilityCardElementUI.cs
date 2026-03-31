using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCardElementUI : MonoBehaviour
{
	[SerializeField] private Image iconImage;

	private Ability _linkedAbility;

	public Ability LinkedAbility => _linkedAbility;

	public void Bind(Ability p_ability)
	{
		_linkedAbility = p_ability;
		Refresh();
	}

	public void Clear()
	{
		_linkedAbility = null;
		Refresh();
	}

	public void Refresh()
	{
		iconImage.sprite = _linkedAbility != null ? _linkedAbility.Icon : null;
		iconImage.enabled = iconImage.sprite != null;
	}
}