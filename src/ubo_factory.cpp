#include "ubo_factory.hpp"

spk::OpenGL::UniformBufferObject& UBOFactory::cameraUBO()
{
	if (spk::Lumina::Shader::Constants::containsUBO(L"cameraUBO") == false)
	{
		/*
		------ C++ ------
		spk::Matrix4x4 viewMatrix() const;
		spk::Matrix4x4 projectionMatrix() const;

		------ glsl ------
		layout(std140, binding = 0) uniform CameraUBO
		{
			mat4 viewMatrix;
			mat4 projectionMatrix;
		} cameraUBO;
		*/

		spk::OpenGL::UniformBufferObject newUBO = spk::OpenGL::UniformBufferObject(L"CameraUBO", 0, 128);

		newUBO.addElement(L"viewMatrix", 0, 64);
		newUBO.addElement(L"projectionMatrix", 0, 64);

		spk::Lumina::Shader::Constants::addUBO(L"cameraUBO", std::move(newUBO));
	}
	return (spk::Lumina::Shader::Constants::ubo(L"cameraUBO"));
}

spk::OpenGL::UniformBufferObject& UBOFactory::timeUBO()
{
    if (!spk::Lumina::Shader::Constants::containsUBO(L"timeUBO"))
    {
        /*
        layout(std140, binding = 1) uniform TimeUBO
        {
            int epoch;
        } timeUBO;
        */
        spk::OpenGL::UniformBufferObject newUBO(L"TimeUBO", 1, 16);
        newUBO.addElement(L"epoch", 0, 4);
        spk::Lumina::Shader::Constants::addUBO(L"timeUBO", std::move(newUBO));
    }
    return spk::Lumina::Shader::Constants::ubo(L"timeUBO");
}

spk::OpenGL::UniformBufferObject& UBOFactory::transformUBO()
{
	spk::OpenGL::UniformBufferObject result(L"TransformUBO", 3, 64);

    result.addElement(L"modelMatrix", 0, 64);

	return (result);
}