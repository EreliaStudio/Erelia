using UnityEngine;

public class DebugEncounterTeamHolder : MonoBehaviour
{
	[SerializeField] private CreatureCardListElementUI creatureCardListElementUI;
	[SerializeField] private EncounterTier encounterTier = new EncounterTier(); // The encounter tier should output in the inspector a list,
	// where i can add or remove weighted teams. I want each element of the list to output on one line, with the weight and the button "Edit Team"
 
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

		creatureCardListElementUI.Bind(encounterTier.WeightedTeams[0].Team);
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (Application.isPlaying == false)
		{
			return;
		}

		Apply();
	}
#endif
}