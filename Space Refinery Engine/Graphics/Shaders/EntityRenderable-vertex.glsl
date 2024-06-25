#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(set = 0, binding = 1) uniform ProjView
{
	mat4 Proj;
	mat4 View;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 3) in vec3 InstancePosition;
layout(location = 4) in float InstanceRotationM11;
layout(location = 5) in float InstanceRotationM12;
layout(location = 6) in float InstanceRotationM13;
layout(location = 7) in float InstanceRotationM21;
layout(location = 8) in float InstanceRotationM22;
layout(location = 9) in float InstanceRotationM23;
layout(location = 10) in float InstanceRotationM31;
layout(location = 11) in float InstanceRotationM32;
layout(location = 12) in float InstanceRotationM33;

layout(location = 0) out vec3 fsin_Position_WorldSpace;
layout(location = 1) out vec3 fsin_Normal;
layout(location = 2) out vec2 fsin_TexCoord;

void main()
{
	mat3 instanceRotFull = mat3(InstanceRotationM11, InstanceRotationM12, InstanceRotationM13, InstanceRotationM21, InstanceRotationM22, InstanceRotationM23,InstanceRotationM31, InstanceRotationM32, InstanceRotationM33);

	vec3 transformedPos = (instanceRotFull * Position) + InstancePosition;
	vec4 pos = vec4(transformedPos, 1);
	fsin_Position_WorldSpace = transformedPos;
	gl_Position = View * Proj * pos;
	fsin_Normal = normalize(instanceRotFull * Normal);
	fsin_TexCoord = vec2(TexCoord);//, 0/*InstanceTexArrayIndex*/);
}
