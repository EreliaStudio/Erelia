using UnityEngine;
using UnityEngine.UI;

public sealed class StatusCardElementUI : ObservableValue<BattleStatus>.Listener
{
	[SerializeField] private Image iconImage;

	public void Bind(ObservableValue<BattleStatus> p_status)
	{
		SubscribeTo(p_status);
	}

	public void Clear()
	{
		ClearBinding();
	}

	protected override void ReactToEdition(BattleStatus p_value)
	{
		Apply(p_value?.Status != null ? p_value.Status.Icon : null);
	}

	protected override void ClearRenderedValue()
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
