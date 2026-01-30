using System;
using UnityEngine;

[Serializable]
public class BattleContext
{
    [SerializeField] private BattleBoard battleBoard = null;

    public BattleBoard BattleBoard
    {
        get => battleBoard;
        set => battleBoard = value;
    }
}
