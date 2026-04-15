using System;
using UnityEngine;

[Serializable]
public class EncounterUnit : CreatureUnit
{
	[SerializeReference] public AIBehaviour Behaviour = new AIBehaviour();
}
