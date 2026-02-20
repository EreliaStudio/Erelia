using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Voxel
{
	[CreateAssetMenu(menuName = "Voxel/Library", fileName = "VoxelLibrary")]
	public class VoxelLibrary : ScriptableObject
	{
		[Serializable]
		public struct Entry
		{
			public int Id;
			public Erelia.Voxel.Definition Definition;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		public IReadOnlyList<Entry> Entries => entries;

		public int RegisterAll(bool clearRegistry)
		{
			if (clearRegistry)
			{
				Registry.Clear();
			}

			int registered = 0;
			HashSet<int> seen = new HashSet<int>();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Definition == null)
				{
					Erelia.Logger.RaiseWarning($"VoxelLibrary '{name}' has a null Definition at index {i}.");
					continue;
				}

				if (!seen.Add(entry.Id))
				{
					Debug.LogError($"VoxelLibrary '{name}' has a duplicate id '{entry.Id}'.");
					continue;
				}

				if (Registry.Contains(entry.Id))
				{
					Debug.LogError($"Duplicate voxel id '{entry.Id}' already registered. Definition '{entry.Definition.name}' skipped.");
					continue;
				}

				try
				{
					Registry.Add(entry.Id, entry.Definition);
					registered++;
				}
				catch (Exception ex)
				{
					Debug.LogError($"Failed to register voxel definition '{entry.Definition.name}' (Id={entry.Id}): {ex.Message}");
				}
			}

			return registered;
		}
	}
}


