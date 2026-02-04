using System;
using UnityEngine;

[Serializable]
public class TeamData
{
    public const int MaxSize = 6;

    [SerializeField] private CreatureData[] creatures = new CreatureData[MaxSize];

    public event Action Changed;

    public CreatureData[] Creatures => creatures;

    public void CopyFrom(CreatureData[] source)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < creatures.Length; i++)
        {
            creatures[i] = i < source.Length && source[i] != null ? source[i].Clone() : null;
        }

        Changed?.Invoke();
    }

    public bool TryGetCreature(int index, out CreatureData creature)
    {
        creature = null;
        if (index < 0 || index >= creatures.Length)
        {
            return false;
        }

        creature = creatures[index];
        return creature != null;
    }

    public void SetCreature(int index, CreatureData creature)
    {
        if (index < 0 || index >= creatures.Length)
        {
            return;
        }

        if (creatures[index] == creature)
        {
            return;
        }

        creatures[index] = creature;
        Changed?.Invoke();
    }

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
