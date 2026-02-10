using UnityEngine;

namespace Battle.EncounterTable.Model
{
	[CreateAssetMenu(menuName = "Battle/Encounter Table", fileName = "NewEncounterTable")]
	public class Data : ScriptableObject
	{
		[SerializeField, Range(0f, 1f)] private float fightChance = 0.1f;
		public float FightChance => fightChance;
		
		[SerializeField] private Vector2Int boardArea = new Vector2Int(10, 10);
		public Vector2Int BoardArea => boardArea;
	}
}
