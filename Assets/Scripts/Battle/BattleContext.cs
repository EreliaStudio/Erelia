using System;

[Serializable]
public sealed class BattleContext
{
	public BoardData Board { get; }

	public BattleContext(BoardData p_board)
	{
		Board = p_board ?? throw new ArgumentNullException(nameof(p_board));
	}

	public bool CanMove(BattleObject p_object, UnityEngine.Vector3Int p_targetPosition)
	{
		return Board.CanPlace(p_object, p_targetPosition);
	}

	public bool TryMove(BattleObject p_object, UnityEngine.Vector3Int p_targetPosition)
	{
		return Board.TryMove(p_object, p_targetPosition);
	}
}