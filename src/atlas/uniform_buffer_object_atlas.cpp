#include "atlas/uniform_buffer_object_atlas.hpp"

UniformBufferObjectAtlas::UniformBufferObjectAtlas()
{
	{
		spk::OpenGL::UniformBufferObject cameraData(L"CameraData", 0, 128);

		auto& viewElem = cameraData.addElement(L"view", 0, 64);
		auto& projElem = cameraData.addElement(L"proj", 64, 64);
		
		_ubos.emplace(L"cameraData", std::move(cameraData));
	}

	{
		spk::OpenGL::UniformBufferObject timeData(L"TimeData", 1, 4);

		auto& epochElem = timeData.addElement(L"epoch", 0, 4);

		_ubos.emplace(L"timeData", std::move(timeData));
	}
}

void UniformBufferObjectAtlas::_loadElement(spk::OpenGL::UniformBufferObject &p_ubo, const spk::JSON::Object &p_elementDesc)
{
	std::wstring name = p_elementDesc[L"Name"].as<std::wstring>();
	size_t offset = static_cast<size_t>(p_elementDesc[L"Offset"].as<long>());

	if (p_elementDesc.contains(L"Size"))
	{
		size_t size = static_cast<size_t>(p_elementDesc[L"Size"].as<long>());
		auto &newElement = p_ubo.addElement(name, offset, size);

		if (p_elementDesc.contains(L"NestedElements"))
		{
			const auto &nestedArray = p_elementDesc[L"NestedElements"].asArray();
			for (const auto *child : nestedArray)
			{
				_loadElement(newElement, *child);
			}
		}
	}
	else if (p_elementDesc.contains(L"NbElements"))
	{
		size_t nbElement = static_cast<size_t>(p_elementDesc[L"NbElements"].as<long>());
		size_t elementSize = static_cast<size_t>(p_elementDesc[L"ElementSize"].as<long>());
		size_t padding = static_cast<size_t>(p_elementDesc[L"ElementPadding"].as<long>());

		auto &arrayElement = p_ubo.addElement(name, offset, nbElement, elementSize, padding);

		if (p_elementDesc.contains(L"ElementComposition"))
		{
			const auto &composition = p_elementDesc[L"ElementComposition"].asArray();
			for (size_t i = 0; i < arrayElement.nbElement(); ++i)
			{
				for (const auto *comp : composition)
				{
					_loadElement(arrayElement[i], *comp);
				}
			}
		}
	}
	else
	{
		GENERATE_ERROR("[UniformBufferObjectAtlas] - Element description must contain either 'Size' or 'NbElements'.");
	}
}

void UniformBufferObjectAtlas::_loadElement(
        spk::OpenGL::ShaderStorageBufferObject::DynamicArray& p_array,
        const spk::JSON::Object&                              p_elemDesc)
{
    std::wstring name   = p_elemDesc[L"Name"  ].as<std::wstring>();
    size_t offset = static_cast<size_t>(p_elemDesc[L"Offset"].as<long>());

    if (p_elemDesc.contains(L"Size"))
    {
        size_t size = static_cast<size_t>(p_elemDesc[L"Size"].as<long>());
        auto&  elem = p_array.addElement(name, offset, size);

        if (p_elemDesc.contains(L"NestedElements"))
        {
            for (auto* child : p_elemDesc[L"NestedElements"].asArray())
                _loadElement(elem, *child);
        }
    }
    else if (p_elemDesc.contains(L"NbElements"))
    {
        size_t nbElem   = static_cast<size_t>(p_elemDesc[L"NbElements"    ].as<long>());
        size_t elemSize = static_cast<size_t>(p_elemDesc[L"ElementSize"   ].as<long>());
        size_t padding  = static_cast<size_t>(p_elemDesc[L"ElementPadding"].as<long>());

        auto& elemArr = p_array.addElement(name, offset, nbElem, elemSize, padding);

        if (p_elemDesc.contains(L"ElementComposition"))
        {
            for (size_t i = 0; i < elemArr.nbElement(); ++i)
            {
                for (auto* comp : p_elemDesc[L"ElementComposition"].asArray())
                    _loadElement(elemArr[i], *comp);
            }
        }
    }
    else
    {
        GENERATE_ERROR("[UniformBufferObjectAtlas] - Element description for SSBO must contain either 'Size' or 'NbElements'.");
    }
}

void UniformBufferObjectAtlas::_loadElement(spk::DataBufferLayout::Element &p_parent, const spk::JSON::Object &p_elementDesc)
{
	std::wstring name = p_elementDesc[L"Name"].as<std::wstring>();
	size_t offset = p_parent.offset() + static_cast<size_t>(p_elementDesc[L"Offset"].as<long>());

	if (p_elementDesc.contains(L"Size"))
	{
		size_t size = static_cast<size_t>(p_elementDesc[L"Size"].as<long>());
		auto &newElement = p_parent.addElement(name, offset, size);

		if (p_elementDesc.contains(L"NestedElements"))
		{
			const auto &nestedArray = p_elementDesc[L"NestedElements"].asArray();
			for (const auto *child : nestedArray)
			{
				_loadElement(newElement, *child);
			}
		}
	}
	else if (p_elementDesc.contains(L"NbElements"))
	{
		size_t nbElement = static_cast<size_t>(p_elementDesc[L"NbElements"].as<long>());
		size_t elementSize = static_cast<size_t>(p_elementDesc[L"ElementSize"].as<long>());
		size_t padding = static_cast<size_t>(p_elementDesc[L"ElementPadding"].as<long>());

		auto &arrayElement = p_parent.addElement(name, offset, nbElement, elementSize, padding);

		if (p_elementDesc.contains(L"ElementComposition"))
		{
			const auto &composition = p_elementDesc[L"ElementComposition"].asArray();
			for (size_t i = 0; i < arrayElement.nbElement(); ++i)
			{
				for (const auto *comp : composition)
				{
					_loadElement(arrayElement[i], *comp);
				}
			}
		}
	}
	else
	{
		GENERATE_ERROR("[UniformBufferObjectAtlas] - Element description must contain either 'Size' or 'NbElements'.");
	}
}

bool UniformBufferObjectAtlas::containsUBO(const std::wstring &p_name) const
{
	return _ubos.contains(p_name);
}

spk::OpenGL::UniformBufferObject& UniformBufferObjectAtlas::ubo(const std::wstring &p_name)
{
	if (!_ubos.contains(p_name))
	{
		GENERATE_ERROR("[UniformBufferObjectAtlas] - UniformBufferObject '" + spk::StringUtils::wstringToString(p_name) + "' not found.");
	}

	return (_ubos.at(p_name));
}

const std::unordered_map<std::wstring, spk::OpenGL::UniformBufferObject> &UniformBufferObjectAtlas::ubos() const
{
	return _ubos;
}

bool UniformBufferObjectAtlas::containsSSBO(const std::wstring& p_name) const
{
    return _ssbos.contains(p_name);
}

spk::OpenGL::ShaderStorageBufferObject& UniformBufferObjectAtlas::ssbo(const std::wstring& p_name)
{
    if (!_ssbos.contains(p_name))
        GENERATE_ERROR("[UniformBufferObjectAtlas] - ShaderStorageBufferObject '"
                       + spk::StringUtils::wstringToString(p_name) + "' not found.");

    return _ssbos.at(p_name);
}

const std::unordered_map<std::wstring, spk::OpenGL::ShaderStorageBufferObject>& UniformBufferObjectAtlas::ssbos() const
{
    return _ssbos;
}