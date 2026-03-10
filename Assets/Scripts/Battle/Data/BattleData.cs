namespace Erelia.Battle
{
	/// <summary>
	/// Serializable container for battle runtime data.
	/// </summary>
	[System.Serializable]
	public sealed class Data
	{
		public Erelia.Battle.Board.Model Board;
		public Erelia.Core.Creature.Team EnemyTeam;
		public Erelia.Battle.Phase.Info PhaseInfo;
		[System.NonSerialized] public Erelia.Battle.Timeline.Model Timeline;
		[System.NonSerialized] public Erelia.Battle.Unit.Presenter ActiveUnit;

		public System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> Units => Timeline?.Units;

		public Data()
		{
			Board = null;
			EnemyTeam = null;
			PhaseInfo = null;
			Timeline = null;
			ActiveUnit = null;
		}
	}
}
