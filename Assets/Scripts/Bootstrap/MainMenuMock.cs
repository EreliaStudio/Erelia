using UnityEngine;

[DisallowMultipleComponent]
public sealed class MainMenuMock : MonoBehaviour
{
	public void EnterGame()
	{
		EventCenter.EmitEnteringGame(new GameSaveData());
	}
}
