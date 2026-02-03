using UnityEngine;

[CreateAssetMenu(menuName = "Creature/Species")]
public class CreatureSpecies : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
}
