#pragma once

#include <concepts>
#include <cstddef>
#include <filesystem>
#include <functional>
#include <memory>
#include <string>
#include <type_traits>
#include <unordered_map>
#include <utility>

#include <container/json/reader.hpp>
#include <exception.hpp>

namespace spk::JSON
{
	template <typename TType>
	concept value_readable =
		native_integer<TType> ||
		native_floating<TType> ||
		std::same_as<clean_type<TType>, bool> ||
		std::same_as<clean_type<TType>, std::string> ||
		json_readable<TType>;

	template <typename TElement>
		requires requires {
			typename TElement::ID;
		} && value_readable<typename TElement::ID>
	class Catalog
	{
	public:
		using Element = TElement;
		using ID = typename Element::ID;

	private:
		std::unordered_map<ID, std::shared_ptr<const Element>> _elements;

		[[noreturn]] static void _throwAt(
			const std::filesystem::path &file,
			const std::string &path,
			const std::string &message)
		{
			throw spk::Exception(file.generic_string() + ":" + path + ": " + message);
		}

		[[nodiscard]] static ID _defaultKeyParser(const Reader &reader)
		{
			return reader.template require<ID>("id");
		}

		[[nodiscard]] static std::shared_ptr<const Element> _defaultElementParser(const Reader &reader)
			requires json_readable<Element>
		{
			return std::shared_ptr<const Element>(new Element(reader.value().template as<Element>()));
		}

	protected:
		Catalog() = default;

		[[nodiscard]] const std::shared_ptr<const Element> &_sharedAt(const ID &id) const
		{
			const auto found = _elements.find(id);
			if (found == _elements.end())
			{
				throw spk::Exception("unknown JSON catalog ID");
			}
			return found->second;
		}

		void _insert(ID id, std::shared_ptr<const Element> element)
		{
			if (!_elements.emplace(std::move(id), std::move(element)).second)
			{
				throw spk::Exception("duplicate JSON catalog ID");
			}
		}

		template <typename TKeyParser, typename TElementParser>
		void _load(
			const std::filesystem::path &file,
			TKeyParser &&keyParser,
			TElementParser &&elementParser)
		{
			const Value document = Loader::parseFile(file);
			const Reader root(document, file);
			root.forbidUnknown({"elements"});

			if (!root.contains("elements"))
			{
				_throwAt(file, root.pathFor("elements"), "missing required field");
			}

			const Value &elements = root.value().at("elements");
			if (!elements.isArray())
			{
				_throwAt(file, root.pathFor("elements"), "expected an array");
			}

			const auto &array = elements.asArray();
			for (std::size_t index = 0; index < array.size(); ++index)
			{
				const std::string path = root.pathFor("elements") + "[" + std::to_string(index) + "]";
				const Reader elementReader(array[index], file, path);
				elementReader.forbidUnknown({"id", "data"});

				ID id = std::invoke(keyParser, elementReader);
				if (contains(id))
				{
					_throwAt(file, elementReader.pathFor("id"), "duplicate catalog ID");
				}

				const Reader dataReader = elementReader.child("data");
				std::shared_ptr<const Element> element = std::invoke(elementParser, dataReader);
				if (element == nullptr)
				{
					_throwAt(file, dataReader.path(), "catalog element parser returned null");
				}

				_elements.emplace(std::move(id), std::move(element));
			}
		}

		template <typename TElementParser>
		void _load(const std::filesystem::path &file, TElementParser &&elementParser)
		{
			_load(file, _defaultKeyParser, std::forward<TElementParser>(elementParser));
		}

		void _load(const std::filesystem::path &file)
			requires json_readable<Element>
		{
			_load(file, _defaultKeyParser, _defaultElementParser);
		}

	public:
		[[nodiscard]] const Element &at(const ID &id) const
		{
			return *_sharedAt(id);
		}

		[[nodiscard]] const Element &operator[](const ID &id) const
		{
			return at(id);
		}

		[[nodiscard]] bool contains(const ID &id) const noexcept
		{
			return _elements.contains(id);
		}

		[[nodiscard]] const Element *tryGet(const ID &id) const noexcept
		{
			const auto found = _elements.find(id);
			return found == _elements.end() ? nullptr : found->second.get();
		}
	};
}
