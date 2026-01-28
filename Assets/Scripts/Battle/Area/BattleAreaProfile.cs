using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Battle Area Profile")]
public class BattleAreaProfile : ScriptableObject
{
    [Header("Shape")]
    [Min(1)][SerializeField] private int size = 8;
    [Range(0.01f, 1f)][SerializeField] private float noiseScale = 0.15f;
    [Range(0f, 1f)][SerializeField] private float noiseStrength = 0.5f;
    [Range(0f, 1f)][SerializeField] private float minEdgeChance = 0.25f;
    [Min(1)][SerializeField] private int minCells = 32;
    [Min(0)][SerializeField] private int fillRadius = 0;

    [Header("Height Sampling")]
    [Min(0)][SerializeField] private int verticalUp = 8;
    [Min(0)][SerializeField] private int verticalDown = 8;

    public int Size => size;
    public float NoiseScale => noiseScale;
    public float NoiseStrength => noiseStrength;
    public float MinEdgeChance => minEdgeChance;
    public int MinCells => minCells;
    public int FillRadius => fillRadius;
    public int VerticalUp => verticalUp;
    public int VerticalDown => verticalDown;
}
