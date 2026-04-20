using UnityEngine;

[CreateAssetMenu(fileName = "CreatureModel", menuName = "Game/Creatures/Creature Model")]
public class CreatureModel : ScriptableObject
{
	public Sprite Avatar;
	public GameObject ModelPrefab;
}
