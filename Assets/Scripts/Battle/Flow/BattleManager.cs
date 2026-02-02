using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	private Dictionary<BattlePhase, BattlePhaseBase> battlePhaseDictionary = new Dictionary<BattlePhase, BattlePhaseBase>();
	[SerializeField] private BattleContext battleContext = new BattleContext();
	[SerializeField] private MouseSurfaceCursorEvents mouseSurfaceEvents = null;

	public BattleRequest CurrentRequest { get; private set; }
	private IBattlePhase currentPhase;
	private bool hasSelectedCell;
	private Vector3Int selectedCell;

	private void OnEnable()
	{
		if (mouseSurfaceEvents != null)
		{
			mouseSurfaceEvents.MoveMouseCursor += HandleMouseSurfaceMove;
			mouseSurfaceEvents.MouseLeaveModel += HandleMouseSurfaceLeave;
		}
	}

	private void OnDisable()
	{
		if (mouseSurfaceEvents != null)
		{
			mouseSurfaceEvents.MoveMouseCursor -= HandleMouseSurfaceMove;
			mouseSurfaceEvents.MouseLeaveModel -= HandleMouseSurfaceLeave;
		}
	}

	public void Initialize(BattleRequest request)
	{
		CurrentRequest = request;

		battlePhaseDictionary.Clear();
		battlePhaseDictionary[BattlePhase.Placement] = new BattlePlacementPhase();

		foreach (var kvp in battlePhaseDictionary)
		{
			kvp.Value.battleContext = battleContext;
		}

		EnterPhase(BattlePhase.Placement);
	}

	public void EnterPhase(BattlePhase phase)
	{
		currentPhase?.OnExit();

		currentPhase = ResolvePhase(phase);

		currentPhase?.OnEntry();
	}

	private IBattlePhase ResolvePhase(BattlePhase phase)
	{
		if (battlePhaseDictionary.TryGetValue(phase, out var resolved) == false)
		{
			return null;
		}
		return resolved;
	}

	private void HandleMouseSurfaceMove(Vector3Int cell, RaycastHit hit)
	{
		BattleBoard board = battleContext != null ? battleContext.BattleBoard : null;
		BattleBoardData data = board != null ? board.Data : null;
		if (data == null)
		{
			return;
		}

		Vector3Int localCell = cell - data.OriginCell;
		if (!IsValidCell(data, localCell))
		{
			ClearSelectedCell(board, data);
			return;
		}

		if (hasSelectedCell && localCell == selectedCell)
		{
			return;
		}

		ClearSelectedCell(board, data);
		data.AddMask(localCell.x, localCell.y, localCell.z, BattleCellMask.Selected);
		board.RebuildMask();

		hasSelectedCell = true;
		selectedCell = localCell;

		Debug.Log("Mouse surface cell: " + cell);
	}

	private void HandleMouseSurfaceLeave()
	{
		BattleBoard board = battleContext != null ? battleContext.BattleBoard : null;
		BattleBoardData data = board != null ? board.Data : null;
		if (data == null)
		{
			return;
		}

		ClearSelectedCell(board, data);
	}

	private void ClearSelectedCell(BattleBoard board, BattleBoardData data)
	{
		if (!hasSelectedCell)
		{
			return;
		}

		if (IsValidCell(data, selectedCell))
		{
			data.RemoveMask(selectedCell.x, selectedCell.y, selectedCell.z, BattleCellMask.Selected);
			board.RebuildMask();
		}

		hasSelectedCell = false;
		selectedCell = default;
	}

	private static bool IsValidCell(BattleBoardData data, Vector3Int cell)
	{
		if (data == null)
		{
			return false;
		}

		return cell.x >= 0 && cell.x < data.SizeX
			&& cell.y >= 0 && cell.y < data.SizeY
			&& cell.z >= 0 && cell.z < data.SizeZ;
	}
}
