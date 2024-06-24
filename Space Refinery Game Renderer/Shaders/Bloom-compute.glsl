#version 450
#extension GL_EXT_samplerless_texture_functions : enable

layout(set = 0, binding = 0) uniform sampler Samp;
layout (set = 0, binding = 1) uniform texture2D TexColorIn;
layout (set = 0, binding = 2) uniform texture2D TexDepth;
layout (set = 0, binding = 3) uniform writeonly image2D TexColorOut;

// Work group size
layout(local_size_x = 32, local_size_y = 32) in;

void main()
{
    ivec2 pixelPos = ivec2(gl_GlobalInvocationID.xy);
    ivec2 texSize = textureSize(TexColorIn, 0);
    vec2 texPos = vec2(dvec2(pixelPos.xy) / dvec2(texSize));
    vec4 color = texture(sampler2D(TexColorIn, Samp), texPos);
    imageStore(TexColorOut, pixelPos, color);
}
