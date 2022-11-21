#version 450
#extension GL_KHR_vulkan_glsl : enable

layout(set = 0, binding = 0) uniform LightInfo
{
	vec3 LightDirection;
	float padding0;
	vec3 CameraPosition;
	float padding1;
};

layout(set = 1, binding = 0) uniform texture2D Tex;
layout(set = 1, binding = 1) uniform sampler Samp;

layout(set = 2, binding = 0) uniform PbrData
{
    float metallic;
    float roughness;
    float ao;
};

layout(location = 0) in vec3 fsin_Position_WorldSpace;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in vec2 fsin_TexCoord;

layout(location = 0) out vec4 outputColor;

float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);

const float PI = 3.14159265359;

void main() // https://learnopengl.com/PBR/Lighting
{
	vec3 texColor = vec3(texture(sampler2D(Tex, Samp), fsin_TexCoord));

	vec3 N = normalize(fsin_Normal); 
	vec3 V = normalize(fsin_Position_WorldSpace);

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, texColor, metallic);

	// reflectance equation
    vec3 Lo = vec3(0.0);
    // calculate per-light radiance
        
    // cook-torrance brdf
    float NDF = DistributionGGX(N, LightDirection, roughness);        
    float G   = GeometrySmith(N, V, LightDirection, roughness);      
    vec3 F    = fresnelSchlick(max(dot(LightDirection, N), 0.0), F0);       
        
    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;	  
        
    vec3 numerator    = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, LightDirection), 0.0) + 0.0001;
    vec3 specular     = numerator / denominator;  
            
    // add to outgoing radiance Lo
    float NdotL = max(dot(N, LightDirection), 0.0);                
    Lo += (kD * texColor / PI + specular) * NdotL;
  
    vec3 ambient = vec3(0.03) * texColor * ao;
    vec3 color = ambient + Lo;
	
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));  
   
    outputColor = vec4(color, 1.0);
}

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a      = roughness*roughness;
    float a2     = a*a;
    float NdotH  = max(dot(N, H), 0.0);
    float NdotH2 = NdotH*NdotH;
	
    float num   = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r*r) / 8.0;

    float num   = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2  = GeometrySchlickGGX(NdotV, roughness);
    float ggx1  = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}