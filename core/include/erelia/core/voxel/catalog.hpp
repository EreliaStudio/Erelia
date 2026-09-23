#pragma once

#include <filesystem>
#include <memory>
#include <sstream>
#include <string>
#include <unordered_map>
#include <utility>

#include <exception.hpp>

#include "erelia/core/voxel/definition.hpp"
#include "erelia/core/voxel/shape.hpp"

namespace Voxel
{
	class Catalog;

	namespace Detail
	{
		template <typename TElement>
		class ImmutableCatalogStorage
		{
		protected:
			using ID = typename TElement::ID;

			std::unordered_map<ID, std::shared_ptr<const TElement>> _elements;
			std::string _elementName;

			explicit ImmutableCatalogStorage(std::string elementName) :
				_elementName(std::move(elementName))
			{
			}

			[[nodiscard]] const std::shared_ptr<const TElement> &_sharedAt(const ID &id) const
			{
				const auto found = _elements.find(id);
				if (found == _elements.end())
				{
					std::ostringstream stream;
					stream << "unknown voxel " << _elementName << " ID '" << id << "'";
					throw spk::Exception(stream.str());
				}
				return found->second;
			}

			void _insert(ID id, std::shared_ptr<const TElement> element)
			{
				if (!_elements.emplace(std::move(id), std::move(element)).second)
				{
					throw spk::Exception("duplicate voxel " + _elementName + " ID");
				}
			}

		public:
			[[nodiscard]] const TElement &at(const ID &id) const
			{
				return *_sharedAt(id);
			}

			[[nodiscard]] const TElement &operator[](const ID &id) const
			{
				return at(id);
			}

			[[nodiscard]] bool contains(const ID &id) const noexcept
			{
				return _elements.contains(id);
			}

			[[nodiscard]] const TElement *tryGet(const ID &id) const noexcept
			{
				const auto found = _elements.find(id);
				return found == _elements.end() ? nullptr : found->second.get();
			}
		};
	}

	class Shape::Catalog final : public Detail::ImmutableCatalogStorage<Shape>
	{
		using Base = Detail::ImmutableCatalogStorage<Shape>;

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

	class Definition::Catalog final : public Detail::ImmutableCatalogStorage<Definition>
	{
		using Base = Detail::ImmutableCatalogStorage<Definition>;

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
