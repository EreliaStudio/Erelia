#pragma once

#include <sparkle.hpp>

class UBOFactory
{
private:

public:
	static spk::OpenGL::UniformBufferObject& cameraUBO();
    static spk::OpenGL::UniformBufferObject& timeUBO();
    static spk::OpenGL::UniformBufferObject& transformUBO();
};