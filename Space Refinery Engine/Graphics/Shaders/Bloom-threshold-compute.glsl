#version 450
#extension GL_EXT_samplerless_texture_functions : enable

layout (set = 0, binding = 0) uniform sampler Samp;
layout (set = 0, binding = 1) uniform texture2D TexColorIn;
layout (set = 0, binding = 2) uniform writeonly image2D TexColorOut;

#define Threshold 0.5

// Work group size
layout(local_size_x = 32, local_size_y = 32) in;

// Assumes RGBA format
float luminance(vec4 color)
{
	return (0.2126 * color.x + 0.7152 * color.y + 0.0722 * color.z);
}

void main()
{
	ivec2 pixelPos = ivec2(gl_GlobalInvocationID.xy);
	vec4 color = texelFetch(sampler2D(TexColorIn, Samp), pixelPos, 0);

	if (luminance(color) > Threshold)
	{
		imageStore(TexColorOut, pixelPos, vec4(color.x - Threshold, color.y - Threshold, color.z - Threshold, 1));
	}
	else
	{
		imageStore(TexColorOut, pixelPos, vec4(0, 0, 0, 0));
	}
}
