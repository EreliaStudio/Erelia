using System.Collections.Generic;

namespace Erelia
{
	public static class EncounterTableRegistry
	{
		private static readonly Dictionary<int, Erelia.Encounter.EncounterTable> idToTable =
			new Dictionary<int, Erelia.Encounter.EncounterTable>();

		private static readonly Dictionary<Erelia.Encounter.EncounterTable, int> tableToId =
			new Dictionary<Erelia.Encounter.EncounterTable, int>();

		private static int nextId;

		public static void Clear()
		{
			idToTable.Clear();
			tableToId.Clear();
			nextId = 0;
		}

		public static int Register(Erelia.Encounter.EncounterTable table)
		{
			if (table == null)
			{
				return -1;
			}

			if (tableToId.TryGetValue(table, out int existing))
			{
				return existing;
			}

			int id = nextId++;
			tableToId.Add(table, id);
			idToTable.Add(id, table);
			return id;
		}

		public static bool TryGetId(Erelia.Encounter.EncounterTable table, out int id)
		{
			return tableToId.TryGetValue(table, out id);
		}

		public static bool TryGetTable(int id, out Erelia.Encounter.EncounterTable table)
		{
			return idToTable.TryGetValue(id, out table);
		}
	}
}
