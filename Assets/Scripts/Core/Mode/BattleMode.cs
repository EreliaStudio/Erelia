using UnityEngine;

public sealed class BattleMode : Mode
{
	[SerializeField] private BoardPresenter boardPresenter;
	[SerializeField] private BattlePlayerController battlePlayerController;

	private BattleSetup currentSetup;

	public BattleSetup CurrentSetup => currentSetup;

	private void Awake()
	{
		if (boardPresenter == null)
		{
			Logger.LogError("[BattleMode] BoardPresenter is not assigned in the inspector. Please assign a BoardPresenter to the BattleMode component.", Logger.Severity.Critical, this);
		}

		if (battlePlayerController == null)
		{
			Logger.LogError("[BattleMode] BattlePlayerController is not assigned in the inspector. Please assign a BattlePlayerController to the BattleMode component.", Logger.Severity.Critical, this);
		}
	}

	public void Enter(BattleSetup setup)
	{
		if (setup == null || setup.Board == null)
		{
			return;
		}

		currentSetup = setup;

		base.Enter();

		boardPresenter.transform.parent.transform.position = (Vector3)currentSetup.Board.WorldAnchor;
		boardPresenter.Assign(currentSetup.Board);

		Vector3Int anchor = currentSetup.Board.WorldAnchor;
		Vector3Int size = new Vector3Int(
			currentSetup.Board.Terrain.SizeX,
			currentSetup.Board.Terrain.SizeY,
			currentSetup.Board.Terrain.SizeZ);

		battlePlayerController.Bind(anchor, size, currentSetup.PlayerWorldPosition);
	}

	protected override void OnExit()
	{
		battlePlayerController.Unbind();
		currentSetup = null;
	}
}