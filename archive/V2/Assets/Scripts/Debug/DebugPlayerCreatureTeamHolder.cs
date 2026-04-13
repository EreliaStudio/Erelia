using UnityEngine;

public class DebugPlayerCreatureTeamHolder : MonoBehaviour
{
	[SerializeField] private CreatureCardListElementUI playerCreatureCardListElementUI;
	[SerializeField] private CreatureUnit[] playerTeam = new CreatureUnit[6];

	private void Start()
	{
		Apply();
	}

	[ContextMenu("Apply")]
	public void Apply()
	{
		playerCreatureCardListElementUI.Bind(playerTeam);
	}
}
