using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Attack
{
	[CreateAssetMenu(menuName = "Attack/Attack", fileName = "NewAttack")]
	public class Definition : ScriptableObject
	{
		[SerializeField] private string displayName;
		[SerializeField] private Sprite icon;
		[SerializeField] private int actionPointCost = 1;
		[SerializeField] private bool requiresLineOfSight = true;
		[SerializeField] private int range = 1;
		[SerializeField] private Erelia.Battle.Attack.RangePattern rangePattern =
			Erelia.Battle.Attack.RangePattern.StraightLine;
		[SerializeField] private Erelia.Battle.Attack.TargetType targetType =
			Erelia.Battle.Attack.TargetType.Both;
		[SerializeField] private int areaOfEffectRange;
		[SerializeReference] private List<Erelia.Battle.Attack.Effect.Definition> effects =
			new List<Erelia.Battle.Attack.Effect.Definition>();

		public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
		public Sprite Icon => icon;
		public int ActionPointCost => Mathf.Max(0, actionPointCost);
		public bool RequiresLineOfSight => requiresLineOfSight;
		public int Range => Mathf.Max(0, range);
		public Erelia.Battle.Attack.RangePattern RangePattern => rangePattern;
		public Erelia.Battle.Attack.TargetType TargetType => targetType;
		public int AreaOfEffectRange => Mathf.Max(0, areaOfEffectRange);
		public IReadOnlyList<Erelia.Battle.Attack.Effect.Definition> Effects => effects;
	}
}
