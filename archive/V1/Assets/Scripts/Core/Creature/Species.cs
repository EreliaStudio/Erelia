using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Erelia.Core.Creature
{
	[CreateAssetMenu(menuName = "Creature/Species", fileName = "NewSpecies")]
	public sealed class Species : ScriptableObject
	{
		[SerializeField] private Sprite icon;

		[FormerlySerializedAs("prefab")]
		[SerializeField] private GameObject unitPrefab;

		[SerializeField] private string identificationName;

		[SerializeField] private string displayName;

		[SerializeField] private Stats stats = new Stats(10, 5f, 6, 3);

		[SerializeField] private List<Erelia.Battle.Attack> defaultActions =
			new List<Erelia.Battle.Attack>();

		[SerializeField] private Erelia.Core.Creature.FeatBoard featBoard;

		[SerializeField] private List<Erelia.Core.Creature.Form> availableForms =
			new List<Erelia.Core.Creature.Form>();

		[NonSerialized] private Erelia.Core.Creature.Form legacyDefaultForm;

		public string IdentificationName =>
			string.IsNullOrWhiteSpace(identificationName)
				? name
				: identificationName;

		public Sprite Icon =>
			DefaultForm != null && DefaultForm.Icon != null
				? DefaultForm.Icon
				: icon;

		public GameObject UnitPrefab =>
			DefaultForm != null && DefaultForm.UnitPrefab != null
				? DefaultForm.UnitPrefab
				: unitPrefab;

		public GameObject Prefab => UnitPrefab;

		public string DisplayName =>
			string.IsNullOrWhiteSpace(displayName)
				? name
				: displayName;

		public Stats Stats => BaseStats;

		public Stats BaseStats => stats ??= new Stats(10, 5f, 6, 3);

		public IReadOnlyList<Erelia.Battle.Attack> DefaultActions =>
			defaultActions ??= new List<Erelia.Battle.Attack>();

		public Erelia.Core.Creature.FeatBoard FeatBoard => featBoard;

		public IReadOnlyList<Erelia.Core.Creature.Form> AvailableForms =>
			availableForms ??= new List<Erelia.Core.Creature.Form>();

		public Erelia.Core.Creature.Form DefaultForm => ResolveForm(null);

		public bool TryGetForm(string formId, out Erelia.Core.Creature.Form form)
		{
			form = ResolveForm(formId);
			return form != null;
		}

		public Erelia.Core.Creature.Form ResolveForm(string formId)
		{
			List<Erelia.Core.Creature.Form> forms = availableForms;
			if (forms != null && forms.Count > 0)
			{
				if (!string.IsNullOrWhiteSpace(formId))
				{
					for (int i = 0; i < forms.Count; i++)
					{
						Erelia.Core.Creature.Form candidate = forms[i];
						if (candidate == null)
						{
							continue;
						}

						if (string.Equals(candidate.IdentificationName, formId, StringComparison.Ordinal))
						{
							return candidate;
						}
					}
				}

				for (int i = 0; i < forms.Count; i++)
				{
					if (forms[i] != null)
					{
						return forms[i];
					}
				}
			}

			return ResolveLegacyDefaultForm();
		}

		private Erelia.Core.Creature.Form ResolveLegacyDefaultForm()
		{
			if (legacyDefaultForm != null)
			{
				return legacyDefaultForm;
			}

			legacyDefaultForm = new Erelia.Core.Creature.Form(
				"default",
				DisplayName,
				icon,
				unitPrefab,
				statModifier: null,
				tier: Erelia.Core.Creature.FormTier.Base,
				formTags: new[] { Erelia.Core.Creature.FormTag.Base },
				grantedActions: null);
			return legacyDefaultForm;
		}
	}
}


