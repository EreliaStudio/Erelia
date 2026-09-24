#include "erelia/core/voxel/catalog.hpp"

namespace Voxel
{
	Catalog::Catalog() :
		_shapes(),
		_definitions(_shapes)
	{
	}

	void Catalog::load(const std::filesystem::path &shapePath, const std::filesystem::path &definitionPath)
	{
		loadShape(shapePath);
		loadDefinition(definitionPath);
	}

	void Catalog::loadShape(const std::filesystem::path &path)
	{
		_shapes.load(path);
	}

	void Catalog::loadDefinition(const std::filesystem::path &path)
	{
		_definitions.load(path);
	}

	const Shape::Catalog &Catalog::shapes() const noexcept
	{
		return _shapes;
	}

	const Definition::Catalog &Catalog::definitions() const noexcept
	{
		return _definitions;
	}
}
