using System;
using UnityEngine;

namespace Voxel
{
	public static class Loader
	{
		public static int Load(string resourcesFolder)
		{
			if (string.IsNullOrWhiteSpace(resourcesFolder))
			{
				throw new ArgumentException("resourcesFolder cannot be null/empty.", nameof(resourcesFolder));
			}

			Registry.Clear();

			Definition[] definitions = Resources.LoadAll<Definition>(resourcesFolder);
			int registered = 0;

			for (int i = 0; i < definitions.Length; i++)
			{
				Definition def = definitions[i];
				if (def == null)
				{
					continue;
				}

				if (Registry.Contains(def.Id))
				{
					Debug.LogError($"Duplicate voxel id '{def.Id}'. Asset '{def.name}' skipped.");
					continue;
				}

				try
				{
					Registry.Add(def.Id, def);
					registered++;
				}
				catch (Exception ex)
				{
					Debug.LogError($"Failed to register voxel definition '{def.name}' (Id={def.Id}): {ex.Message}");
				}
			}

			return registered;
		}
	}
}
