using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	[CreateAssetMenu(menuName = "Attack/Attack", fileName = "NewAttack")]
	public class Attack : ScriptableObject
	{
		[SerializeField] private string identificationName;
		[SerializeField] private string displayName;
		[SerializeField] private Sprite icon;
		[SerializeField] private int actionPointCost = 1;
		[SerializeField] private bool requiresLineOfSight = true;
		[SerializeField] private int range = 1;
		[SerializeField] private Erelia.Battle.RangePattern rangePattern =
			Erelia.Battle.RangePattern.StraightLine;
		[SerializeField] private Erelia.Battle.TargetType targetType =
			Erelia.Battle.TargetType.Both;
		[SerializeField] private int areaOfEffectRange;
		[SerializeField] private bool usableInWorld;
		[SerializeField] private List<Erelia.Core.Creature.FormTag> requiredFormTags =
			new List<Erelia.Core.Creature.FormTag>();
		[SerializeField] private List<Erelia.Core.Creature.FormTag> blockedFormTags =
			new List<Erelia.Core.Creature.FormTag>();
		[SerializeReference] private List<Erelia.Battle.Effects.AttackEffect> effects =
			new List<Erelia.Battle.Effects.AttackEffect>();

		public string IdentificationName => string.IsNullOrWhiteSpace(identificationName) ? name : identificationName;
		public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
		public string Description => BuildDescriptionFromEffects();
		public Sprite Icon => icon;
		public int ActionPointCost => Mathf.Max(0, actionPointCost);
		public bool RequiresLineOfSight => requiresLineOfSight;
		public int Range => Mathf.Max(0, range);
		public Erelia.Battle.RangePattern RangePattern => rangePattern;
		public Erelia.Battle.TargetType TargetType => targetType;
		public int AreaOfEffectRange => Mathf.Max(0, areaOfEffectRange);
		public bool UsableInWorld => usableInWorld;
		public IReadOnlyList<Erelia.Core.Creature.FormTag> RequiredFormTags =>
			requiredFormTags ??= new List<Erelia.Core.Creature.FormTag>();
		public IReadOnlyList<Erelia.Core.Creature.FormTag> BlockedFormTags =>
			blockedFormTags ??= new List<Erelia.Core.Creature.FormTag>();
		public IReadOnlyList<Erelia.Battle.Effects.AttackEffect> Effects =>
			effects ??= new List<Erelia.Battle.Effects.AttackEffect>();

		public bool IsAllowedFor(Erelia.Core.Creature.Form form)
		{
			if (requiredFormTags != null && requiredFormTags.Count > 0)
			{
				if (form == null || !form.HasAllTags(requiredFormTags))
				{
					return false;
				}
			}

			if (blockedFormTags != null && blockedFormTags.Count > 0 && form != null && form.HasAnyTag(blockedFormTags))
			{
				return false;
			}

			return true;
		}

		private string BuildDescriptionFromEffects()
		{
			IReadOnlyList<Erelia.Battle.Effects.AttackEffect> configuredEffects = Effects;
			if (configuredEffects == null || configuredEffects.Count == 0)
			{
				return string.Empty;
			}

			var descriptionParts = new List<string>(configuredEffects.Count);
			for (int i = 0; i < configuredEffects.Count; i++)
			{
				Erelia.Battle.Effects.AttackEffect effect = configuredEffects[i];
				if (effect == null)
				{
					continue;
				}

				string part = effect.BuildDescription();
				if (!string.IsNullOrWhiteSpace(part))
				{
					descriptionParts.Add(part);
				}
			}

			if (descriptionParts.Count == 0)
			{
				return string.Empty;
			}

			return string.Join(". ", descriptionParts) + ".";
		}
	}
}

