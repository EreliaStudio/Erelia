using System;
using System.Collections.Generic;

namespace Voxel
{
	public static class Registry
	{
		private static readonly Dictionary<int, Voxel.Definition> registeredDefinition = new();

		public static void Add(int id, Voxel.Definition definition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException(nameof(definition));
			}

			if (registeredDefinition.ContainsKey(id))
			{
				throw new ArgumentException($"A voxel definition with id '{id}' is already registered.", nameof(id));
			}

			registeredDefinition.Add(id, definition);
		}

		public static bool TryGet(int id, out Voxel.Definition definition)
		{
			return registeredDefinition.TryGetValue(id, out definition);
		}

		public static bool Contains(int id)
		{
			return registeredDefinition.ContainsKey(id);
		}

		public static void Clear()
		{
			registeredDefinition.Clear();
		}

		public static int Count => registeredDefinition.Count;
	}
}