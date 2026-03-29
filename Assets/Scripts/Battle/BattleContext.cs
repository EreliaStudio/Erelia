using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleContext
{
    public List<BattleObject> ActiveObjects = new List<BattleObject>();

    [Serializable]
	public class BattleCell
	{
		public BattleUnit Unit;
		public List<BattleInteractiveObject> InteractiveObjects = new List<BattleInteractiveObject>();
	}

	public Dictionary<Vector3Int, BattleCell> Cells = new Dictionary<Vector3Int, BattleCell>();
	public Dictionary<BattleObject, Vector3Int> PositionByObject = new Dictionary<BattleObject, Vector3Int>();
}
