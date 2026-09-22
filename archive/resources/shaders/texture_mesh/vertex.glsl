#version 460 core

layout(location=0) in vec3 inPosition;
layout(location=1) in vec3 inNormal;
layout(location=2) in vec2 inUV;

layout(std140, binding = 1) uniform CameraData
{
	mat4 uView;
	mat4 uProjection;
};

layout(std140) uniform Model3DData
{
	mat4 uModel;
};

layout(location=0) out vec2 vertexUV;
layout(location=1) out float light;

void main()
{
	gl_Position = uProjection * uView * uModel * vec4(inPosition,1.0);
	vertexUV = inUV;
	vec3 n = normalize(transpose(inverse(mat3(uModel)))*inNormal);
	light = 0.35 + 0.65 * max(dot(n, normalize(vec3(.35,.8,.25))), 0.0);
}
