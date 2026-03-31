using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleContext
{
	public Board Board;
	public List<BattleObject> ActiveObjects = new List<BattleObject>();
}
