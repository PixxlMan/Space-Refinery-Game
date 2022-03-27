#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(set = 0, binding = 1) uniform ProjView
{
    mat4 View;
    mat4 Proj;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 3) in vec4 Color;
layout(location = 4) in vec3 InstancePosition;
layout(location = 5) in float InstanceRotationM11;
layout(location = 6) in float InstanceRotationM12;
layout(location = 7) in float InstanceRotationM13;
layout(location = 8) in float InstanceRotationM21;
layout(location = 9) in float InstanceRotationM22;
layout(location = 10) in float InstanceRotationM23;
layout(location = 11) in float InstanceRotationM31;
layout(location = 12) in float InstanceRotationM32;
layout(location = 13) in float InstanceRotationM33;
layout(location = 14) in vec3 InstanceScale;

layout(location = 0) out vec4 color;

void main()
{
    mat3 instanceRotFull = mat3(InstanceRotationM11, InstanceRotationM12, InstanceRotationM13, InstanceRotationM21, InstanceRotationM22, InstanceRotationM23,InstanceRotationM31, InstanceRotationM32, InstanceRotationM33);
    mat3 scalingMat = mat3(InstanceScale.x, 0, 0, 0, InstanceScale.y, 0, 0, 0, InstanceScale.z);

    // Could multiplying with scalingMat after the other matrices fix potential bug with scaling being applied globally instead of locally?
    vec3 transformedPos = (scalingMat * instanceRotFull * Position) + InstancePosition;
    vec4 pos = vec4(transformedPos, 1); 
    gl_Position = Proj * View * pos;

    color = Color;
}
