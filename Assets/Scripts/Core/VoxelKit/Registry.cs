using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Registry of voxel definitions, stored as a Unity asset.
	/// </summary>
	/// <remarks>
	/// This registry provides a stable mapping from an integer voxel id to a <see cref="Definition"/>.
	/// <para>
	/// Authoring workflow:
	/// </para>
	/// <list type="bullet">
	///   <item><description>Edit <see cref="Entries"/> in the Inspector.</description></item>
	///   <item><description>On load / validation, the runtime dictionary is rebuilt from these entries.</description></item>
	/// </list>
	/// <para>
	/// Runtime workflow:
	/// </para>
	/// <list type="bullet">
	///   <item><description>Use <see cref="TryGet"/> / <see cref="Contains"/> to query by id.</description></item>
	///   <item><description>Optionally add entries at runtime using <see cref="Add"/> (does not modify the serialized list).</description></item>
	/// </list>
	/// </remarks>
	[CreateAssetMenu(menuName = "Voxel/Registry", fileName = "VoxelRegistry")]
	public sealed class Registry : ScriptableObject
	{
		/// <summary>
		/// One registry entry mapping an integer id to a voxel <see cref="Definition"/>.
		/// </summary>
		[Serializable]
		public struct Entry
		{
			/// <summary>
			/// The integer id used to reference the voxel definition (e.g., in chunk data).
			/// </summary>
			public int Id;

			/// <summary>
			/// The voxel definition associated with <see cref="Id"/>.
			/// </summary>
			public Erelia.Core.VoxelKit.Definition Definition;
		}

		/// <summary>
		/// Serialized entry list used for authoring in the Inspector.
		/// </summary>
		[SerializeField] private List<Entry> entries = new List<Entry>();

		/// <summary>
		/// Runtime lookup table built from <see cref="entries"/>.
		/// </summary>
		/// <remarks>
		/// Marked non-serialized because it is derived data and can be rebuilt at any time.
		/// </remarks>
		[NonSerialized] private Dictionary<int, Erelia.Core.VoxelKit.Definition> registeredDefinition;

		/// <summary>
		/// Unity callback invoked when the asset is loaded or enabled.
		/// </summary>
		private void OnEnable()
		{
			// Rebuild the runtime dictionary so lookups are ready immediately.
			RebuildFromEntries();
		}

		/// <summary>
		/// Unity Editor callback invoked when the asset is modified in the Inspector.
		/// </summary>
		private void OnValidate()
		{
			// Keep the runtime dictionary in sync with the serialized entry list.
			RebuildFromEntries();
		}

		/// <summary>
		/// Rebuilds the runtime dictionary from the serialized <see cref="Entries"/> list.
		/// </summary>
		/// <returns>The number of successfully registered definitions.</returns>
		/// <remarks>
		/// This method:
		/// <list type="bullet">
		///   <item><description>Creates a fresh dictionary.</description></item>
		///   <item><description>Skips null definitions.</description></item>
		///   <item><description>Detects duplicate ids and logs errors.</description></item>
		///   <item><description>Registers each valid mapping id -&gt; definition.</description></item>
		/// </list>
		/// </remarks>
		public int RebuildFromEntries()
		{
			// Always rebuild from scratch to reflect current serialized data.
			registeredDefinition = new Dictionary<int, Erelia.Core.VoxelKit.Definition>();

			int registered = 0;

			// Track ids already encountered in the entries list to report duplicates clearly.
			HashSet<int> seen = new HashSet<int>();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];

				// Skip empty entries.
				if (entry.Definition == null)
				{
					continue;
				}

				// Detect duplicate ids in the serialized list.
				if (!seen.Add(entry.Id))
				{
					Debug.LogError($"VoxelRegistry '{name}' has a duplicate id '{entry.Id}'.");
					continue;
				}

				// Defensive check: dictionary duplicates should not happen if 'seen' is used,
				// but keep this to guard against future code changes.
				if (registeredDefinition.ContainsKey(entry.Id))
				{
					Debug.LogError($"Duplicate voxel id '{entry.Id}' already registered. Definition '{entry.Definition.name}' skipped.");
					continue;
				}

				try
				{
					// Register the mapping.
					registeredDefinition.Add(entry.Id, entry.Definition);
					registered++;
				}
				catch (Exception ex)
				{
					// Catch unexpected failures (e.g., invalid key state) and continue processing.
					Debug.LogError($"Failed to register voxel definition '{entry.Definition.name}' (Id={entry.Id}): {ex.Message}");
				}
			}

			return registered;
		}

		/// <summary>
		/// Adds a voxel definition to the runtime registry.
		/// </summary>
		/// <param name="id">The integer id to register.</param>
		/// <param name="definition">The voxel definition to associate with the id.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="definition"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is already registered.</exception>
		/// <remarks>
		/// This does not modify the serialized <see cref="entries"/> list. It only affects the in-memory dictionary.
		/// </remarks>
		public void Add(int id, Erelia.Core.VoxelKit.Definition definition)
		{
			// Fail fast: registry entries must reference a real definition.
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			// Ensure the dictionary exists before writing to it.
			EnsureRegistry();

			// Prevent id collisions.
			if (registeredDefinition.ContainsKey(id))
			{
				throw new ArgumentException($"A voxel definition with id '{id}' is already registered.", nameof(id));
			}

			registeredDefinition.Add(id, definition);
		}

		/// <summary>
		/// Attempts to retrieve a voxel definition by id.
		/// </summary>
		/// <param name="id">The voxel id to look up.</param>
		/// <param name="definition">Output definition when found; otherwise null.</param>
		/// <returns><c>true</c> if the id is registered; otherwise <c>false</c>.</returns>
		public bool TryGet(int id, out Erelia.Core.VoxelKit.Definition definition)
		{
			// Ensure the dictionary exists before reading.
			EnsureRegistry();
			return registeredDefinition.TryGetValue(id, out definition);
		}

		/// <summary>
		/// Checks whether an id is registered.
		/// </summary>
		/// <param name="id">Id to test.</param>
		/// <returns><c>true</c> if registered; otherwise <c>false</c>.</returns>
		public bool Contains(int id)
		{
			EnsureRegistry();
			return registeredDefinition.ContainsKey(id);
		}

		/// <summary>
		/// Clears the runtime dictionary.
		/// </summary>
		/// <remarks>
		/// This does not modify the serialized <see cref="entries"/> list; only the in-memory dictionary.
		/// </remarks>
		public void Clear()
		{
			EnsureRegistry();
			registeredDefinition.Clear();
		}

		/// <summary>
		/// Gets the number of registered voxel definitions in the runtime dictionary.
		/// </summary>
		public int Count
		{
			get
			{
				EnsureRegistry();
				return registeredDefinition.Count;
			}
		}

		/// <summary>
		/// Ensures the runtime dictionary is built before any read/write operation.
		/// </summary>
		private void EnsureRegistry()
		{
			// Lazily rebuild if the dictionary has not been created yet (e.g., asset not enabled).
			if (registeredDefinition == null)
			{
				RebuildFromEntries();
			}
		}
	}
}