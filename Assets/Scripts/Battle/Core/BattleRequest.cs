using System;
using UnityEngine;

[Serializable]
public class BattleRequest
{
    public Vector3 PlayerWorldPosition;
    public Vector3 CameraLocalPosition;
    public int Seed;
    public BattleAreaProfile AreaProfile;
    public BattleBoard BattleBoard;
    public VoxelRegistry Registry;
    public string EnemyTableId;
}
