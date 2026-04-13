using UnityEngine;

public sealed class AttributesElementUI : MonoBehaviour
{
	[SerializeField] private ObservableResourceBarElementUI healthBarElementUI;
	[SerializeField] private ObservableResourceBarElementUI actionPointsBarElementUI;
	[SerializeField] private ObservableResourceBarElementUI movementPointsBarElementUI;
	[SerializeField] private AttributeValueElementUI attackValueElementUI;
	[SerializeField] private AttributeValueElementUI armorValueElementUI;
	[SerializeField] private AttributeValueElementUI magicValueElementUI;
	[SerializeField] private AttributeValueElementUI resistanceValueElementUI;

	private BattleUnit linkedBattleUnit;

	public void Bind(BattleUnit p_battleUnit)
	{
		linkedBattleUnit = p_battleUnit;

		if (p_battleUnit == null)
		{
			healthBarElementUI.SubscribeTo(null);
			actionPointsBarElementUI.SubscribeTo(null);
			movementPointsBarElementUI.SubscribeTo(null);
			attackValueElementUI.SubscribeTo(null);
			armorValueElementUI.SubscribeTo(null);
			magicValueElementUI.SubscribeTo(null);
			resistanceValueElementUI.SubscribeTo(null);
			return;
		}

		healthBarElementUI.SubscribeTo(p_battleUnit.BattleAttributes.Health);
		actionPointsBarElementUI.SubscribeTo(p_battleUnit.BattleAttributes.ActionPoints);
		movementPointsBarElementUI.SubscribeTo(p_battleUnit.BattleAttributes.MovementPoints);
		attackValueElementUI.SubscribeTo(p_battleUnit.BattleAttributes.Attack);
		armorValueElementUI.SubscribeTo(p_battleUnit.BattleAttributes.Armor);
		magicValueElementUI.SubscribeTo(p_battleUnit.BattleAttributes.Magic);
		resistanceValueElementUI.SubscribeTo(p_battleUnit.BattleAttributes.Resistance);
	}

	public void Clear()
	{
		Bind(null);
	}

	public void Refresh()
	{
		Bind(linkedBattleUnit);
	}
}
