using System;
using UnityEngine;

[Serializable]
public class BattleRequest
{
    public Vector3 PlayerWorldPosition;
    public int Seed;
    public BattleAreaProfile AreaProfile;
    public BattleBoardData Board;
    public string EnemyTableId;
}
