using AYellowpaper.SerializedCollections;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCreatureSpecies", menuName = "Game/Creature Species")]
public class CreatureSpecies : ScriptableObject
{
	public Attributes Attributes = new Attributes();
	public List<Ability> DefaultAbilities = new List<Ability>();

	[SerializeField]
	public TamingProfile TamingProfile = new TamingProfile();

	[HideInInspector]
	public FeatBoard FeatBoard = new FeatBoard();

	[SerializedDictionary("Form Id", "Form Data")]
	public SerializedDictionary<string, CreatureForm> Forms = new SerializedDictionary<string, CreatureForm>();
}