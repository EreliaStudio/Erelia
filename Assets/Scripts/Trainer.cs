using System;

[Serializable]
public class Trainer
{
	public class Unit
	{
		CreatureUnit CreatureUnit;
		AIBehaviour AIBehaviour;
	};

	public Unit[] Team = new Unit[6];
	public TrainerReward Reward;
};