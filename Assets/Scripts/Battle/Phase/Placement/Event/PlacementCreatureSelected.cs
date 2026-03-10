namespace Erelia.Battle.Phase.Placement.Event
{
	public sealed class PlacementCreatureSelected : Erelia.Core.Event.GenericEvent
	{
		public Erelia.Battle.Unit.Presenter Unit { get; }
		public Erelia.Core.Creature.Instance.Model Creature => Unit?.Model?.Creature;

		public PlacementCreatureSelected(Erelia.Battle.Unit.Presenter unit)
		{
			Unit = unit;
		}
	}
}
