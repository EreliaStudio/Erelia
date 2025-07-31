#include "chunk.hpp"

#include "ssbo_factory.hpp"
#include "sampler_factory.hpp"
#include "ubo_factory.hpp"

spk::Lumina::Shader::Object BakableChunk::Renderer::createObject()
{
	if (_shader == nullptr)
	{
		const std::string vertexCode = R"(#version 450 core

layout(location = 0) in vec3 modelPosition; // 2-D quad position in local space
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

    vec4 worldPosition = transformUBO.modelMatrix * vec4(modelPosition, 1.0);
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
		
        _shader->addAttribute(0, spk::OpenGL::LayoutBufferObject::Attribute::Type::Vector3);
        _shader->addAttribute(1, spk::OpenGL::LayoutBufferObject::Attribute::Type::Vector2);
        _shader->addAttribute(2, spk::OpenGL::LayoutBufferObject::Attribute::Type::Int);

        const auto  &tilesetSampler   = SamplerFactory::tilesetTextureSampler();  // binding 4
        const auto  &cameraUBO        = UBOFactory::cameraUBO();                  // binding 0
        const auto  &timeUBO          = UBOFactory::timeUBO();                    // binding 1
        const auto  &transformUBO     = UBOFactory::transformUBO();               // binding 3
        const auto  &nodeCollection   = SSBOFactory::nodeCollectionSSBO();        // binding 2

        _shader->addSampler(L"tilesetTexture",      tilesetSampler, spk::Lumina::Shader::Mode::Constant);
        _shader->addUBO    (L"cameraUBO",           cameraUBO,      spk::Lumina::Shader::Mode::Constant);
        _shader->addUBO    (L"timeUBO",             timeUBO,        spk::Lumina::Shader::Mode::Constant);
        _shader->addUBO    (L"transformUBO",        transformUBO,   spk::Lumina::Shader::Mode::Attribute);
        _shader->addSSBO   (L"nodeCollectionSSBO",  nodeCollection, spk::Lumina::Shader::Mode::Constant);
    }

	return _shader->createObject();
}

BakableChunk::Renderer::Renderer() :
	_object(createObject()),
	_bufferSet(_object.bufferSet()),
	_transformUBO(_object.UBO(L"transformUBO"))
{
	_object.setNbInstance(1);
}

void BakableChunk::Renderer::setTileset(const spk::SafePointer<spk::SpriteSheet> &p_tileset)
{
	_tileset = p_tileset;
	SamplerFactory::tilesetTextureSampler().bind(p_tileset);
}

void BakableChunk::Renderer::render()
{
	_object.render();
}