namespace Erelia.Battle.Phase.Placement.Event
{
	public sealed class PlacementCreatureSelected : Erelia.Core.Event.GenericEvent
	{
		public Erelia.Core.Creature.Instance.Model Creature { get; }

		public PlacementCreatureSelected(Erelia.Core.Creature.Instance.Model creature)
		{
			Creature = creature;
		}
	}
}