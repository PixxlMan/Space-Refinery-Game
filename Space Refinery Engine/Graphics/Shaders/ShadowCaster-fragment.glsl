#version 450

layout(set = 0, binding = 0) uniform LightInfo
{
	vec3 LightDirection;
	float padding0;
	vec3 CameraPosition;
	float padding1;
};

layout(location = 0) in vec3 fsin_Position_WorldSpace;

layout(location = 0) out float outputColor;

void main() // https://learnopengl.com/PBR/Lighting
{
}
