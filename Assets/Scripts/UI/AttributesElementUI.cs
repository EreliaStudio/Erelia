using TMPro;
using UnityEngine;

public class AttributesElementUI : MonoBehaviour
{
	[SerializeField] private TMP_Text healthValueLabel;
	[SerializeField] private TMP_Text actionPointsValueLabel;
	[SerializeField] private TMP_Text movementValueLabel;
	[SerializeField] private TMP_Text attackValueLabel;
	[SerializeField] private TMP_Text armorValueLabel;
	[SerializeField] private TMP_Text magicValueLabel;
	[SerializeField] private TMP_Text resistanceValueLabel;
	[SerializeField] private TMP_Text bonusRangeValueLabel;
	[SerializeField] private TMP_Text recoveryValueLabel;

	private Attributes linkedAttributes;

	public void Bind(Attributes p_attributes)
	{
		linkedAttributes = p_attributes;
		Refresh();
	}

	public void Clear()
	{
		linkedAttributes = null;
		Refresh();
	}

	public void Refresh()
	{
		if (linkedAttributes == null)
		{
			SetText(healthValueLabel, string.Empty);
			SetText(actionPointsValueLabel, string.Empty);
			SetText(movementValueLabel, string.Empty);
			SetText(attackValueLabel, string.Empty);
			SetText(armorValueLabel, string.Empty);
			SetText(magicValueLabel, string.Empty);
			SetText(resistanceValueLabel, string.Empty);
			SetText(bonusRangeValueLabel, string.Empty);
			SetText(recoveryValueLabel, string.Empty);
			return;
		}

		SetText(healthValueLabel, $"HP : {linkedAttributes.Health}");
		SetText(actionPointsValueLabel, $"AP : {linkedAttributes.ActionPoints}");
		SetText(movementValueLabel, $"MP : {linkedAttributes.Movement}");
		SetText(attackValueLabel, $"ATK : {linkedAttributes.Attack}");
		SetText(armorValueLabel, $"ARM : {linkedAttributes.Armor}");
		SetText(magicValueLabel, $"MAG : {linkedAttributes.Magic}");
		SetText(resistanceValueLabel, $"RES : {linkedAttributes.Resistance}");
		SetText(bonusRangeValueLabel, $"Bonus Range : {linkedAttributes.BonusRange}");
		SetText(recoveryValueLabel, $"Recovery : {linkedAttributes.Recovery:0.##}");
	}

	private static void SetText(TMP_Text p_text, string p_value)
	{
		if (p_text == null)
		{
			return;
		}

		p_text.text = p_value;
	}
}