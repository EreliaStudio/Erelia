using UnityEngine;

[CreateAssetMenu(menuName = "Creatures/Creature Definition")]
public class CreatureDefinition : ScriptableObject
{
    [SerializeField] private string creatureId = "creature";
    [SerializeField] private string displayName = "Creature";
    [SerializeField] private GameObject modelPrefab = null;
    [SerializeField] private Sprite portrait = null;

    public string CreatureId => creatureId;
    public string DisplayName => displayName;
    public GameObject ModelPrefab => modelPrefab;
    public Sprite Portrait => portrait;
}
