using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Team")]
public class PlayerTeam : ScriptableObject
{
    public const int MaxSlots = 6;

    [SerializeField] private TeamSlot[] slots = new TeamSlot[MaxSlots];

    public TeamSlot[] Slots => slots;

    public bool TryGetSlot(int index, out TeamSlot slot)
    {
        slot = default;
        if (slots == null || index < 0 || index >= slots.Length)
        {
            return false;
        }

        slot = slots[index];
        return true;
    }

    public void SetSlot(int index, TeamSlot slot)
    {
        if (slots == null || index < 0 || index >= slots.Length)
        {
            return;
        }

        slots[index] = slot;
    }

    public int CountFilled()
    {
        if (slots == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty)
            {
                count++;
            }
        }

        return count;
    }

    private void OnValidate()
    {
        if (slots == null || slots.Length != MaxSlots)
        {
            TeamSlot[] resized = new TeamSlot[MaxSlots];
            if (slots != null)
            {
                int copy = Mathf.Min(slots.Length, MaxSlots);
                for (int i = 0; i < copy; i++)
                {
                    resized[i] = slots[i];
                }
            }
            slots = resized;
        }
    }
}

[Serializable]
public struct TeamSlot
{
    public CreatureDefinition Creature;
    public int Level;

    public bool IsEmpty => Creature == null;
}
