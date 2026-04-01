using UnityEngine;
using UnityEngine.UI;

public class StatusCardElementUI : MonoBehaviour
{
	[SerializeField] private Image iconImage;

	private Status linkedStatus;

	public void Bind(Status p_status)
	{
		linkedStatus = p_status;
		Refresh();
	}

	public void Clear()
	{
		linkedStatus = null;
		Refresh();
	}

	public void Refresh()
	{
		Sprite icon = linkedStatus != null ? linkedStatus.Icon : null;

		if (iconImage != null)
		{
			iconImage.sprite = icon;
			iconImage.enabled = icon != null;
		}
	}
}