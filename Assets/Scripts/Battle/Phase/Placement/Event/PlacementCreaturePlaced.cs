namespace Erelia.Battle.Phase.Placement.Event
{
	public sealed class PlacementCreaturePlaced : Erelia.Core.Event.GenericEvent
	{
		public Erelia.Core.Creature.Instance.Model Creature { get; }

		public PlacementCreaturePlaced(Erelia.Core.Creature.Instance.Model creature)
		{
			Creature = creature;
		}
	}
}