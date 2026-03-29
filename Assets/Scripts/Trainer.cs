using System;
using System.Collections.Generic;

[Serializable]
public class Trainer
{
	public EncounterDefinition Encounter;
	public List<TrainerReward> Reward = new List<TrainerReward>();
	public string BeforeFightDialogue = "";
	public string AfterDefeatDialogue = "";
};
