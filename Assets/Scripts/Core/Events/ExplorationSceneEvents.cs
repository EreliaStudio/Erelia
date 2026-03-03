namespace Erelia.Core.Event
{
	/// <summary>
	/// Event requesting that exploration scene data be provided.
	/// </summary>
	/// <remarks>
	/// Typically emitted by a scene loader or bootstrapper to ask systems
	/// to populate the exploration context before the scene begins.
	/// </remarks>
	public sealed class ExplorationSceneDataRequest : GenericEvent
	{
	}
}
