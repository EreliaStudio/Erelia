#include "ubo_factory.hpp"

spk::OpenGL::UniformBufferObject& UBOFactory::cameraUBO()
{
	if (spk::Lumina::Shader::Constants::containsUBO(L"cameraUBO") == false)
	{
		spk::OpenGL::UniformBufferObject newUBO = spk::OpenGL::UniformBufferObject(L"CameraUBO", 0, 128);

		newUBO.addElement(L"viewMatrix", 0, 64);
		newUBO.addElement(L"projectionMatrix", 0, 64);

		spk::Lumina::Shader::Constants::addUBO(L"cameraUBO", std::move(newUBO));
	}
	return (spk::Lumina::Shader::Constants::ubo(L"cameraUBO"));
}