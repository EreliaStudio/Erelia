using AYellowpaper.SerializedCollections;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCreatureSpecies", menuName = "Game/Creature Species")]
public class CreatureSpecies : ScriptableObject
{
	public string DisplayName= "UnnamedSpecies";
	public Attributes Attributes = new Attributes();
	public List<Ability> Abilities = new List<Ability>();

	[HideInInspector]
	public FeatBoard FeatBoard = new FeatBoard();

	[SerializedDictionary("Form Name", "Form Data")]
	public SerializedDictionary<string, CreatureForm> Forms = new SerializedDictionary<string, CreatureForm>();
};