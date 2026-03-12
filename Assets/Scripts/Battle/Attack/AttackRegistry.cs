using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Attack
{
	/// <summary>
	/// Singleton registry mapping integer ids to attack assets.
	/// </summary>
	[CreateAssetMenu(menuName = "Attack/Registry", fileName = "AttackRegistry")]
	public sealed class AttackRegistry : Erelia.Core.SingletonRegistry<Erelia.Battle.Attack.AttackRegistry>
	{
		public const int EmptyAttackId = -1;
		public const int NoAttackId = EmptyAttackId;

		[Serializable]
		public struct Entry
		{
			public int Id;
			public Erelia.Battle.Attack.Definition Attack;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		[NonSerialized] private readonly Dictionary<int, Erelia.Battle.Attack.Definition> byId =
			new Dictionary<int, Erelia.Battle.Attack.Definition>();
		[NonSerialized] private readonly Dictionary<Erelia.Battle.Attack.Definition, int> byAttack =
			new Dictionary<Erelia.Battle.Attack.Definition, int>();

		protected override string ResourcePath => "Attack/AttackRegistry";

		public IReadOnlyList<Entry> Entries => entries;

		protected override void Rebuild()
		{
			byId.Clear();
			byAttack.Clear();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Attack == null)
				{
					continue;
				}

				if (byId.ContainsKey(entry.Id))
				{
					Debug.LogWarning(
						$"[Erelia.Battle.Attack.AttackRegistry] Duplicate id {entry.Id} for '{entry.Attack.name}'. Keeping the first occurrence.");
					continue;
				}

				byId.Add(entry.Id, entry.Attack);
				if (!byAttack.ContainsKey(entry.Attack))
				{
					byAttack.Add(entry.Attack, entry.Id);
				}
			}
		}

		public bool TryGet(int id, out Erelia.Battle.Attack.Definition attack)
		{
			return byId.TryGetValue(id, out attack);
		}

		public bool TryGetId(Erelia.Battle.Attack.Definition attack, out int id)
		{
			return byAttack.TryGetValue(attack, out id);
		}
	}
}
