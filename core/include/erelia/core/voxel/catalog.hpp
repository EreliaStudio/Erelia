#pragma once

#include <filesystem>

#include <container/json/catalog.hpp>

#include "erelia/core/voxel/definition.hpp"
#include "erelia/core/voxel/shape.hpp"

namespace Voxel
{
	class Shape::Catalog final : public spk::JSON::Catalog<Shape>
	{
		friend class Definition::Catalog;

	private:
		Shape _empty;

		[[nodiscard]] Shape::ID _parseKey(const spk::JSON::Reader &reader) const override;
		[[nodiscard]] Shape _parseElement(const spk::JSON::Reader &reader) const override;
	};

	class Definition::Catalog final : public spk::JSON::Catalog<Definition>
	{
	private:
		const Shape::Catalog &_shapes;

		[[nodiscard]] Definition::ID _parseKey(const spk::JSON::Reader &reader) const override;
		[[nodiscard]] Definition _parseElement(const spk::JSON::Reader &reader) const override;

	public:
		explicit Catalog(const Shape::Catalog &shapes);
	};

	class Catalog final
	{
	private:
		Shape::Catalog _shapes;
		Definition::Catalog _definitions;

	public:
		Catalog();
		Catalog(const Catalog &) = delete;
		Catalog &operator=(const Catalog &) = delete;
		Catalog(Catalog &&) = delete;
		Catalog &operator=(Catalog &&) = delete;
		~Catalog() = default;

		void load(const std::filesystem::path &shapePath, const std::filesystem::path &definitionPath);
		void loadShape(const std::filesystem::path &path);
		void loadDefinition(const std::filesystem::path &path);

		[[nodiscard]] const Shape::Catalog &shapes() const noexcept;
		[[nodiscard]] const Definition::Catalog &definitions() const noexcept;
	};
}
