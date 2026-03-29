using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Effect
{
	public abstract void Apply(CreatureUnit caster, CreatureUnit target);
}

[Serializable]
public class ApplyStatusEffect : Effect
{
	public Status Status;
	public int StackCount = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		
	}
}

[Serializable]
public class RemoveStatusEffect : Effect
{
	public Status Status;
	public int StackCount = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		
	}
}

[Serializable]
public class ReviveEffect : Effect
{
	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class CleanseEffect : Effect
{
	public List<string> TagsToCleanse = new List<string>();

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class ResourceChangeEffect : Effect
{
	public enum Target
	{
		ActionPoint,
		MovementPoint,
		Range
	};

	public Target ResourceTargeted = Target.ActionPoint;
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class MoveStatus : Effect
{
	public enum Orientation
	{
		TowardCaster,
		AwayFromCaster
	};

	public Orientation ForceOrientation = Orientation.AwayFromCaster;
	public int Force = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class SwapPositionEffect : Effect
{
	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class TeleportEffect : Effect
{
	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class StealResourceEffect : Effect
{
	public enum Target
	{
		Health,
		ActionPoint,
		MovementPoint,
		Range,
		Stamina
	};

	public Target ResourceTargeted = Target.ActionPoint;
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class ConsumeStatus : Effect
{
	public Status Status;
	public int NbOfStackConsumed = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
	}
}

[Serializable]
public class ChangeFormEffect : Effect
{
	public string FormID;
	
	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		
	}
}

[Serializable]
public class AdjustTurnBarTimeEffect : Effect
{
	public float Delta = 1f;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		
	}
}

[Serializable]
public class AdjustTurnBarDurationEffect : Effect
{
	public float Delta = 1f;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		
	}
}

[Serializable]
public class DamageTargetEffect : Effect
{
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		
	}
}

[Serializable]
public class HealTargetEffect : Effect
{
	public int Value = 1;

	public override void Apply(CreatureUnit caster, CreatureUnit target)
	{
		
	}
}
