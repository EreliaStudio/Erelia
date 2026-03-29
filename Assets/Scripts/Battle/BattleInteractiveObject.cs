using System;

[Serializable]
public class BattleInteractiveObject : BattleObject
{
	public InteractionObject InteractionObject;
	public Duration RemainingDuration = new Duration();
};
