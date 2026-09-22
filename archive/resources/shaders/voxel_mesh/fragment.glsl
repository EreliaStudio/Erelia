#version 460 core

struct PaletteElement
{
	vec4 color;
};

layout(std430, binding = 2) readonly buffer PaletteData
{
	PaletteElement paletteElements[];
};

layout(location=0) flat in uint vertexPaletteElementIndex;
layout(location=1) in float light;

layout(location=0) out vec4 outColor;

void main()
{
	outColor = paletteElements[vertexPaletteElementIndex].color;
	if (outColor.a <= 0.0)
		discard;
	outColor.rgb *= light;
}
