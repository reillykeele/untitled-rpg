#ifndef REIBRARY4_LIGHTING_INLCUDED
#define REIBRARY4_LIGHTING_INLCUDED

struct ReibraryLightingData
{
    float3 position;
    float3 normal;
    float3 viewDirection;
    float4 shadowCoord;

    float3 albedo;
    float smoothness;
    float ambientOcclusion;    

    float3 bakedGI;
    float4 shadowMask;
    float fogFactor;
};

#ifndef SHADERGRAPH_PREVIEW

float CalculateDiffuse(Light light, float3 normal)
{
    return saturate(dot(normal, light.direction));
}

float CalculateSpecular(Light light, float3 normal, float3 viewDir, float smoothness)
{
    float3 H = normalize(light.direction + viewDir);
    float NdotH = saturate(dot(normal, H));
    return pow(NdotH, exp2(10 * smoothness + 1));

    // return pow(saturate(dot(normal, normalize(light.direction + viewDir))), exp2(10 * smoothness + 1));    
}

float3 CalculateGlobalIllumination(ReibraryLightingData data) 
{
    float3 indirectDiffuse = data.albedo * data.bakedGI * data.ambientOcclusion;    

    // makes reflections stronger along the edge
    float fresnel = Pow4(1 - saturate(dot(data.viewDirection, data.normal)));

    // samples baked cubemap reflections
    float3 indirectSpecular = GlossyEnvironmentReflection(
        reflect(-data.viewDirection, data.normal), // reflect vector
        RoughnessToPerceptualRoughness(1 - data.smoothness),
        data.ambientOcclusion * fresnel);

    return indirectDiffuse + indirectSpecular;
}

float3 CalculateLightingForLight(Light light, ReibraryLightingData data)
{
    // radiance is the lights strength and color 
    float attenuation = light.distanceAttenuation * light.shadowAttenuation;

    float3 radiance = light.color * (attenuation);

    float diffuse = CalculateDiffuse(light, data.normal);
    float specular = CalculateSpecular(light, data.normal, data.viewDirection, data.smoothness);

    return data.albedo * radiance * (diffuse /*+ specular*/);
}

float3 CalculateDiffuseForLight(Light light, ReibraryLightingData data)
{
    // radiance is the lights strength and color 
    float attenuation = light.distanceAttenuation * light.shadowAttenuation;

    float3 radiance = light.color * (attenuation);

    float diffuse = CalculateDiffuse(light, data.normal);    

    return data.albedo * radiance * diffuse;
}

float3 CalculateSpecularForLight(Light light, ReibraryLightingData data)
{
    // radiance is the lights strength and color 
    float attenuation = light.distanceAttenuation * light.shadowAttenuation;

    float3 radiance = light.color * (attenuation);
    
    float specular = CalculateSpecular(light, data.normal, data.viewDirection, data.smoothness);

    return data.albedo * radiance * specular;
}

#endif

void GetMainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
    #if SHADERGRAPH_PREVIEW
        Direction = float3(0.5, 0.5, 0);
        Color = 1;
        DistanceAtten = 1;
        ShadowAtten = 1;
    #else
        #if SHADOWS_SCREEN
            float4 clipPos = TransformWorldToHClip(WorldPos);
            float4 shadowCoord = ComputeScreenPos(clipPos);
        #else
            float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif

        Light mainLight = GetMainLight(shadowCoord);

        Direction = mainLight.direction;
        Color = mainLight.color;
        DistanceAtten = mainLight.distanceAttenuation;
        ShadowAtten = mainLight.shadowAttenuation;
    #endif
}

void CalculateReiBrarySpecular_float(
    float3 Position, 
    float3 Normal, 
    float3 ViewDirection, 
    float Smoothness, 
    float AmbientOcclusion, 
    float2 LightmapUV,    
    out float3 Color)
{
    ReibraryLightingData data;

    data.position = Position;
    data.normal = Normal;
    data.viewDirection = ViewDirection;
    
    data.albedo = 1;
    data.smoothness = Smoothness;
    data.ambientOcclusion = AmbientOcclusion;    

    #ifdef SHADERGRAPH_PREVIEW
        Color = data.albedo;
        return;
    #else                
        float4 positionCS = TransformWorldToHClip(Position);
        #if SHADOWS_SCREEN
            data.shadowCoord = ComputeScreenPos(positionCS);
        #else
            data.shadowCoord = TransformWorldToShadowCoord(Position);
        #endif

        float2 lightmapUV;
        OUTPUT_LIGHTMAP_UV(LightmapUV, unity_LightmapST, lightmapUV);

        float3 vertexSH;
        OUTPUT_SH(Normal, vertexSH);
        
        // Calculate baked lighting from light maps / probes
        data.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, Normal);

        // Calculate shadow mask if basked shadows are on
        data.shadowMask = SAMPLE_SHADOWMASK(lightmapUV);

        // Calculate environment fog 
        data.fogFactor = ComputeFogFactor(positionCS);

        Light mainLight = GetMainLight(data.shadowCoord, data.position, 1);
        
        // Subtract main light from baked lighting
        MixRealtimeAndBakedGI(mainLight, data.normal, data.bakedGI);

        float3 color = CalculateGlobalIllumination(data);
        
        color += CalculateSpecularForLight(mainLight, data);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++) 
            {
                Light light = GetAdditionalLight(i, data.position, 1);
                
                color += CalculateSpecularForLight(light, data);
            }
        #endif        

        color = MixFog(color, data.fogFactor);
        
        Color = color;
    #endif
}

void CalculateReiBraryLighting_float(
    float3 Position, 
    float3 Normal, 
    float3 ViewDirection, 
    float Smoothness, 
    float AmbientOcclusion, 
    float2 LightmapUV,    
    out float3 Color)
{
    ReibraryLightingData data;

    data.position = Position;
    data.normal = Normal;
    data.viewDirection = ViewDirection;
    
    data.albedo = 1;
    data.smoothness = Smoothness;
    data.ambientOcclusion = AmbientOcclusion;    

    #ifdef SHADERGRAPH_PREVIEW
        Color = data.albedo;
        return;
    #else                
        float4 positionCS = TransformWorldToHClip(Position);
        #if SHADOWS_SCREEN
            data.shadowCoord = ComputeScreenPos(positionCS);
        #else
            data.shadowCoord = TransformWorldToShadowCoord(Position);
        #endif

        float2 lightmapUV;
        OUTPUT_LIGHTMAP_UV(LightmapUV, unity_LightmapST, lightmapUV);

        float3 vertexSH;
        OUTPUT_SH(Normal, vertexSH);
        
        // Calculate baked lighting from light maps / probes
        data.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, Normal);

        // Calculate shadow mask if basked shadows are on
        data.shadowMask = SAMPLE_SHADOWMASK(lightmapUV);

        // Calculate environment fog 
        data.fogFactor = ComputeFogFactor(positionCS);

        Light mainLight = GetMainLight(data.shadowCoord, data.position, 1);
        
        // Subtract main light from baked lighting
        MixRealtimeAndBakedGI(mainLight, data.normal, data.bakedGI);

        float3 color = CalculateGlobalIllumination(data);
        
        color += CalculateLightingForLight(mainLight, data);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++) 
            {
                Light light = GetAdditionalLight(i, data.position, 1);
                
                color += CalculateLightingForLight(light, data);
            }
        #endif        

        color = MixFog(color, data.fogFactor);
        
        Color = color;
    #endif
}

#endif
