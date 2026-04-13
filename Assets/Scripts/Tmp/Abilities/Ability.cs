using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Game/Ability")]
public class Ability : ScriptableObject
{
	[Serializable] 
	public class RangeDefinition
	{
		public enum Shape
		{
			Circle,
			Line,
			Diagonal
		};

		public Shape Type;
		public int Value;
		public bool RequireLineOfSight = false;
	};
	
	[Serializable]
	public class AreaOfEffectDefinition
	{
		public enum Shape
		{
			Square,
			Cross,
			Circle
		};

		public Shape Type;
		public int Value;
	};

	public Sprite Icon = null;
	public AbilityCost Cost = new AbilityCost();
	public RangeDefinition Range = new RangeDefinition{
		Type = RangeDefinition.Shape.Circle,
		Value = 10,
		RequireLineOfSight = true
	};
	
	public AreaOfEffectDefinition AreaOfEffect = new AreaOfEffectDefinition{
		Type = AreaOfEffectDefinition.Shape.Cross,
		Value = 10
	};

	public TargetProfile TargetProfile = TargetProfile.Everything;

	[SerializeReference] public List<Effect> Effects = new List<Effect>();
}
