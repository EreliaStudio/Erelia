using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Creature
{
	[Serializable]
	public sealed class Form
	{
		[SerializeField] private string identificationName;
		[SerializeField] private string displayName;
		[SerializeField] private List<Erelia.Core.Creature.FormTag> formTags =
			new List<Erelia.Core.Creature.FormTag>();
		[SerializeField] private Sprite icon;
		[SerializeField] private GameObject unitPrefab;
		[SerializeField] private Erelia.Core.Creature.Stats statModifier =
			new Erelia.Core.Creature.Stats();
		[SerializeField] private Erelia.Core.Creature.FormTier tier =
			Erelia.Core.Creature.FormTier.Base;
		[SerializeField] private List<Erelia.Battle.Attack> grantedActions =
			new List<Erelia.Battle.Attack>();

		public Form()
		{
		}

		public Form(
			string identificationName,
			string displayName,
			Sprite icon,
			GameObject unitPrefab,
			Erelia.Core.Creature.Stats statModifier = null,
			Erelia.Core.Creature.FormTier tier = Erelia.Core.Creature.FormTier.Base,
			IEnumerable<Erelia.Core.Creature.FormTag> formTags = null,
			IEnumerable<Erelia.Battle.Attack> grantedActions = null)
		{
			this.identificationName = identificationName;
			this.displayName = displayName;
			this.icon = icon;
			this.unitPrefab = unitPrefab;
			this.statModifier = statModifier != null
				? statModifier.Clone()
				: new Erelia.Core.Creature.Stats();
			this.tier = tier;

			if (formTags != null)
			{
				this.formTags = new List<Erelia.Core.Creature.FormTag>(formTags);
			}

			if (grantedActions != null)
			{
				this.grantedActions = new List<Erelia.Battle.Attack>(grantedActions);
			}
		}

		public string IdentificationName =>
			string.IsNullOrWhiteSpace(identificationName)
				? string.Empty
				: identificationName.Trim();

		public string DisplayName =>
			string.IsNullOrWhiteSpace(displayName)
				? IdentificationName
				: displayName;

		public IReadOnlyList<Erelia.Core.Creature.FormTag> FormTags =>
			formTags ??= new List<Erelia.Core.Creature.FormTag>();

		public Sprite Icon => icon;

		public GameObject UnitPrefab => unitPrefab;

		public Erelia.Core.Creature.Stats StatModifier =>
			statModifier ??= new Erelia.Core.Creature.Stats();

		public Erelia.Core.Creature.FormTier Tier => tier;

		public IReadOnlyList<Erelia.Battle.Attack> GrantedActions =>
			grantedActions ??= new List<Erelia.Battle.Attack>();

		public bool HasTag(Erelia.Core.Creature.FormTag tag)
		{
			if (tag == Erelia.Core.Creature.FormTag.None)
			{
				return false;
			}

			List<Erelia.Core.Creature.FormTag> tags = formTags;
			if (tags == null)
			{
				return false;
			}

			for (int i = 0; i < tags.Count; i++)
			{
				if (tags[i] == tag)
				{
					return true;
				}
			}

			return false;
		}

		public bool HasAllTags(IReadOnlyList<Erelia.Core.Creature.FormTag> requiredTags)
		{
			if (requiredTags == null || requiredTags.Count == 0)
			{
				return true;
			}

			for (int i = 0; i < requiredTags.Count; i++)
			{
				if (!HasTag(requiredTags[i]))
				{
					return false;
				}
			}

			return true;
		}

		public bool HasAnyTag(IReadOnlyList<Erelia.Core.Creature.FormTag> candidateTags)
		{
			if (candidateTags == null || candidateTags.Count == 0)
			{
				return false;
			}

			for (int i = 0; i < candidateTags.Count; i++)
			{
				if (HasTag(candidateTags[i]))
				{
					return true;
				}
			}

			return false;
		}
	}
}


