using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameSceneEntryEmulator : MonoBehaviour
{
	[SerializeField] private bool enterGameOnStart = true;

	[SerializeReference]
	public CreatureUnit[] playerTeam = new CreatureUnit[GameRule.TeamMemberCount];

	private void Start()
	{
		if (enterGameOnStart)
		{
			EnterGame();
		}
	}

	public void EnterGame()
	{
		var saveData = new GameSaveData();
		ApplyTeamToSave(saveData);
		EventCenter.EmitEnteringGame(saveData);
	}

	private void ApplyTeamToSave(GameSaveData p_saveData)
	{
		if (playerTeam == null || p_saveData?.Player?.Team == null)
		{
			return;
		}

		int count = Mathf.Min(playerTeam.Length, p_saveData.Player.Team.Length);
		for (int index = 0; index < count; index++)
		{
			p_saveData.Player.Team[index] = playerTeam[index];
		}
	}
}
