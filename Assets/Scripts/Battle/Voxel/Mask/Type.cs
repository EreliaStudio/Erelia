namespace Erelia.Battle.Voxel.Mask
{
	/// <summary>
	/// Mask types used to mark battle cells (placement, ranges, selection).
	/// These drive overlay mesh generation and user feedback.
	/// </summary>
	public enum Type
	{
		Placement,
		AttackRange,
		MovementRange,
		AreaOfEffect,
		Selected
	}
}
