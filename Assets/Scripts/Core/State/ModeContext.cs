public sealed class ModeContext
{
	public static readonly ModeContext Empty = new ModeContext();

	public GameContext GameContext { get; set; }
	public BattleSetup BattleSetup { get; set; }
}
