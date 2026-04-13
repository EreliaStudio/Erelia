using UnityEngine;

public class DebugEncounterTeamHolder : MonoBehaviour
{
	[SerializeField] private CreatureCardListElementUI creatureCardListElementUI;
	[SerializeField] private EncounterTier encounterTier = new EncounterTier();

	private void Start()
	{
		Apply();
	}

	[ContextMenu("Apply")]
	public void Apply()
	{
		EncounterTier.Entry teamEntry = encounterTier.WeightedTeams.Count > 0
			? encounterTier.WeightedTeams[0]
			: null;

		if (teamEntry == null)
		{
			creatureCardListElementUI.Clear();
			return;
		}

		creatureCardListElementUI.Bind(teamEntry.Team);
	}
}
