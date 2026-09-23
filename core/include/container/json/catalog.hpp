#pragma once

#include <cstddef>
#include <filesystem>
#include <string>
#include <unordered_map>
#include <utility>

#include <container/json/reader.hpp>
#include <exception.hpp>

namespace spk::JSON
{
	template <typename TElement>
	class Catalog
	{
	public:
		using Element = TElement;
		using ID = typename Element::ID;

	private:
		std::unordered_map<ID, Element> _elements;

		[[noreturn]] static void _throwAt(
			const std::filesystem::path &file,
			const std::string &path,
			const std::string &message)
		{
			throw spk::Exception(file.generic_string() + ":" + path + ": " + message);
		}

	protected:
		Catalog() = default;
		virtual ~Catalog() = default;

		[[nodiscard]] virtual ID _parseKey(const Reader &reader) const = 0;
		[[nodiscard]] virtual Element _parseElement(const Reader &reader) const = 0;

		void _insert(ID id, Element element)
		{
			if (contains(id))
			{
				throw spk::Exception("duplicate JSON catalog ID");
			}
			_elements.emplace(std::move(id), std::move(element));
		}

		void _load(const std::filesystem::path &file)
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

				ID id = _parseKey(elementReader);
				if (contains(id))
				{
					_throwAt(file, elementReader.pathFor("id"), "duplicate catalog ID");
				}

				const Reader dataReader = elementReader.child("data");
				Element element = _parseElement(dataReader);
				_elements.emplace(std::move(id), std::move(element));
			}
		}

	public:
		[[nodiscard]] const Element &at(const ID &id) const
		{
			const auto found = _elements.find(id);
			if (found == _elements.end())
			{
				throw spk::Exception("unknown JSON catalog ID");
			}
			return found->second;
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
			return found == _elements.end() ? nullptr : &found->second;
		}
	};
}
