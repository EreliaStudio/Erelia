using System;

[Serializable]
public abstract class AIDecision
{
	
};

[Serializable]
public class CastAbility : AIDecision
{
}

[Serializable]
public class MoveUnit : AIDecision
{
}

[Serializable]
public class EndTurn : AIDecision
{
}