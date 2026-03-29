using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class AIRule
{
    [SerializeReference] public List<AICondition> Conditions = new();
    [SerializeReference] public AIDecision Decision;
}
