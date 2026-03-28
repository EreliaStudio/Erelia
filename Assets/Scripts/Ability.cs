using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Ability")]
public class Ability : ScriptableObject
{
	public AbilityCost Cost = new AbilityCost();
	public RangeType RangeType = RangeType.Circle;
	public int RangeValue = 10;
	public bool RequireLineOfSight = false;
	public AreaOfEffectType AreaOfEffectType = AreaOfEffectType.Cross;
	public int AreaOfEffectValue = 1;

	public TargetProfile TargetProfile = TargetProfile.Everything;

	[SerializeReference] public List<Effect> Effects = new List<Effect>();
}