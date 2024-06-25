#version 450
#extension GL_EXT_samplerless_texture_functions : enable

layout (set = 0, binding = 0) uniform sampler Samp;
layout (set = 0, binding = 1) uniform texture2D ThresholdIn;
layout (set = 0, binding = 2) uniform texture2D TexColorIn;
layout (set = 0, binding = 3) uniform writeonly image2D TexColorOut;
layout(set = 0, binding = 4) uniform PassInformation
{
	int Pass;
};

// Work group size
layout(local_size_x = 32, local_size_y = 32) in;

#define Radius 5

const float weights[Radius] = float[](0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

void main()
{
	ivec2 pixelPos = ivec2(gl_GlobalInvocationID.xy);
	vec3 color = texelFetch(sampler2D(ThresholdIn, Samp), pixelPos, 0).xyz;
	vec3 baseColor = texelFetch(sampler2D(TexColorIn, Samp), pixelPos, 0).xyz;

	vec3 result = color.xyz * weights[0]; // Current fragment's contribution

	if (Pass == 0)
	{
		// First pass (horizontal pass)
		for (int i = 1; i < Radius; ++i)
		{
			result += texelFetch(sampler2D(ThresholdIn, Samp), ivec2(pixelPos.x + i, pixelPos.y), 0).xyz * weights[i];
			result += texelFetch(sampler2D(ThresholdIn, Samp), ivec2(pixelPos.x - i, pixelPos.y), 0).xyz * weights[i];
		}

		imageStore(TexColorOut, pixelPos, vec4(result, 1));
	}
	else if (Pass == 1)
	{
		// Second pass (vertical pass)
		for (int i = 1; i < Radius; ++i)
		{
			result += texelFetch(sampler2D(ThresholdIn, Samp), ivec2(pixelPos.x, pixelPos.y + i), 0).xyz * weights[i];
			result += texelFetch(sampler2D(ThresholdIn, Samp), ivec2(pixelPos.x, pixelPos.y - i), 0).xyz * weights[i];
		}

		imageStore(TexColorOut, pixelPos, vec4(baseColor + result * 2, 1));
	}
}