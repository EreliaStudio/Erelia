using UnityEngine;

public class DebugCreatureTeamHolder : MonoBehaviour
{
	[SerializeField] private CreatureCardListElementUI creatureCardListElementUI;
	[SerializeField] private CreatureUnit[] creatureTeam = new CreatureUnit[6];

	private void Start()
	{
		Apply();
	}

	[ContextMenu("Apply")]
	public void Apply()
	{
		creatureCardListElementUI.Bind(creatureTeam);
	}
}
