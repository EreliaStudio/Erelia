namespace Erelia.Battle.Phase.Placement.Event
{
	/// <summary>
	/// Emitted when a creature is removed from the board.
	/// </summary>
	public sealed class PlacementCreatureUnplaced : Erelia.Core.Event.GenericEvent
	{
		/// <summary>
		/// Gets removed unit.
		/// </summary>
		public Erelia.Battle.Unit.Presenter Unit { get; }
		public Erelia.Core.Creature.Instance.Model Creature => Unit?.Model?.Creature;

		public PlacementCreatureUnplaced(Erelia.Battle.Unit.Presenter unit)
		{
			Unit = unit;
		}
	}
}
