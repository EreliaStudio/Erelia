using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureModelRegistry", menuName = "Game/Creatures/Creature Model Registry")]
public class CreatureModelRegistry : ScriptableObject
{
	[SerializedDictionary("Model Id", "Creature Model")]
	public SerializedDictionary<string, CreatureModel> ModelsById = new SerializedDictionary<string, CreatureModel>();

	public bool TryGetModel(string p_modelId, out CreatureModel p_model)
	{
		p_model = null;
		return !string.IsNullOrWhiteSpace(p_modelId) &&
			ModelsById != null &&
			ModelsById.TryGetValue(p_modelId, out p_model) &&
			p_model != null;
	}
}
