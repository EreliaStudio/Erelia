#pragma once

#include <sparkle.hpp>

class UniformBufferObjectAtlas : public spk::Singleton<UniformBufferObjectAtlas>
{
	friend class spk::Singleton<UniformBufferObjectAtlas>;

private:
	std::unordered_map<std::wstring, spk::OpenGL::UniformBufferObject> _ubos;
	std::unordered_map<std::wstring, spk::OpenGL::ShaderStorageBufferObject>    _ssbos;

	void _loadElement(spk::OpenGL::UniformBufferObject &p_ubo, const spk::JSON::Object &p_elementDesc);
	void _loadElement(spk::OpenGL::ShaderStorageBufferObject::DynamicArray& p_array, const spk::JSON::Object& p_elemDesc);
	void _loadElement(spk::DataBufferLayout::Element &p_parent, const spk::JSON::Object &p_elementDesc);

public:
	UniformBufferObjectAtlas();
	
	bool containsUBO(const std::wstring &p_name) const;
	spk::OpenGL::UniformBufferObject& ubo(const std::wstring &p_name);
	const std::unordered_map<std::wstring, spk::OpenGL::UniformBufferObject> &ubos() const;

	bool containsSSBO(const std::wstring& p_name) const;
    spk::OpenGL::ShaderStorageBufferObject& ssbo(const std::wstring& p_name);
    const std::unordered_map<std::wstring, spk::OpenGL::ShaderStorageBufferObject>& ssbos() const;
};