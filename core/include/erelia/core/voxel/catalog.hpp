#pragma once

#include <filesystem>
#include <memory>
#include <string>
#include <unordered_map>

#include "erelia/core/voxel/definition.hpp"
#include "erelia/core/voxel/shape.hpp"

namespace Voxel
{
	class Catalog;

	class Shape::Catalog final
	{
		friend class Voxel::Catalog;
		friend class Definition::Catalog;

		std::unordered_map<Shape::ID, std::shared_ptr<const Shape>> _elements;

		Catalog();
		void _load(const std::filesystem::path &path);
		void _insert(Shape::ID id, std::shared_ptr<const Shape> shape);
		[[nodiscard]] const std::shared_ptr<const Shape> &_sharedAt(const Shape::ID &id) const;
		[[nodiscard]] std::shared_ptr<const Shape> _sharedShape(const Shape::ID &id) const;

	public:
		[[nodiscard]] const Shape &at(const Shape::ID &id) const;
		[[nodiscard]] const Shape &operator[](const Shape::ID &id) const;
		[[nodiscard]] bool contains(const Shape::ID &id) const noexcept;
		[[nodiscard]] const Shape *tryGet(const Shape::ID &id) const noexcept;
	};

	class Definition::Catalog final
	{
		friend class Voxel::Catalog;

		const Shape::Catalog *_shapes;
		std::unordered_map<Definition::ID, std::shared_ptr<const Definition>> _elements;

		explicit Catalog(const Shape::Catalog &shapes);
		void _load(const std::filesystem::path &path);
		void _insert(Definition::ID id, std::shared_ptr<const Definition> definition);
		[[nodiscard]] const std::shared_ptr<const Definition> &_sharedAt(const Definition::ID &id) const;

	public:
		[[nodiscard]] const Definition &at(const Definition::ID &id) const;
		[[nodiscard]] const Definition &operator[](const Definition::ID &id) const;
		[[nodiscard]] bool contains(const Definition::ID &id) const noexcept;
		[[nodiscard]] const Definition *tryGet(const Definition::ID &id) const noexcept;
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
