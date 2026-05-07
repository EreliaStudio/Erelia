using System;

[Serializable]
public sealed class BattleHookContext
{
	public BattleContext BattleContext { get; set; }
	public StatusHookPoint HookPoint { get; set; }
	public BattleUnit HookOwner { get; set; }
	public BattleObject SourceObject { get; set; }
	public BattleObject TargetObject { get; set; }
	public BattleAction Action { get; set; }

	public BattleUnit SourceUnit => SourceObject as BattleUnit;
	public BattleUnit TargetUnit => TargetObject as BattleUnit;
	public bool HasAction => Action != null;
}
