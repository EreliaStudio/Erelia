using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	[CreateAssetMenu(menuName = "Attack/Registry", fileName = "AttackRegistry")]
	public sealed class AttackRegistry : Erelia.Core.SingletonRegistry<Erelia.Battle.AttackRegistry>
	{
		public const int EmptyAttackId = -1;
		public const int NoAttackId = EmptyAttackId;

		[Serializable]
		public struct Entry
		{
			public int Id;
			public Erelia.Battle.Attack Attack;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		[NonSerialized] private readonly Dictionary<int, Erelia.Battle.Attack> byId =
			new Dictionary<int, Erelia.Battle.Attack>();
		[NonSerialized] private readonly Dictionary<Erelia.Battle.Attack, int> byAttack =
			new Dictionary<Erelia.Battle.Attack, int>();

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
						$"[Erelia.Battle.AttackRegistry] Duplicate id {entry.Id} for '{entry.Attack.name}'. Keeping the first occurrence.");
					continue;
				}

				byId.Add(entry.Id, entry.Attack);
				if (!byAttack.ContainsKey(entry.Attack))
				{
					byAttack.Add(entry.Attack, entry.Id);
				}
			}
		}

		public bool TryGet(int id, out Erelia.Battle.Attack attack)
		{
			return byId.TryGetValue(id, out attack);
		}

		public bool TryGetId(Erelia.Battle.Attack attack, out int id)
		{
			return byAttack.TryGetValue(attack, out id);
		}
	}
}


