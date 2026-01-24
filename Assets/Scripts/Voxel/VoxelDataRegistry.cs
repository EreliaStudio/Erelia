using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/VoxelDataRegistry")]
public class VoxelDataRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int Id;
        public VoxelData Data;
    }

    [SerializeField] public int AirId = -1;
    [SerializeField] private List<Entry> entries = new List<Entry>();
    private readonly Dictionary<int, VoxelData> data = new Dictionary<int, VoxelData>();

    public IReadOnlyDictionary<int, VoxelData> Data => data;

    public bool TryGetData(int id, out VoxelData voxelData)
    {
        return data.TryGetValue(id, out voxelData);
    }

    private void OnEnable()
    {
        RebuildDictionary();
    }

    private void OnValidate()
    {
        RebuildDictionary();
    }

    private void RebuildDictionary()
    {
        data.Clear();
        for (int i = 0; i < entries.Count; i++)
        {
            data[entries[i].Id] = entries[i].Data;
        }
    }

    public bool TryGetFirstSolidId(out int id)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            int entryId = entries[i].Id;
            if (entryId != AirId)
            {
                id = entryId;
                return true;
            }
        }

        id = AirId;
        return false;
    }
}
