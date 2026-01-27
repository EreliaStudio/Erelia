using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Encounter Settings")]
public class EncounterSettings : ScriptableObject
{
    [Range(0f, 1f)][SerializeField] private float chanceOnEnter = 0.15f;
    [Range(0f, 1f)][SerializeField] private float chanceOnMove = 0.05f;
    [Min(0f)][SerializeField] private float rollCooldownSeconds = 2f;
    [SerializeField] private string enemyTableId = "default";

    public float ChanceOnEnter => chanceOnEnter;
    public float ChanceOnMove => chanceOnMove;
    public float RollCooldownSeconds => rollCooldownSeconds;
    public string EnemyTableId => enemyTableId;
}
