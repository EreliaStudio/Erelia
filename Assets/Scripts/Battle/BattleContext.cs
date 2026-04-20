using System;

[Serializable]
public sealed class BattleContext
{
	public BoardData Board { get; }

	public BattleContext(BoardData p_board)
	{
		Board = p_board ?? throw new ArgumentNullException(nameof(p_board));
	}

}