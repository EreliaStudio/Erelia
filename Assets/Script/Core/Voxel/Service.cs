using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Voxel
{
	[Serializable]
	public class Service
	{		
		[Serializable]
		public struct Entry
		{
			public int Id;
			public Core.Voxel.Model.Definition Definition;
		}

		private readonly Dictionary<int, Core.Voxel.Model.Definition> data = new Dictionary<int, Core.Voxel.Model.Definition>();
		public IReadOnlyDictionary<int, Core.Voxel.Model.Definition> Data => data;
		[HideInInspector] public static readonly int AirID = -1;

		public Service(List<Entry> entries)
		{
			RebuildDictionary(entries);
		}

		public bool TryGetDefinition(int id, out Core.Voxel.Model.Definition definition)
		{
			if (data.TryGetValue(id, out definition) == false)
			{
				return false;
			}
			return true;
		}

		public bool TryGetData(int id, out Core.Voxel.Model.Data voxel)
		{
			if (TryGetDefinition(id, out Core.Voxel.Model.Definition definition) == false)
			{
				voxel = default;
				return false;
			}

			voxel = definition.Data;
			return true;
		}

		public bool TryGetShape(int id, out Core.Voxel.Geometry.Shape voxel)
		{
			if (TryGetDefinition(id, out Core.Voxel.Model.Definition definition) == false)
			{
				voxel = default;
				return false;
			}

			voxel = definition.Shape;
			return true;
		}

		private void RebuildDictionary(List<Entry> entries)
		{
			data.Clear();

			if (entries == null)
			{
				return ;
			}

			for (int i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];
				var definition = entry.Definition;
				if (definition == null)
				{
					Debug.LogError("Core.Voxel.Service: definition is null.");
					continue;
				}

				if (definition.Shape == null)
				{
					Debug.LogError($"Core.Voxel.Service: definition {definition.name} has no shape.");
					continue;
				}

				definition.Shape.Initialize();
				data[entry.Id] = definition;
			}
		}
	}
}
