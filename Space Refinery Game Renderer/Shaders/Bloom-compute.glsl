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
    vec4 color = texelFetch(sampler2D(TexColorIn, Samp), pixelPos, 0);
    imageStore(TexColorOut, pixelPos, color);
}
