using System.Collections.Generic;

public class BattleCell
{
    private readonly List<BattleCellMask> masks = new List<BattleCellMask>();

    public IReadOnlyList<BattleCellMask> Masks => masks;

    public bool IsEmpty => masks.Count == 0;

    public bool HasMask(BattleCellMask mask)
    {
        return masks.Contains(mask);
    }

    public void AddMask(BattleCellMask mask)
    {
        if (masks.Contains(mask))
        {
            return;
        }

        masks.Add(mask);
    }

    public void RemoveMask(BattleCellMask mask)
    {
        masks.Remove(mask);
    }

    public void ClearMasks()
    {
        masks.Clear();
    }
}
