#include "voxel/voxel_model.hpp"

#include <cstdint>
#include <map>
#include <set>
#include <string>

#include <exception.hpp>

namespace voxel
{
	namespace
	{
		[[nodiscard]] Voxel::Orientation loadOrientation(const spk::JSON::Reader &reader)
		{
			static const std::map<std::string, Voxel::Orientation> values = {
				{"negative-x", Voxel::Orientation::NegativeX},
				{"negative-z", Voxel::Orientation::NegativeZ},
				{"positive-x", Voxel::Orientation::PositiveX},
				{"positive-z", Voxel::Orientation::PositiveZ}};
			return reader.requireEnum<Voxel::Orientation>("orientation", values);
		}

		[[nodiscard]] Voxel::Flip loadFlip(const spk::JSON::Reader &reader)
		{
			if (!reader.contains("flip"))
				return Voxel::Flip::PositiveY;
			static const std::map<std::string, Voxel::Flip> values = {
				{"negative-y", Voxel::Flip::NegativeY},
				{"positive-y", Voxel::Flip::PositiveY}};
			return reader.requireEnum<Voxel::Flip>("flip", values);
		}

		[[nodiscard]] Voxel::Cell loadCell(const spk::JSON::Reader &reader)
		{
			reader.forbidUnknown({"position", "value", "orientation", "flip"});
			const auto value = reader.require<Voxel::ID>("value");
			if (value > Voxel::Cell::IDMask)
				throw spk::Exception(spk::JSON::detail::makeErrorMessage(reader.file(), reader.pathFor("value"), "voxel value exceeds packed capacity"));
			return Voxel::Cell(value, loadOrientation(reader), loadFlip(reader));
		}

		void validatePosition(const VoxelModel &model, const spk::JSON::Reader &reader, spk::Vector3Int position)
		{
			if (!model.contains(position))
				throw spk::Exception(spk::JSON::detail::makeErrorMessage(reader.file(), reader.pathFor("position"), "voxel position is out of range"));
		}

		void loadVoxels(VoxelModel &model, const spk::JSON::Reader &reader)
		{
			std::set<spk::Vector3Int> positions;
			auto editor = model.edit();
			for (const auto &voxelReader : reader.childArray("voxels"))
			{
				const auto position = voxelReader.require<spk::Vector3Int>("position");
				validatePosition(model, voxelReader, position);
				if (!positions.insert(position).second)
					throw spk::Exception(spk::JSON::detail::makeErrorMessage(voxelReader.file(), voxelReader.pathFor("position"), "duplicate voxel position"));
				editor.set(position, loadCell(voxelReader));
			}
		}
	}

	VoxelModel::VoxelModel(spk::Vector3UInt dimensions, float voxelSize) :
		VoxelVolume(dimensions, voxelSize)
	{
	}

	VoxelModel::VoxelModel(const spk::JSON::Reader &reader) :
		VoxelVolume(reader.require<spk::Vector3UInt>("dimensions"), reader.require<float>("voxelSize"))
	{
		reader.forbidUnknown({"dimensions", "voxelSize", "voxels"});
		loadVoxels(*this, reader);
	}
}
