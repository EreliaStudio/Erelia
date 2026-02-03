using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Creature/SpeciesRegistry")]
public class CreatureSpeciesRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int Id;
        public CreatureSpecies Species;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();
    private readonly Dictionary<int, CreatureSpecies> data = new Dictionary<int, CreatureSpecies>();

    public IReadOnlyDictionary<int, CreatureSpecies> Data => data;

    public bool TryGetSpecies(int id, out CreatureSpecies species)
    {
        return data.TryGetValue(id, out species);
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
            data[entries[i].Id] = entries[i].Species;
        }
    }
}
