using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Encounter
{
	/// <summary>
	/// Registry that maps encounter ids to encounter tables and vice versa.
	/// </summary>
	/// <remarks>
	/// Loads a JSON registry file that lists encounter ids and table paths.
	/// Each table path is resolved via <see cref="Erelia.Core.Utils.PathUtils"/>.
	/// </remarks>
	public static class EncounterTableRegistry
	{
		/// <summary>
		/// Default Resources path (without extension) to the encounter registry JSON.
		/// </summary>
		private const string DefaultResourcePath = "Encounter/EncounterRegistry";

		/// <summary>
		/// Runtime lookup from id to encounter table.
		/// </summary>
		private static readonly Dictionary<int, Erelia.Core.Encounter.EncounterTable> idToTable =
			new Dictionary<int, Erelia.Core.Encounter.EncounterTable>();

		/// <summary>
		/// Runtime lookup from encounter table to id.
		/// </summary>
		private static readonly Dictionary<Erelia.Core.Encounter.EncounterTable, int> tableToId =
			new Dictionary<Erelia.Core.Encounter.EncounterTable, int>();

		/// <summary>
		/// True if the registry has been loaded (even if empty).
		/// </summary>
		private static bool isLoaded;

		/// <summary>
		/// Clears all registry mappings and resets the loaded state.
		/// </summary>
		public static void Clear()
		{
			// Reset all mappings and loaded state.
			idToTable.Clear();
			tableToId.Clear();
			isLoaded = false;
		}

		/// <summary>
		/// Loads the registry from a Resources path (or default if null/empty).
		/// </summary>
		/// <param name="resourcePath">Resources path without extension.</param>
		public static void LoadFromResources(string resourcePath)
		{
			// Resolve the registry resource path (fallback to default).
			string path = string.IsNullOrEmpty(resourcePath) ? DefaultResourcePath : resourcePath;
			string json = Erelia.Core.Utils.PathUtils.ReadTextFromPath(path);
			if (string.IsNullOrEmpty(json))
			{
				// Missing registry: clear state and mark as loaded to avoid repeated attempts.
				Debug.LogWarning($"[EncounterTableRegistry] Registry JSON not found at '{path}'.");
				Clear();
				isLoaded = true;
				return;
			}

			// Delegate to JSON loader.
			LoadFromJson(json);
		}

		/// <summary>
		/// Loads the registry from a JSON string payload.
		/// </summary>
		/// <param name="json">Registry JSON content.</param>
		public static void LoadFromJson(string json)
		{
			// Always reset before loading.
			Clear();

			if (string.IsNullOrEmpty(json))
			{
				// Empty payload is treated as a valid "empty registry".
				isLoaded = true;
				return;
			}

			// Parse the registry JSON.
			RegistryData data = JsonUtility.FromJson<RegistryData>(json);
			if (data == null || data.Encounters == null)
			{
				Debug.LogWarning("[EncounterTableRegistry] Failed to parse registry JSON.");
				isLoaded = true;
				return;
			}

			// Load each encounter entry.
			for (int i = 0; i < data.Encounters.Count; i++)
			{
				Entry entry = data.Encounters[i];
				if (entry == null || string.IsNullOrEmpty(entry.Path))
				{
					continue;
				}

				if (idToTable.ContainsKey(entry.Id))
				{
					// Duplicate ids are ignored to keep the mapping deterministic.
					Debug.LogWarning($"[EncounterTableRegistry] Duplicate encounter id {entry.Id}.");
					continue;
				}

				// Resolve and load the encounter table file.
				if (!Erelia.Core.Utils.JsonIO.TryLoad(entry.Path, out Erelia.Core.Encounter.EncounterTable table))
				{
					Debug.LogWarning($"[EncounterTableRegistry] Encounter JSON not found or invalid at '{entry.Path}'.");
					continue;
				}

				// Register both directions.
				idToTable.Add(entry.Id, table);
				tableToId.Add(table, entry.Id);
			}

			// Mark registry as loaded so lazy access won't reload.
			isLoaded = true;
		}

		/// <summary>
		/// Attempts to retrieve the id associated with a given encounter table.
		/// </summary>
		/// <param name="table">Encounter table instance.</param>
		/// <param name="id">Resolved id if found.</param>
		/// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
		public static bool TryGetId(Erelia.Core.Encounter.EncounterTable table, out int id)
		{
			// Ensure registry is loaded before accessing.
			EnsureLoaded();
			return tableToId.TryGetValue(table, out id);
		}

		/// <summary>
		/// Attempts to retrieve the encounter table for a given id.
		/// </summary>
		/// <param name="id">Encounter id.</param>
		/// <param name="table">Resolved table if found.</param>
		/// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
		public static bool TryGetTable(int id, out Erelia.Core.Encounter.EncounterTable table)
		{
			// Ensure registry is loaded before accessing.
			EnsureLoaded();
			return idToTable.TryGetValue(id, out table);
		}

		/// <summary>
		/// Ensures the registry has been loaded before access.
		/// </summary>
		private static void EnsureLoaded()
		{
			if (isLoaded)
			{
				return;
			}

			// Lazy-load from the default registry resource.
			LoadFromResources(DefaultResourcePath);
		}

		[System.Serializable]
		private sealed class RegistryData
		{
			/// <summary>
			/// List of encounter entries in the registry.
			/// </summary>
			public List<Entry> Encounters = new List<Entry>();
		}

		[System.Serializable]
		private sealed class Entry
		{
			/// <summary>
			/// Encounter id.
			/// </summary>
			public int Id;

			/// <summary>
			/// Path to the encounter table JSON.
			/// </summary>
			public string Path;
		}
	}
}
