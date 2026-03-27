using System;

[Serializable]
public class Attributes
{
	public StatValue Health = new StatValue(10);

	public StatValue ActionPoints = new StatValue(6);

	public StatValue Movement = new StatValue(3);

	public int Attack = 0;
	public int Armor = 0;
	public int Magic = 0;
	public int Resistance = 0;
	public int BonusRange = 0;

	public float Recovery = 4.0f;
};