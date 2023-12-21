#version 450

layout(set = 1, binding = 0) uniform sampler2D tex;

layout(location = 0) in vec2 inTexCoord;
layout(location = 1) in vec4 inColor;

layout(location = 0) out vec4 outColor;

void main()
{
    outColor = texture(tex, inTexCoord) * inColor;
}
