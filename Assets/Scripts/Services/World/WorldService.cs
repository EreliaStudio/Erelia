using UnityEngine;

public sealed class WorldService
{
	private readonly GameContext gameContext;

	private VoxelRegistry voxelRegistry;

	public WorldService(GameContext p_gameContext)
	{
		gameContext = p_gameContext;
	}

	public WorldContext WorldContext => gameContext?.World;
	public VoxelRegistry VoxelRegistry => voxelRegistry;

	public void Initialize()
	{
	}

	public void Shutdown()
	{
		voxelRegistry = null;
	}

	public void ConfigureVoxelRegistry(VoxelRegistry p_voxelRegistry)
	{
		voxelRegistry = p_voxelRegistry;
	}

	public bool TryBuildBattleBoard(
		BoardConfiguration p_boardConfiguration,
		Vector3 p_battleOriginWorldPosition,
		out BoardData p_boardData)
	{
		p_boardData = null;

		if (WorldContext?.WorldData == null ||
			voxelRegistry == null ||
			p_boardConfiguration == null)
		{
			return false;
		}

		Vector3Int anchorCell = Vector3Int.FloorToInt(p_battleOriginWorldPosition);
		p_boardData = BoardDataBuilder.Build(
			WorldContext.WorldData,
			voxelRegistry,
			anchorCell,
			p_boardConfiguration);

		return p_boardData != null;
	}
}
