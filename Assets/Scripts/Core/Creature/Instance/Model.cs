using System;
using UnityEngine;

namespace Erelia.Core.Creature.Instance
{
	/// <summary>
	/// Serializable data model representing a single creature instance.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A creature instance references its species through <see cref="SpeciesId"/> (resolved via
	/// <see cref="Erelia.Core.Creature.SpeciesRegistry"/>) and optionally stores a nickname.
	/// </para>
	/// <para>
	/// JSON format (Unity <see cref="JsonUtility"/>):
	/// </para>
	/// <code>
	/// {
	///   "speciesId": 12,
	///   "nickname": "Kitsu",
	///   "attackIds": [0, 4, -1, -1, -1, -1, -1, -1]
	/// }
	/// </code>
	/// <para>
	/// Notes:
	/// <list type="bullet">
	/// <item><description>Property names match the serialized field names (<c>speciesId</c>, <c>nickname</c>, <c>attackIds</c>).</description></item>
	/// <item><description>Private fields are serialized because they are marked with <c>[SerializeField]</c>.</description></item>
	/// <item><description>Attack ids are resolved through <see cref="Erelia.Battle.Attack.AttackRegistry"/> after deserialization.</description></item>
	/// <item><description>Serialization is handled externally via <see cref="JsonUtility"/>.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	[System.Serializable]
	public sealed class Model : ISerializationCallbackReceiver
	{
		public const int MaxAttackCount = 8;

		/// <summary>
		/// Species registry id associated with this creature instance.
		/// </summary>
		[SerializeField] private int speciesId = Erelia.Core.Creature.SpeciesRegistry.EmptySpeciesId;

		/// <summary>
		/// Optional nickname assigned to this creature instance.
		/// </summary>
		[SerializeField] private string nickname;

		/// <summary>
		/// Additive stats gained by this specific creature instance.
		/// </summary>
		[SerializeField] private Erelia.Core.Creature.Stats stats = new Erelia.Core.Creature.Stats();

		/// <summary>
		/// Persistent feat progression state for this creature instance.
		/// </summary>
		[SerializeField] private Erelia.Core.Creature.FeatProgress featProgress =
			new Erelia.Core.Creature.FeatProgress();

		/// <summary>
		/// Serialized attack ids used for save/load.
		/// </summary>
		[SerializeField] private int[] attackIds = CreateAttackIdSlots();

		/// <summary>
		/// Runtime-resolved attack definitions.
		/// </summary>
		[NonSerialized] private Erelia.Battle.Attack.Definition[] attacks = CreateAttackSlots();

		/// <summary>
		/// Gets the species registry id.
		/// </summary>
		public int SpeciesId => speciesId;

		/// <summary>
		/// Gets the nickname.
		/// </summary>
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

		/// <summary>
		/// Creates an empty creature instance model.
		/// </summary>
		public Model()
		{
			// Default constructor required for serialization.
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

		/// <summary>
		/// Sets the species registry id.
		/// </summary>
		/// <param name="id">New species id.</param>
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
