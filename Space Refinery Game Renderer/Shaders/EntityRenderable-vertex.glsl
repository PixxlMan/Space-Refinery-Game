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
layout(location = 3) in vec3 InstancePosition;
layout(location = 4) in vec3 InstanceRotation;
layout(location = 5) in vec3 InstanceScale;

layout(location = 0) out vec3 fsin_Position_WorldSpace;
layout(location = 1) out vec3 fsin_Normal;
layout(location = 2) out vec3 fsin_TexCoord;

void main()
{
    float cosX = cos(InstanceRotation.x);
    float sinX = sin(InstanceRotation.x);
    mat3 instanceRotX = mat3(
        1, 0, 0,
        0, cosX, -sinX,
        0, sinX, cosX);

    float cosY = cos(InstanceRotation.y);
    float sinY = sin(InstanceRotation.y);
    mat3 instanceRotY = mat3(
        cosY, 0, sinY,
        0, 1, 0,
        -sinY, 0, cosY);

    float cosZ = cos(InstanceRotation.z);
    float sinZ = sin(InstanceRotation.z);
    mat3 instanceRotZ =mat3(
        cosZ, -sinZ, 0,
        sinZ, cosZ, 0,
        0, 0, 1);

    mat3 instanceRotFull = instanceRotZ * instanceRotY * instanceRotZ;
    mat3 scalingMat = mat3(InstanceScale.x, 0, 0, 0, InstanceScale.y, 0, 0, 0, InstanceScale.z);

    vec3 transformedPos = (scalingMat * instanceRotFull * Position) + InstancePosition;
    vec4 pos = vec4(transformedPos, 1);
    fsin_Position_WorldSpace = transformedPos;
    gl_Position = Proj * View * pos;
    fsin_Normal = normalize(instanceRotFull * Normal);
    fsin_TexCoord = vec3(TexCoord, 0/*InstanceTexArrayIndex*/);
}
