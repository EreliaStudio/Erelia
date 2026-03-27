using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	[Serializable]
	public sealed class CreatureInstance : ISerializationCallbackReceiver
	{
		public const int MaxAttackCount = 8;

		[SerializeField] private string unitId;

		[SerializeField] private int speciesId = Erelia.Core.Creature.SpeciesRegistry.EmptySpeciesId;

		[SerializeField] private string nickname;

		[SerializeField] private string currentFormId;

		[SerializeField] private Erelia.Core.Creature.Stats stats = new Erelia.Core.Creature.Stats();

		[SerializeField] private Erelia.Core.Creature.FeatProgress featProgress =
			new Erelia.Core.Creature.FeatProgress();

		[SerializeField] private int[] attackIds = CreateAttackIdSlots();

		[SerializeField] private List<int> unlockedAttackIds = new List<int>();

		[NonSerialized] private Erelia.Battle.Attack[] attacks;

		[NonSerialized] private List<Erelia.Battle.Attack> unlockedAttacks;

		public string UnitId => EnsureUnitId();

		public int SpeciesId => speciesId;

		public string Nickname => nickname;

		public string CurrentFormId
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(currentFormId))
				{
					return currentFormId;
				}

				Erelia.Core.Creature.Form form = CurrentForm;
				return form != null ? form.IdentificationName : string.Empty;
			}
		}

		public Erelia.Core.Creature.Stats Stats => stats ??= new Erelia.Core.Creature.Stats();

		public Erelia.Core.Creature.Stats PersistentStats => Stats;

		public Erelia.Core.Creature.FeatProgress FeatProgress =>
			featProgress ??= new Erelia.Core.Creature.FeatProgress();

		public Erelia.Battle.Attack[] Attacks
		{
			get
			{
				EnsureEquippedAttacks();
				EnsureUnlockedActionsFromEquippedActions();
				return attacks;
			}
		}

		public IReadOnlyList<Erelia.Battle.Attack> UnlockedActions
		{
			get
			{
				EnsureUnlockedAttacks();
				return unlockedAttacks;
			}
		}

		public IReadOnlyList<Erelia.Battle.Attack> AvailableActions => BuildAvailableActions();

		public bool IsEmpty => speciesId < 0;

		public string DisplayName
		{
			get
			{
				if (!string.IsNullOrEmpty(nickname))
				{
					return nickname;
				}

				if (TryGetSpecies(out Erelia.Core.Creature.Species species) && species != null)
				{
					return species.DisplayName;
				}

				return string.Empty;
			}
		}

		public Erelia.Core.Creature.Form CurrentForm
		{
			get
			{
				TryGetCurrentForm(out Erelia.Core.Creature.Form form);
				return form;
			}
		}

		public Sprite Icon
		{
			get
			{
				Erelia.Core.Creature.Form form = CurrentForm;
				if (form != null && form.Icon != null)
				{
					return form.Icon;
				}

				return TryGetSpecies(out Erelia.Core.Creature.Species species)
					? species.Icon
					: null;
			}
		}

		public GameObject UnitPrefab
		{
			get
			{
				Erelia.Core.Creature.Form form = CurrentForm;
				if (form != null && form.UnitPrefab != null)
				{
					return form.UnitPrefab;
				}

				return TryGetSpecies(out Erelia.Core.Creature.Species species)
					? species.UnitPrefab
					: null;
			}
		}

		public Erelia.Core.Creature.Stats ResolvedStats =>
			Erelia.Core.Creature.Stats.Sum(
				ResolveSpeciesStats(),
				ResolveFormStats(),
				Stats);

		public CreatureInstance()
		{
		}

		public CreatureInstance(
			int speciesId,
			string nickname,
			Erelia.Core.Creature.Stats stats,
			params Erelia.Battle.Attack[] attacks)
		{
			unitId = Guid.NewGuid().ToString("N");
			this.speciesId = speciesId;
			this.nickname = nickname;
			this.stats = stats ?? new Erelia.Core.Creature.Stats();
			featProgress = new Erelia.Core.Creature.FeatProgress();
			SetAttacks(attacks);
		}

		public bool TryGetSpecies(out Erelia.Core.Creature.Species species)
		{
			species = null;
			if (IsEmpty)
			{
				return false;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			return registry != null &&
				registry.TryGet(speciesId, out species) &&
				species != null;
		}

		public bool TryGetCurrentForm(out Erelia.Core.Creature.Form form)
		{
			form = null;
			if (!TryGetSpecies(out Erelia.Core.Creature.Species species) || species == null)
			{
				return false;
			}

			form = species.ResolveForm(currentFormId);
			return form != null;
		}

		public Erelia.Core.Creature.Instance.CreatureProjection CreateProjection()
		{
			EnsureEquippedAttacks();
			EnsureUnlockedActionsFromEquippedActions();

			TryGetSpecies(out Erelia.Core.Creature.Species species);
			TryGetCurrentForm(out Erelia.Core.Creature.Form form);

			return new Erelia.Core.Creature.Instance.CreatureProjection(
				UnitId,
				DisplayName,
				species,
				form,
				ResolvedStats,
				CreateBattleAttackCopy(form));
		}

		public void SetSpeciesId(int id)
		{
			speciesId = id;
		}

		public void SetCurrentFormId(string formId)
		{
			currentFormId = string.IsNullOrWhiteSpace(formId)
				? string.Empty
				: formId.Trim();
		}

		public void SetAttack(int index, Erelia.Battle.Attack attack)
		{
			if (index < 0 || index >= MaxAttackCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			EnsureEquippedAttacks();
			if (attack != null)
			{
				RegisterUnlockedAttack(attack);
			}

			attacks[index] = attack;
		}

		public void SetAttacks(params Erelia.Battle.Attack[] values)
		{
			Erelia.Battle.Attack[] normalized = CreateAttackSlots();
			if (values != null)
			{
				Array.Copy(values, normalized, Mathf.Min(values.Length, MaxAttackCount));
			}

			attacks = normalized;
			unlockedAttacks = unlockedAttacks ?? new List<Erelia.Battle.Attack>();
			if (values != null)
			{
				for (int i = 0; i < values.Length; i++)
				{
					RegisterUnlockedAttack(values[i]);
				}
			}
		}

		public bool TryEquipAttack(int index, Erelia.Battle.Attack attack)
		{
			if (index < 0 || index >= MaxAttackCount)
			{
				return false;
			}

			if (attack != null && !IsActionAvailable(attack))
			{
				return false;
			}

			SetAttack(index, attack);
			return true;
		}

		public void ApplyStatBonus(Erelia.Core.Creature.Stats bonus)
		{
			if (bonus == null)
			{
				return;
			}

			stats = Erelia.Core.Creature.Stats.Combine(Stats, bonus);
		}

		public bool TryUnlockAttack(Erelia.Battle.Attack attack)
		{
			if (attack == null)
			{
				return false;
			}

			bool alreadyAvailable = IsActionAvailable(attack);
			bool added = RegisterUnlockedAttack(attack);
			if (!IsAttackEquipped(attack))
			{
				EquipIntoFirstAvailableSlot(attack);
			}

			return alreadyAvailable || added;
		}

		public void OnBeforeSerialize()
		{
			EnsureUnitId();
			stats ??= new Erelia.Core.Creature.Stats();
			featProgress ??= new Erelia.Core.Creature.FeatProgress();
			featProgress.Normalize();
			Erelia.Core.Creature.FeatProgression.EnsureInitialized(this);

			EnsureUnlockedAttacks();
			EnsureEquippedAttacks();
			NormalizeAttackIds();
			NormalizeUnlockedAttackIds();
			SyncAttackIdsFromDefinitions();
			SyncUnlockedAttackIdsFromDefinitions();
		}

		public void OnAfterDeserialize()
		{
			stats ??= new Erelia.Core.Creature.Stats();
			featProgress ??= new Erelia.Core.Creature.FeatProgress();
			featProgress.Normalize();
			NormalizeAttackIds();
			NormalizeUnlockedAttackIds();

			attacks = null;
			unlockedAttacks = null;

			EnsureUnlockedAttacks();
			EnsureEquippedAttacks();
			EnsureUnlockedActionsFromEquippedActions();
			Erelia.Core.Creature.FeatProgression.EnsureInitialized(this);
		}

		private string EnsureUnitId()
		{
			if (!string.IsNullOrWhiteSpace(unitId))
			{
				return unitId;
			}

			unitId = Guid.NewGuid().ToString("N");
			return unitId;
		}

		private Erelia.Core.Creature.Stats ResolveSpeciesStats()
		{
			return TryGetSpecies(out Erelia.Core.Creature.Species species)
				? species.BaseStats
				: new Erelia.Core.Creature.Stats();
		}

		private Erelia.Core.Creature.Stats ResolveFormStats()
		{
			return TryGetCurrentForm(out Erelia.Core.Creature.Form form)
				? form.StatModifier
				: new Erelia.Core.Creature.Stats();
		}

		private IReadOnlyList<Erelia.Battle.Attack> BuildAvailableActions()
		{
			EnsureUnlockedAttacks();
			EnsureEquippedAttacks();

			var availableActions = new List<Erelia.Battle.Attack>(MaxAttackCount);
			TryGetSpecies(out Erelia.Core.Creature.Species species);
			Erelia.Core.Creature.Form form = CurrentForm;

			if (species != null)
			{
				AppendUniqueActions(availableActions, species.DefaultActions, form);
			}

			if (form != null)
			{
				AppendUniqueActions(availableActions, form.GrantedActions, form);
			}

			AppendUniqueActions(availableActions, unlockedAttacks, form);
			AppendUniqueActions(availableActions, attacks, form);
			return availableActions;
		}

		private void AppendUniqueActions(
			List<Erelia.Battle.Attack> target,
			IReadOnlyList<Erelia.Battle.Attack> source,
			Erelia.Core.Creature.Form form)
		{
			if (target == null || source == null)
			{
				return;
			}

			for (int i = 0; i < source.Count; i++)
			{
				Erelia.Battle.Attack attack = source[i];
				if (attack == null || !attack.IsAllowedFor(form))
				{
					continue;
				}

				if (!ContainsAttack(target, attack))
				{
					target.Add(attack);
				}
			}
		}

		private bool IsActionAvailable(Erelia.Battle.Attack attack)
		{
			if (attack == null)
			{
				return false;
			}

			IReadOnlyList<Erelia.Battle.Attack> availableActions = BuildAvailableActions();
			return ContainsAttack(availableActions, attack);
		}

		private bool IsAttackEquipped(Erelia.Battle.Attack attack)
		{
			if (attack == null)
			{
				return false;
			}

			Erelia.Battle.Attack[] equippedAttacks = Attacks;
			for (int i = 0; i < equippedAttacks.Length; i++)
			{
				if (equippedAttacks[i] == attack)
				{
					return true;
				}
			}

			return false;
		}

		private void EquipIntoFirstAvailableSlot(Erelia.Battle.Attack attack)
		{
			if (attack == null)
			{
				return;
			}

			Erelia.Battle.Attack[] equippedAttacks = Attacks;
			for (int i = 0; i < equippedAttacks.Length; i++)
			{
				if (equippedAttacks[i] != null)
				{
					continue;
				}

				equippedAttacks[i] = attack;
				return;
			}
		}

		private bool RegisterUnlockedAttack(Erelia.Battle.Attack attack)
		{
			if (attack == null)
			{
				return false;
			}

			unlockedAttacks = unlockedAttacks ?? new List<Erelia.Battle.Attack>();
			if (ContainsAttack(unlockedAttacks, attack))
			{
				return false;
			}

			unlockedAttacks.Add(attack);
			return true;
		}

		private void EnsureEquippedAttacks()
		{
			if (attacks == null || NeedsEquippedAttackSyncFromIds())
			{
				SyncDefinitionsFromAttackIds();
			}

			NormalizeAttacks();
			EnsureDefaultLoadout();
		}

		private void EnsureUnlockedAttacks()
		{
			if (unlockedAttacks == null || NeedsUnlockedAttackSyncFromIds())
			{
				SyncUnlockedDefinitionsFromIds();
			}

			unlockedAttacks ??= new List<Erelia.Battle.Attack>();
		}

		private void EnsureUnlockedActionsFromEquippedActions()
		{
			Erelia.Battle.Attack[] equippedAttacks = attacks;
			if (equippedAttacks == null)
			{
				return;
			}

			for (int i = 0; i < equippedAttacks.Length; i++)
			{
				RegisterUnlockedAttack(equippedAttacks[i]);
			}
		}

		private void EnsureDefaultLoadout()
		{
			if (HasAnyEquippedAttack())
			{
				return;
			}

			IReadOnlyList<Erelia.Battle.Attack> availableActions = BuildAvailableActionsWithoutEquippedFallback();
			for (int i = 0; i < attacks.Length && i < availableActions.Count; i++)
			{
				attacks[i] = availableActions[i];
			}
		}

		private IReadOnlyList<Erelia.Battle.Attack> BuildAvailableActionsWithoutEquippedFallback()
		{
			EnsureUnlockedAttacks();
			var availableActions = new List<Erelia.Battle.Attack>(MaxAttackCount);
			TryGetSpecies(out Erelia.Core.Creature.Species species);
			Erelia.Core.Creature.Form form = CurrentForm;

			if (species != null)
			{
				AppendUniqueActions(availableActions, species.DefaultActions, form);
			}

			if (form != null)
			{
				AppendUniqueActions(availableActions, form.GrantedActions, form);
			}

			AppendUniqueActions(availableActions, unlockedAttacks, form);
			return availableActions;
		}

		private bool HasAnyEquippedAttack()
		{
			if (attacks == null)
			{
				return false;
			}

			for (int i = 0; i < attacks.Length; i++)
			{
				if (attacks[i] != null)
				{
					return true;
				}
			}

			return false;
		}

		private void NormalizeAttacks()
		{
			if (attacks != null && attacks.Length == MaxAttackCount)
			{
				return;
			}

			Erelia.Battle.Attack[] normalized = CreateAttackSlots();
			if (attacks != null)
			{
				Array.Copy(attacks, normalized, Mathf.Min(attacks.Length, MaxAttackCount));
			}

			attacks = normalized;
		}

		private void NormalizeAttackIds()
		{
			if (attackIds == null || attackIds.Length != MaxAttackCount)
			{
				int[] normalized = CreateAttackIdSlots();
				if (attackIds != null)
				{
					Array.Copy(attackIds, normalized, Mathf.Min(attackIds.Length, MaxAttackCount));
				}

				attackIds = normalized;
			}

			for (int i = 0; i < attackIds.Length; i++)
			{
				if (attackIds[i] < 0)
				{
					attackIds[i] = Erelia.Battle.AttackRegistry.EmptyAttackId;
				}
			}
		}

		private void NormalizeUnlockedAttackIds()
		{
			unlockedAttackIds ??= new List<int>();

			var uniqueIds = new HashSet<int>();
			for (int i = unlockedAttackIds.Count - 1; i >= 0; i--)
			{
				int attackId = unlockedAttackIds[i];
				if (attackId < 0 ||
					attackId == Erelia.Battle.AttackRegistry.EmptyAttackId ||
					!uniqueIds.Add(attackId))
				{
					unlockedAttackIds.RemoveAt(i);
				}
			}
		}

		private void SyncAttackIdsFromDefinitions()
		{
			Erelia.Battle.AttackRegistry registry = Erelia.Battle.AttackRegistry.Instance;
			if (registry == null)
			{
				return;
			}

			for (int i = 0; i < MaxAttackCount; i++)
			{
				Erelia.Battle.Attack attack = attacks[i];
				if (attack == null)
				{
					attackIds[i] = Erelia.Battle.AttackRegistry.EmptyAttackId;
					continue;
				}

				if (registry != null && registry.TryGetId(attack, out int attackId))
				{
					attackIds[i] = attackId;
					continue;
				}

				attackIds[i] = Erelia.Battle.AttackRegistry.EmptyAttackId;
			}
		}

		private void SyncUnlockedAttackIdsFromDefinitions()
		{
			Erelia.Battle.AttackRegistry registry = Erelia.Battle.AttackRegistry.Instance;
			if (registry == null || unlockedAttacks == null)
			{
				return;
			}

			unlockedAttackIds.Clear();
			for (int i = 0; i < unlockedAttacks.Count; i++)
			{
				Erelia.Battle.Attack attack = unlockedAttacks[i];
				if (attack == null || !registry.TryGetId(attack, out int attackId))
				{
					continue;
				}

				if (!unlockedAttackIds.Contains(attackId))
				{
					unlockedAttackIds.Add(attackId);
				}
			}
		}

		private void SyncDefinitionsFromAttackIds()
		{
			NormalizeAttacks();
			Erelia.Battle.AttackRegistry registry = Erelia.Battle.AttackRegistry.Instance;
			if (registry == null)
			{
				return;
			}

			for (int i = 0; i < MaxAttackCount; i++)
			{
				int attackId = attackIds[i];
				if (attackId == Erelia.Battle.AttackRegistry.EmptyAttackId)
				{
					attacks[i] = null;
					continue;
				}

				if (registry != null && registry.TryGet(attackId, out Erelia.Battle.Attack attack))
				{
					attacks[i] = attack;
					continue;
				}

				attacks[i] = null;
				attackIds[i] = Erelia.Battle.AttackRegistry.EmptyAttackId;
			}
		}

		private void SyncUnlockedDefinitionsFromIds()
		{
			NormalizeUnlockedAttackIds();
			unlockedAttacks = new List<Erelia.Battle.Attack>(unlockedAttackIds.Count);

			Erelia.Battle.AttackRegistry registry = Erelia.Battle.AttackRegistry.Instance;
			if (registry == null)
			{
				return;
			}

			for (int i = 0; i < unlockedAttackIds.Count; i++)
			{
				int attackId = unlockedAttackIds[i];
				if (registry.TryGet(attackId, out Erelia.Battle.Attack attack) &&
					attack != null &&
					!ContainsAttack(unlockedAttacks, attack))
				{
					unlockedAttacks.Add(attack);
				}
			}
		}

		private Erelia.Battle.Attack[] CreateBattleAttackCopy(Erelia.Core.Creature.Form form)
		{
			Erelia.Battle.Attack[] battleAttacks = CreateAttackSlots();
			Erelia.Battle.Attack[] equippedAttacks = Attacks;
			for (int i = 0; i < equippedAttacks.Length; i++)
			{
				Erelia.Battle.Attack attack = equippedAttacks[i];
				if (attack == null || !attack.IsAllowedFor(form))
				{
					continue;
				}

				battleAttacks[i] = attack;
			}

			return battleAttacks;
		}

		private static bool ContainsAttack(
			IReadOnlyList<Erelia.Battle.Attack> attacks,
			Erelia.Battle.Attack attack)
		{
			if (attacks == null || attack == null)
			{
				return false;
			}

			for (int i = 0; i < attacks.Count; i++)
			{
				if (attacks[i] == attack)
				{
					return true;
				}
			}

			return false;
		}

		private bool NeedsEquippedAttackSyncFromIds()
		{
			if (attacks == null || HasAnyEquippedAttack())
			{
				return attacks == null;
			}

			for (int i = 0; i < attackIds.Length; i++)
			{
				if (attackIds[i] != Erelia.Battle.AttackRegistry.EmptyAttackId)
				{
					return true;
				}
			}

			return false;
		}

		private bool NeedsUnlockedAttackSyncFromIds()
		{
			return unlockedAttackIds != null &&
				unlockedAttackIds.Count > 0 &&
				(unlockedAttacks == null || unlockedAttacks.Count == 0);
		}

		private static Erelia.Battle.Attack[] CreateAttackSlots()
		{
			return new Erelia.Battle.Attack[MaxAttackCount];
		}

		private static int[] CreateAttackIdSlots()
		{
			int[] ids = new int[MaxAttackCount];
			for (int i = 0; i < ids.Length; i++)
			{
				ids[i] = Erelia.Battle.AttackRegistry.EmptyAttackId;
			}

			return ids;
		}
	}
}



