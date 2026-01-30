using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Battle Area Profile")]
public class BattleAreaProfile : ScriptableObject
{
    [Header("Shape")]
    [Min(1)][SerializeField] private int size = 8;
    [Header("Placement")]
    [Min(1)][SerializeField] private int placementCellCount = 12;

    public int Size => size;
    public int PlacementCellCount => placementCellCount;
}
