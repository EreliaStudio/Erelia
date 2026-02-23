using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelKit
{
	[CreateAssetMenu(menuName = "Voxel/Registry", fileName = "VoxelRegistry")]
	public sealed class Registry : ScriptableObject
	{
		[Serializable]
		public struct Entry
		{
			public int Id;
			public VoxelKit.Definition Definition;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		[NonSerialized] private Dictionary<int, VoxelKit.Definition> registeredDefinition;

		public IReadOnlyList<Entry> Entries => entries;

		private void OnEnable()
		{
			RebuildFromEntries();
		}

		private void OnValidate()
		{
			RebuildFromEntries();
		}

		public int RebuildFromEntries()
		{
			registeredDefinition = new Dictionary<int, VoxelKit.Definition>();

			int registered = 0;
			HashSet<int> seen = new HashSet<int>();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Definition == null)
				{
					continue;
				}

				if (!seen.Add(entry.Id))
				{
					Debug.LogError($"VoxelRegistry '{name}' has a duplicate id '{entry.Id}'.");
					continue;
				}

				if (registeredDefinition.ContainsKey(entry.Id))
				{
					Debug.LogError($"Duplicate voxel id '{entry.Id}' already registered. Definition '{entry.Definition.name}' skipped.");
					continue;
				}

				try
				{
					registeredDefinition.Add(entry.Id, entry.Definition);
					registered++;
				}
				catch (Exception ex)
				{
					Debug.LogError($"Failed to register voxel definition '{entry.Definition.name}' (Id={entry.Id}): {ex.Message}");
				}
			}

			return registered;
		}

		public void Add(int id, VoxelKit.Definition definition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			EnsureRegistry();

			if (registeredDefinition.ContainsKey(id))
			{
				throw new ArgumentException($"A voxel definition with id '{id}' is already registered.", nameof(id));
			}

			registeredDefinition.Add(id, definition);
		}

		public bool TryGet(int id, out VoxelKit.Definition definition)
		{
			EnsureRegistry();
			return registeredDefinition.TryGetValue(id, out definition);
		}

		public bool Contains(int id)
		{
			EnsureRegistry();
			return registeredDefinition.ContainsKey(id);
		}

		public void Clear()
		{
			EnsureRegistry();
			registeredDefinition.Clear();
		}

		public int Count
		{
			get
			{
				EnsureRegistry();
				return registeredDefinition.Count;
			}
		}

		private void EnsureRegistry()
		{
			if (registeredDefinition == null)
			{
				RebuildFromEntries();
			}
		}
	}
}


