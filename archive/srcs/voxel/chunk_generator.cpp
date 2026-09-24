#include "voxel/chunk_generator.hpp"

namespace voxel
{
	Chunk::Generator::Generator(Collection &collection) :
		_collection(collection),
		_requests(collection.subscribeToRequests([this](Coordinate coordinate) {
			try
			{
				_collection.publish(generate(coordinate));
			} catch (...)
			{
				_collection.fail(coordinate);
				throw;
			}
		}))
	{
	}
}
