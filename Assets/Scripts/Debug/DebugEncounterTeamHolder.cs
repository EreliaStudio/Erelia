using UnityEngine;

public class DebugEncounterTeamHolder : MonoBehaviour
{
	[SerializeField] private CreatureCardListElementUI creatureCardListElementUI;
	[SerializeField] private EncounterTier encounterTier = new EncounterTier();

	private void Start()
	{
		Apply();
	}

	public void Apply()
	{
		if (creatureCardListElementUI == null)
		{
			return;
		}

		if (encounterTier == null ||
			encounterTier.WeightedTeams == null ||
			encounterTier.WeightedTeams.Count == 0)
		{
			creatureCardListElementUI.Clear();
			return;
		}

		if (encounterTier.WeightedTeams[0] == null)
		{
			creatureCardListElementUI.Clear();
			return;
		}

		creatureCardListElementUI.Bind(encounterTier.WeightedTeams[0].Team);
	}
}