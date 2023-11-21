#version 450
#extension GL_KHR_vulkan_glsl : enable // just to satisfy my plugin for source highlighting

layout(set = 0, binding = 0) uniform LightInfo
{
	vec3 LightDirection;
	float padding0;
	vec3 CameraPosition;
	float padding1;
};

layout(set = 1, binding = 0) uniform sampler Samp;
layout(set = 1, binding = 1) uniform texture2D DiffTex;
layout(set = 1, binding = 2) uniform texture2D MetalTex;
layout(set = 1, binding = 3) uniform texture2D RoughTex;
layout(set = 1, binding = 4) uniform texture2D AOTex;

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
	vec3 texColor = vec3(texture(sampler2D(DiffTex, Samp), fsin_TexCoord));
    float metallic = float(texture(sampler2D(MetalTex, Samp), fsin_TexCoord));
    float roughness = float(texture(sampler2D(RoughTex, Samp), fsin_TexCoord));
    float ao = float(texture(sampler2D(AOTex, Samp), fsin_TexCoord));

    vec3 N = normalize(fsin_Normal);
    vec3 V = normalize(fsin_Position_WorldSpace);

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, texColor, metallic);
	           
    // reflectance equation
    vec3 Lo = vec3(0.0);
    {
        // calculate per-light radiance
        vec3 L = normalize(LightDirection);
        vec3 H = normalize(V + L);
        //float distance    = length(lightPositions[i] - worldPos);
        //float attenuation = 1.0 / (distance * distance);
        vec3 radiance     = vec3(1, 1, 1);//lightColors[i] * attenuation;        
        
        // cook-torrance brdf
        float NDF = DistributionGGX(N, H, roughness);        
        float G   = GeometrySmith(N, V, L, roughness);      
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);       
        
        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;	  
        
        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
        vec3 specular     = numerator / denominator;  
            
        // add to outgoing radiance Lo
        float NdotL = max(dot(N, L), 0.0);                
        Lo += (kD * texColor / PI + specular) * radiance * NdotL; 
    }   
  
    vec3 ambient = vec3(0.03) * texColor * ao;
    vec3 color = ambient + Lo;
	
    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));  

    // Add ambient lighting. Not PBR-based.
    color = max(color, 0.2 * texColor);

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