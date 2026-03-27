namespace Erelia.Battle.Phase.Placement.Event
{
	public sealed class PlacementUnitPlaced : Erelia.Core.Event.GenericEvent
	{
		public PlacementUnitPlaced(Erelia.Battle.Unit.Presenter unit)
		{
			Unit = unit;
		}

		public Erelia.Battle.Unit.Presenter Unit { get; }
	}
}
