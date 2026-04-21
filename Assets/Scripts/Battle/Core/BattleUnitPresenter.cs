using UnityEngine;

[DisallowMultipleComponent]
public class BattleUnitPresenter : MonoBehaviour
{
	[SerializeField] private BattleUnitView battleUnitView;

	private BattleUnit battleUnit;
	private BoardData boardData;

	public BattleUnit BattleUnit => battleUnit;
	public BattleUnitView BattleUnitView => battleUnitView;

	private void Awake()
	{
		if (battleUnitView == null)
		{
			Logger.LogError("[BattleUnitPresenter] BattleUnitView is not assigned in the inspector. Please assign a BattleUnitView child to the BattleUnitPresenter component.", Logger.Severity.Critical, this);
		}
	}

	public void Bind(BattleUnit p_battleUnit, BoardData p_boardData)
	{
		if (battleUnit != null)
		{
			battleUnit.PositionChanged -= OnUnitPositionChanged;
		}

		battleUnit = p_battleUnit;
		boardData = p_boardData;

		if (battleUnit != null)
		{
			battleUnit.PositionChanged += OnUnitPositionChanged;
		}

		RefreshVisual();
		RefreshPosition();
	}

	public void RefreshVisual()
	{
		if (battleUnitView == null)
		{
			return;
		}

		GameObject modelPrefab = null;
		if (battleUnit?.SourceUnit != null &&
			battleUnit.SourceUnit.TryGetForm(out CreatureForm form) &&
			form != null)
		{
			modelPrefab = form.ModelPrefab;
		}

		if (modelPrefab != null)
		{
			battleUnitView.SetModel(modelPrefab);
		}
		else
		{
			battleUnitView.ClearModel();
		}
	}

	private void OnDestroy()
	{
		if (battleUnit != null)
		{
			battleUnit.PositionChanged -= OnUnitPositionChanged;
		}
	}

	private void OnUnitPositionChanged(BattleUnit p_unit, Vector3Int? p_position)
	{
		RefreshPosition();
	}

	private void RefreshPosition()
	{
		if (battleUnit == null || !battleUnit.HasBoardPosition || boardData?.Terrain?.VoxelRegistry == null)
		{
			return;
		}

		if (!VoxelTraversalUtility.TryGetWorldHeight(boardData.Terrain, battleUnit.BoardPosition, CardinalHeightSet.Direction.Stationary, boardData.Terrain.VoxelRegistry, out float height))
		{
			return;
		}

		Vector3Int anchor = boardData.WorldAnchor;
		transform.position = new Vector3(
			anchor.x + battleUnit.BoardPosition.x + 0.5f,
			height,
			anchor.z + battleUnit.BoardPosition.z + 0.5f);
	}
}
