#version 460 core

layout(location = 0) in vec2 inPosition;
layout(location = 1) in float inDepth;
layout(location = 2) in vec4 inColor;

layout(std140, binding = 1) uniform CameraData
{
	mat4 uView;
	mat4 uProjection;
};

layout(std140) uniform ModelData
{
	mat4 uModel;
};

layout(location = 0) out vec4 vertexColor;

void main()
{
	gl_Position = uProjection * uView * uModel * vec4(inPosition, inDepth, 1.0);
	vertexColor = inColor;
}