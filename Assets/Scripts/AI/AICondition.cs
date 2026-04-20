using System;

[Serializable]
public abstract class AICondition
{
	public abstract bool IsMet(BattleUnit caster, BoardData battleContext);
};
