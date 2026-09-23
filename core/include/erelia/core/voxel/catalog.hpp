#pragma once

#include <filesystem>
#include <memory>

#include <container/json/catalog.hpp>

#include "erelia/core/voxel/definition.hpp"
#include "erelia/core/voxel/shape.hpp"

namespace Voxel
{
	class Catalog;

	class Shape::Catalog final : private spk::JSON::Catalog<Shape>
	{
		using Base = spk::JSON::Catalog<Shape>;

		friend class Voxel::Catalog;
		friend class Definition::Catalog;

		Catalog();
		void _load(const std::filesystem::path &path);
		[[nodiscard]] std::shared_ptr<const Shape> _sharedShape(const Shape::ID &id) const;

	public:
		using Base::at;
		using Base::contains;
		using Base::operator[];
		using Base::tryGet;
	};

	class Definition::Catalog final : private spk::JSON::Catalog<Definition, const Shape::Catalog &>
	{
		using Base = spk::JSON::Catalog<Definition, const Shape::Catalog &>;

		friend class Voxel::Catalog;

		const Shape::Catalog *_shapes;

		explicit Catalog(const Shape::Catalog &shapes);
		void _load(const std::filesystem::path &path);

	public:
		using Base::at;
		using Base::contains;
		using Base::operator[];
		using Base::tryGet;
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
