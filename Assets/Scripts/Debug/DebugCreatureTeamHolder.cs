using UnityEngine;

public class DebugCreatureTeamHolder : MonoBehaviour
{
	[SerializeField] private CreatureCardListElementUI creatureCardListElementUI;
	[SerializeField] private CreatureUnit[] creatureTeam = new CreatureUnit[6];

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

		creatureCardListElementUI.Bind(creatureTeam);
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