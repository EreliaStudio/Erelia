using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Voxel
{
	public static class Registry
	{
		private static readonly Dictionary<int, Erelia.Voxel.Definition> registeredDefinition = new();

		private const string ResourcesPath = "Voxel/VoxelLibrary";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			VoxelLibrary library = Resources.Load<VoxelLibrary>(ResourcesPath);
			if (library == null)
			{
				Erelia.Logger.RaiseWarning($"VoxelLibrary not found at Resources path '{ResourcesPath}'. Registry not initialized.");
				return;
			}

			int registered = library.RegisterAll(clearRegistry: true);
			Erelia.Logger.Log($"Voxel Registry initialized with {registered} definitions from '{library.name}'.");
		}

		public static void Add(int id, Erelia.Voxel.Definition definition)
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

		public static bool TryGet(int id, out Erelia.Voxel.Definition definition)
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

