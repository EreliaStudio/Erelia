using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	private Dictionary<BattlePhase, BattlePhaseBase> battlePhaseDictionary = new Dictionary<BattlePhase, BattlePhaseBase>();
	[SerializeField] private BattleContext battleContext = new BattleContext();
	[SerializeField] private MouseSurfaceCursorEvents mouseSurfaceEvents = null;

	public BattleRequest CurrentRequest { get; private set; }
	private IBattlePhase currentPhase;

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
		Debug.Log("Mouse surface cell: " + cell);
	}

	private void HandleMouseSurfaceLeave()
	{
		Debug.Log("Mouse left battle board");
	}
}
