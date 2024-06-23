#version 450

layout (set = 0, binding = 0) uniform texture2D TexColorIn;
layout (set = 0, binding = 1) uniform writeonly image2D TexColorOut;
layout (set = 0, binding = 2) uniform texture2D TexDepth;
layout(set = 0, binding = 3) uniform sampler Samp;

// Work group size
layout(local_size_x = 32, local_size_y = 32) in;

void main()
{
    ivec2 pixelPos = ivec2(gl_GlobalInvocationID.xy);
    vec4 color = vec4(.5, .5, .5, 1);//texture(sampler2D(TexColor, Samp), pixelPos) * 0.5;
    imageStore(TexColorOut, pixelPos, color);
}
