using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusCardElementUI : MonoBehaviour
{
	[SerializeField] private Image iconImage;

	private Status _linkedStatus;

	public Status LinkedStatus => _linkedStatus;

	public void Bind(Status p_ability)
	{
		_linkedStatus = p_ability;
		Refresh();
	}

	public void Clear()
	{
		_linkedStatus = null;
		Refresh();
	}

	public void Refresh()
	{
		iconImage.sprite = _linkedStatus != null ? _linkedStatus.Icon : null;
		iconImage.enabled = iconImage.sprite != null;
	}
}