using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	public readonly struct BattleResultCreatureData
	{
		private static readonly IReadOnlyList<Erelia.Battle.BattleResultEntryData> EmptyEntries =
			Array.Empty<Erelia.Battle.BattleResultEntryData>();

		public BattleResultCreatureData(
			int slotIndex,
			string creatureName,
			Sprite creatureIcon,
			bool hasCreature,
			IReadOnlyList<Erelia.Battle.BattleResultEntryData> entries)
		{
			SlotIndex = Mathf.Max(0, slotIndex);
			CreatureName = creatureName ?? string.Empty;
			CreatureIcon = creatureIcon;
			HasCreature = hasCreature;
			Entries = entries ?? EmptyEntries;
		}

		public int SlotIndex { get; }
		public string CreatureName { get; }
		public Sprite CreatureIcon { get; }
		public bool HasCreature { get; }
		public IReadOnlyList<Erelia.Battle.BattleResultEntryData> Entries { get; }
	}
}
