using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	public sealed class CreatureProjection
	{
		private readonly Erelia.Battle.Attack[] equippedActions;

		public CreatureProjection(
			string unitId,
			string displayName,
			Erelia.Core.Creature.Species species,
			Erelia.Core.Creature.Form form,
			Erelia.Core.Creature.Stats stats,
			Erelia.Battle.Attack[] equippedActions)
		{
			UnitId = unitId ?? string.Empty;
			DisplayName = displayName ?? string.Empty;
			Species = species;
			Form = form;
			Stats = stats != null
				? stats.Clone()
				: new Erelia.Core.Creature.Stats();

			if (equippedActions != null && equippedActions.Length > 0)
			{
				this.equippedActions = new Erelia.Battle.Attack[equippedActions.Length];
				Array.Copy(equippedActions, this.equippedActions, equippedActions.Length);
			}
			else
			{
				this.equippedActions = Array.Empty<Erelia.Battle.Attack>();
			}
		}

		public string UnitId { get; }

		public string DisplayName { get; }

		public Erelia.Core.Creature.Species Species { get; }

		public Erelia.Core.Creature.Form Form { get; }

		public Erelia.Core.Creature.Stats Stats { get; }

		public Sprite Icon =>
			Form != null && Form.Icon != null
				? Form.Icon
				: Species != null
					? Species.Icon
					: null;

		public GameObject UnitPrefab =>
			Form != null && Form.UnitPrefab != null
				? Form.UnitPrefab
				: Species != null
					? Species.UnitPrefab
					: null;

		public IReadOnlyList<Erelia.Battle.Attack> EquippedActions => equippedActions;
	}
}



