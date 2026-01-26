using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Voxel/VoxelRegistry")]
public class VoxelRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int Id;
        public Voxel Voxel;
    }

    [SerializeField] public int AirId = -1;
    [SerializeField] private List<Entry> entries = new List<Entry>();
    private readonly Dictionary<int, Voxel> data = new Dictionary<int, Voxel>();

    public IReadOnlyDictionary<int, Voxel> Data => data;

    public bool TryGetVoxel(int id, out Voxel voxel)
    {
        return data.TryGetValue(id, out voxel);
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
            data[entries[i].Id] = entries[i].Voxel;
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
