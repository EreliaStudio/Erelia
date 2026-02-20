namespace Erelia.World.Chunk.Generation
{
	public interface IGenerator
	{
		void Generate(Erelia.World.Chunk.Model chunk, Erelia.World.Chunk.Coordinates coordinates);
	}
}
