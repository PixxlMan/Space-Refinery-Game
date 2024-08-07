#version 450

layout(set = 0, binding = 0) uniform LightInfo
{
	vec3 LightDirection;
	float padding0;
	vec3 CameraPosition;
	float padding1;
};

layout(set = 1, binding = 0) uniform sampler Samp;
layout(set = 1, binding = 1) uniform texture2D AlbedoTex;
layout(set = 1, binding = 2) uniform texture2D NormalTex;
layout(set = 1, binding = 3) uniform texture2D MetalTex;
layout(set = 1, binding = 4) uniform texture2D RoughTex;
layout(set = 1, binding = 5) uniform texture2D AOTex;

layout(set = 3, binding = 0) uniform texture2D shadowMap;

layout(location = 0) in vec3 fsin_Position_WorldSpace;
layout(location = 1) in vec3 fsin_Normal;
layout(location = 2) in vec2 fsin_TexCoord;
layout(location = 3) in vec4 fsin_Position_LightSpace;

layout(location = 0) out vec4 outputColor;

float ShadowCalculation(vec4 fragPosLightSpace)
{
	// perform perspective divide
	vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	// transform to [0,1] range
	projCoords = projCoords * 0.5 + 0.5;
	// get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
	float closestDepth = texture(sampler2D(shadowMap, Samp), projCoords.xy).r; 
	// get depth of current fragment from light's perspective
	float currentDepth = projCoords.z;
	// check whether current frag pos is in shadow
	float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

	return shadow;
}  

float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);
vec3 fresnelSchlick(float cosTheta, vec3 F0);

const float PI = 3.14159265359;

// https://learnopengl.com/PBR/Lighting
void main()
{
	vec3 albedo = texture(sampler2D(AlbedoTex, Samp), fsin_TexCoord).xyz;

	vec3 normal = texture(sampler2D(NormalTex, Samp), fsin_TexCoord).rgb;
	normal = normalize(normal * 2.0 - 1.0);

	float metallic = texture(sampler2D(MetalTex, Samp), fsin_TexCoord).r;

	float roughness = texture(sampler2D(RoughTex, Samp), fsin_TexCoord).r;

	float ao = texture(sampler2D(AOTex, Samp), fsin_TexCoord).r;

	float shadow = ShadowCalculation(fsin_Position_LightSpace);

	vec3 N = normalize(fsin_Normal);
	vec3 V = normalize(CameraPosition - fsin_Position_WorldSpace);

	vec3 F0 = vec3(0.04);
	F0 = mix(F0, albedo, metallic);
			   
	// reflectance equation
	vec3 Lo = vec3(0.0);

	// calculate per-light radiance
	vec3 L = LightDirection;
	vec3 H = normalize(V + L);
	vec3 radiance     = vec3(5, 5, 5);
		
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
	Lo += (kD * albedo / PI + specular) * radiance * NdotL; 
  
	vec3 ambient = vec3(0.2) * albedo * ao;
	vec3 color = ambient + (Lo * shadow);
   
	color = color / (color + vec3(1.0)); // Gamma correction
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