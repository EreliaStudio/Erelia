using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel
{
	[Serializable]
	public class Service
	{		
		[Serializable]
		public struct Entry
		{
			public int Id;
			public Voxel.Model.Definition Definition;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		

		private readonly Dictionary<int, Voxel.Model.Definition> data = new Dictionary<int, Voxel.Model.Definition>();
		public IReadOnlyDictionary<int, Voxel.Model.Definition> Data => data;
		[HideInInspector] public static readonly int AirID = -1;
		
		public void Init()
		{
			RebuildDictionary(entries);
		}

		public bool TryGetDefinition(int id, out Voxel.Model.Definition definition)
		{
			if (data.TryGetValue(id, out definition) == false)
			{
				return false;
			}
			return true;
		}

		public bool TryGetData(int id, out Voxel.Model.Data voxel)
		{
			if (TryGetDefinition(id, out Voxel.Model.Definition definition) == false)
			{
				voxel = default;
				return false;
			}

			voxel = definition.Data;
			return true;
		}

		public bool TryGetShape(int id, out Voxel.View.Shape voxel)
		{
			if (TryGetDefinition(id, out Voxel.Model.Definition definition) == false)
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
			for (int i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];
				var definition = entry.Definition;
				if (definition == null)
				{
					Debug.LogError("Voxel.Service: definition is null.");
					continue;
				}

				if (definition.Shape == null)
				{
					Debug.LogError($"Voxel.Service: definition {definition.name} has no shape.");
					continue;
				}

				data[entry.Id] = definition;
			}
		}
	}
}
