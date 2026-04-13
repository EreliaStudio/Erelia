namespace Erelia.Battle.Phase.Placement.Event
{
	public sealed class PlacementUnitSelected : Erelia.Core.Event.GenericEvent
	{
		public PlacementUnitSelected(Erelia.Battle.Unit.Presenter unit)
		{
			Unit = unit;
		}

		public Erelia.Battle.Unit.Presenter Unit { get; }
	}
}
