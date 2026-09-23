#pragma once

#include <concepts>
#include <cstddef>
#include <filesystem>
#include <functional>
#include <memory>
#include <string>
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
	concept catalog_element =
		requires {
			typename TElement::ID;
		} && value_readable<typename TElement::ID>;

	template <catalog_element TElement, typename... TArgs>
	class Catalog
	{
	public:
		using Element = TElement;
		using ID = typename Element::ID;
		using KeyParser = std::function<ID(const Reader &, TArgs...)>;
		using ElementParser = std::function<std::shared_ptr<const Element>(const Reader &, const ID &, TArgs...)>;

	private:
		std::unordered_map<ID, std::shared_ptr<const Element>> _elements;
		KeyParser _keyParser;
		ElementParser _elementParser;

		[[noreturn]] static void _throwAt(
			const std::filesystem::path &file,
			const std::string &path,
			const std::string &message)
		{
			throw spk::Exception(file.generic_string() + ":" + path + ": " + message);
		}

		[[nodiscard]] static ID _defaultKeyParser(const Reader &reader, TArgs...)
		{
			return reader.template require<ID>("id");
		}

		[[nodiscard]] static std::shared_ptr<const Element> _defaultElementParser(const Reader &reader, const ID &, TArgs...)
			requires json_readable<Element>
		{
			return std::shared_ptr<const Element>(new Element(reader.value().template as<Element>()));
		}

	protected:
		Catalog(KeyParser keyParser, ElementParser elementParser) :
			_keyParser(std::move(keyParser)),
			_elementParser(std::move(elementParser))
		{
		}

		explicit Catalog(ElementParser elementParser) :
			Catalog(_defaultKeyParser, std::move(elementParser))
		{
		}

		Catalog()
			requires json_readable<Element>
			:
			Catalog(_defaultKeyParser, _defaultElementParser)
		{
		}

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

		void _load(const std::filesystem::path &file, TArgs... args)
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

				ID id = _keyParser(elementReader, args...);
				if (contains(id))
				{
					_throwAt(file, elementReader.pathFor("id"), "duplicate catalog ID");
				}

				const Reader dataReader = elementReader.child("data");
				std::shared_ptr<const Element> element = _elementParser(dataReader, id, args...);
				if (element == nullptr)
				{
					_throwAt(file, dataReader.path(), "catalog element parser returned null");
				}

				_elements.emplace(std::move(id), std::move(element));
			}
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
