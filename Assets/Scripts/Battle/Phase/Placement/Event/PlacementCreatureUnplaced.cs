namespace Erelia.Battle.Phase.Placement.Event
{
	/// <summary>
	/// Emitted when a creature is removed from the board.
	/// </summary>
	public sealed class PlacementCreatureUnplaced : Erelia.Core.Event.GenericEvent
	{
		/// <summary>
		/// Gets removed creature.
		/// </summary>
		public Erelia.Core.Creature.Instance.Model Creature { get; }

		public PlacementCreatureUnplaced(Erelia.Core.Creature.Instance.Model creature)
		{
			Creature = creature;
		}
	}
}