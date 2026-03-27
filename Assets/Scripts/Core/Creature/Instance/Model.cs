using System;
using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	[System.Serializable]
	public sealed class Model : ISerializationCallbackReceiver
	{
		public const int MaxAttackCount = 8;

		[SerializeField] private int speciesId = Erelia.Core.Creature.SpeciesRegistry.EmptySpeciesId;

		[SerializeField] private string nickname;

		[SerializeField] private Erelia.Core.Creature.Stats stats = new Erelia.Core.Creature.Stats();

		[SerializeField] private Erelia.Core.Creature.FeatProgress featProgress =
			new Erelia.Core.Creature.FeatProgress();

		[SerializeField] private int[] attackIds = CreateAttackIdSlots();

		[NonSerialized] private Erelia.Battle.Attack.Definition[] attacks = CreateAttackSlots();

		public int SpeciesId => speciesId;

		public string Nickname => nickname;
		public Erelia.Core.Creature.Stats Stats => stats ??= new Erelia.Core.Creature.Stats();
		public Erelia.Core.Creature.FeatProgress FeatProgress => featProgress ??= new Erelia.Core.Creature.FeatProgress();
		public Erelia.Battle.Attack.Definition[] Attacks => attacks ??= CreateAttackSlots();
		public bool IsEmpty => speciesId < 0;

		public string DisplayName
		{
			get
			{
				if (!string.IsNullOrEmpty(nickname))
				{
					return nickname;
				}

				Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
				if (registry != null &&
					registry.TryGet(speciesId, out Erelia.Core.Creature.Species species) &&
					species != null)
				{
					return species.DisplayName;
				}

				return string.Empty;
			}
		}

		public Model()
		{
		}

		public Model(
			int speciesId,
			string nickname,
			Erelia.Core.Creature.Stats stats,
			params Erelia.Battle.Attack.Definition[] attacks)
		{
			this.speciesId = speciesId;
			this.nickname = nickname;
			this.stats = stats ?? new Erelia.Core.Creature.Stats();
			featProgress = new Erelia.Core.Creature.FeatProgress();
			SetAttacks(attacks);
		}

		public void SetSpeciesId(int id)
		{
			speciesId = id;
		}

		public void SetAttack(int index, Erelia.Battle.Attack.Definition attack)
		{
			if (index < 0 || index >= MaxAttackCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			Attacks[index] = attack;
		}

		public void SetAttacks(params Erelia.Battle.Attack.Definition[] values)
		{
			Erelia.Battle.Attack.Definition[] normalized = CreateAttackSlots();
			if (values != null)
			{
				Array.Copy(values, normalized, Mathf.Min(values.Length, MaxAttackCount));
			}

			attacks = normalized;
		}

		public void ApplyStatBonus(Erelia.Core.Creature.Stats bonus)
		{
			if (bonus == null)
			{
				return;
			}

			stats = Erelia.Core.Creature.Stats.Combine(Stats, bonus);
		}

		public bool TryUnlockAttack(Erelia.Battle.Attack.Definition attack)
		{
			if (attack == null)
			{
				return false;
			}

			Erelia.Battle.Attack.Definition[] availableAttacks = Attacks;
			for (int i = 0; i < availableAttacks.Length; i++)
			{
				if (availableAttacks[i] == attack)
				{
					return true;
				}
			}

			for (int i = 0; i < availableAttacks.Length; i++)
			{
				if (availableAttacks[i] != null)
				{
					continue;
				}

				availableAttacks[i] = attack;
				return true;
			}

			return false;
		}

		public void OnBeforeSerialize()
		{
			stats ??= new Erelia.Core.Creature.Stats();
			featProgress ??= new Erelia.Core.Creature.FeatProgress();
			featProgress.Normalize();
			Erelia.Core.Creature.FeatProgression.EnsureInitialized(this);
			NormalizeAttacks();
			NormalizeAttackIds();
			SyncAttackIdsFromDefinitions();
		}

		public void OnAfterDeserialize()
		{
			stats ??= new Erelia.Core.Creature.Stats();
			featProgress ??= new Erelia.Core.Creature.FeatProgress();
			featProgress.Normalize();
			Erelia.Core.Creature.FeatProgression.EnsureInitialized(this);
			NormalizeAttackIds();
			SyncDefinitionsFromAttackIds();
		}

		private void NormalizeAttacks()
		{
			if (attacks != null && attacks.Length == MaxAttackCount)
			{
				return;
			}

			Erelia.Battle.Attack.Definition[] normalized = CreateAttackSlots();
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
					attackIds[i] = Erelia.Battle.Attack.AttackRegistry.EmptyAttackId;
				}
			}
		}

		private void SyncAttackIdsFromDefinitions()
		{
			Erelia.Battle.Attack.AttackRegistry registry = Erelia.Battle.Attack.AttackRegistry.Instance;

			for (int i = 0; i < MaxAttackCount; i++)
			{
				Erelia.Battle.Attack.Definition attack = attacks[i];
				if (attack == null)
				{
					attackIds[i] = Erelia.Battle.Attack.AttackRegistry.EmptyAttackId;
					continue;
				}

				if (registry != null && registry.TryGetId(attack, out int attackId))
				{
					attackIds[i] = attackId;
					continue;
				}

				attackIds[i] = Erelia.Battle.Attack.AttackRegistry.EmptyAttackId;
			}
		}

		private void SyncDefinitionsFromAttackIds()
		{
			NormalizeAttacks();
			Erelia.Battle.Attack.AttackRegistry registry = Erelia.Battle.Attack.AttackRegistry.Instance;

			for (int i = 0; i < MaxAttackCount; i++)
			{
				int attackId = attackIds[i];
				if (attackId == Erelia.Battle.Attack.AttackRegistry.EmptyAttackId)
				{
					attacks[i] = null;
					continue;
				}

				if (registry != null && registry.TryGet(attackId, out Erelia.Battle.Attack.Definition attack))
				{
					attacks[i] = attack;
					continue;
				}

				attacks[i] = null;
				attackIds[i] = Erelia.Battle.Attack.AttackRegistry.EmptyAttackId;
			}
		}

		private static Erelia.Battle.Attack.Definition[] CreateAttackSlots()
		{
			return new Erelia.Battle.Attack.Definition[MaxAttackCount];
		}

		private static int[] CreateAttackIdSlots()
		{
			int[] ids = new int[MaxAttackCount];
			for (int i = 0; i < ids.Length; i++)
			{
				ids[i] = Erelia.Battle.Attack.AttackRegistry.EmptyAttackId;
			}

			return ids;
		}
	}
}
