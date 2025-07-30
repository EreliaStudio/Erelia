#include "chunk.hpp"

spk::Lumina::Shader::Object BakableChunk::Renderer::createObject()
{
	if (_shader == nullptr)
	{
		const std::string vertexCode = R"(#version 450 core

layout(location = 0) in vec2 modelPosition; // 2-D quad position in local space
layout(location = 1) in vec2 modelUVs;      // (0-1) UV inside a single sprite frame
layout(location = 2) in flat int nodeIndex; // which entry in the SSBO to use

layout(std140, binding = 0) uniform CameraUBO
{
    mat4 viewMatrix;
    mat4 projectionMatrix;
} cameraUBO;

layout(std140, binding = 1) uniform TimeUBO
{
    int epoch; // milliseconds since program start (or any epoch you like)
} timeUBO;

struct Node
{
    vec2 sprite; // UV of the *first* frame's top-left corner (0-1)
    struct Animation
    {
        int  nbFrame;          // number of frames in the loop
        vec2 offsetPerFrame;   // UV offset to jump from one frame to the next
        int  duration;         // total duration of the loop, in ms
    } animation;
};

layout(std430, binding = 2) buffer NodeCollectionSSBO
{
    int  nbNode; // for debug only
    Node nodes[];
} nodeCollectionSSBO;

layout(std140, binding = 3) uniform TransformUBO
{
    mat4 modelMatrix;
} transformUBO;

layout(location = 0) out vec2 texCoord;

void main()
{
    Node node = nodeCollectionSSBO.nodes[nodeIndex];

    int currentFrame = 0;
    if (node.animation.nbFrame > 0 && node.animation.duration > 0)
    {
        int timeInCycle   = timeUBO.epoch % node.animation.duration;
        int frameDuration = node.animation.duration / node.animation.nbFrame;
        currentFrame      = timeInCycle / max(frameDuration, 1);
    }

    vec2 frameOffset = node.animation.offsetPerFrame * float(currentFrame);
    vec2 baseUV      = node.sprite + frameOffset;

    texCoord = baseUV + modelUVs;

    vec4 worldPosition = transformUBO.modelMatrix * vec4(modelPosition, 0.0, 1.0);
    vec4 viewPosition  = cameraUBO.viewMatrix   * worldPosition;
    gl_Position        = cameraUBO.projectionMatrix * viewPosition;
})";

		const std::string fragmentCode = R"(#version 450 core

layout(location = 0) in  vec2 texCoord;
layout(location = 0) out vec4 outColor;

layout(binding = 4) uniform sampler2D tilesetTexture;

void main()
{
    vec4 texel = texture(tilesetTexture, texCoord);
    if (texel.a < 0.1)
    {
        discard;
    }
    outColor = texel;
})";

		_shader = std::make_unique<spk::Lumina::Shader>(vertexCode, fragmentCode);
	}

	return _shader->createObject();
}

BakableChunk::Renderer::Renderer() :
	_object(createObject()),
	_cameraUBO(UBOFactory::cameraUBO()),
	_timeUBO(UBOFactory::timeUBO()),
	_nodeCollectionSSBO(SSBOFactory::nodeCollectionSSBO()),
	_transformUBO(UBOFactory::transformUBO()),
	_tilesetTextureSampler(SamplerFactory::tilesetTextureSampler())
{
}

void BakableChunk::Renderer::clear()
{
}
void BakableChunk::Renderer::prepare(const spk::SafePointer<BakableChunk> &)
{
}
void BakableChunk::Renderer::validate()
{
}
void BakableChunk::Renderer::render()
{
}
