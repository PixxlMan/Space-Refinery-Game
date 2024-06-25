#version 450

layout(set = 0, binding = 0) uniform InvCameraInfo
{
    mat4 InvProj;
    mat4 InvView;
};

layout(location = 0) in vec4 fsin_ClipPos;
layout(location = 1) in vec3 fsin_TexCoord;

layout(location = 0) out vec4 outputColor;

vec3 sky(vec3 pos)
{
    vec3 color = vec3(max(min(0.7 - pos.y * 2, 0.5), 0.25), 0.6, max(pos.y + 0.5, 0.8));



    return color;
}

void main()
{
    // View Coordinates
    vec4 viewCoords = InvProj * fsin_ClipPos;
    viewCoords.z = -1.0f;
    viewCoords.w = 0.0f;

    vec3 worldDirection = (InvView * viewCoords).xyz;
    worldDirection = normalize(worldDirection);

    worldDirection = floor(worldDirection * 700) / 700;

    outputColor =  vec4(sky(worldDirection), 1.0);
}
