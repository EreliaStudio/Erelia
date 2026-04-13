namespace Erelia.Battle.Phase.Placement.Event
{
	public sealed class PlacementUnitUnplaced : Erelia.Core.Event.GenericEvent
	{
		public PlacementUnitUnplaced(Erelia.Battle.Unit.Presenter unit)
		{
			Unit = unit;
		}

		public Erelia.Battle.Unit.Presenter Unit { get; }
	}
}
