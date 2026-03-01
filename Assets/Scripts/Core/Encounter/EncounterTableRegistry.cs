using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Encounter
{
	public static class EncounterTableRegistry
	{
		private const string DefaultResourcePath = "Encounter/EncounterRegistry";

		private static readonly Dictionary<int, Erelia.Core.Encounter.EncounterTable> idToTable =
			new Dictionary<int, Erelia.Core.Encounter.EncounterTable>();

		private static readonly Dictionary<Erelia.Core.Encounter.EncounterTable, int> tableToId =
			new Dictionary<Erelia.Core.Encounter.EncounterTable, int>();

		private static bool isLoaded;

		public static void Clear()
		{
			idToTable.Clear();
			tableToId.Clear();
			isLoaded = false;
		}

		public static void LoadFromResources(string resourcePath)
		{
			string path = string.IsNullOrEmpty(resourcePath) ? DefaultResourcePath : resourcePath;
			string json = Erelia.Core.Utils.PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				Debug.LogWarning($"[EncounterTableRegistry] Registry JSON not found at '{path}'.");
				Clear();
				isLoaded = true;
				return;
			}

			LoadFromJson(json);
		}

		public static void LoadFromJson(string json)
		{
			Clear();

			if (string.IsNullOrEmpty(json))
			{
				isLoaded = true;
				return;
			}

			RegistryData data = JsonUtility.FromJson<RegistryData>(json);
			if (data == null || data.Encounters == null)
			{
				Debug.LogWarning("[EncounterTableRegistry] Failed to parse registry JSON.");
				isLoaded = true;
				return;
			}

			for (int i = 0; i < data.Encounters.Count; i++)
			{
				Entry entry = data.Encounters[i];
				if (entry == null || string.IsNullOrEmpty(entry.Path))
				{
					continue;
				}

				if (idToTable.ContainsKey(entry.Id))
				{
					Debug.LogWarning($"[EncounterTableRegistry] Duplicate encounter id {entry.Id}.");
					continue;
				}

				Erelia.Core.Encounter.EncounterTable table = Erelia.Core.Encounter.EncounterTable.LoadFromPath(entry.Path);
				if (table == null)
				{
					Debug.LogWarning($"[EncounterTableRegistry] Encounter JSON not found at '{entry.Path}'.");
					continue;
				}

				idToTable.Add(entry.Id, table);
				tableToId.Add(table, entry.Id);
			}

			isLoaded = true;
		}

		public static bool TryGetId(Erelia.Core.Encounter.EncounterTable table, out int id)
		{
			EnsureLoaded();
			return tableToId.TryGetValue(table, out id);
		}

		public static bool TryGetTable(int id, out Erelia.Core.Encounter.EncounterTable table)
		{
			EnsureLoaded();
			return idToTable.TryGetValue(id, out table);
		}

		private static void EnsureLoaded()
		{
			if (isLoaded)
			{
				return;
			}

			LoadFromResources(DefaultResourcePath);
		}

		[System.Serializable]
		private sealed class RegistryData
		{
			public List<Entry> Encounters = new List<Entry>();
		}

		[System.Serializable]
		private sealed class Entry
		{
			public int Id;
			public string Path;
		}
	}
}
