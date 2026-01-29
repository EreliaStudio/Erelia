using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Battle Area Profile")]
public class BattleAreaProfile : ScriptableObject
{
    [Header("Shape")]
    [Min(1)][SerializeField] private int size = 8;

    public int Size => size;
}
