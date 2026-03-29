using System;

[Serializable]
public class BattleStatus
{
	public Status Status;
	public int Stack = 0;
	public Duration RemainingDuration = new Duration();
};
