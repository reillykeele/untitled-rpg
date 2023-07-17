#ifndef REIBRARY2_LIGHTING_INLCUDED
#define REIBRARY2_LIGHTING_INLCUDED

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
    return pow(saturate(dot(normal, normalize(light.direction + viewDir))), exp2(10 * smoothness + 1));
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
    // radiance it the lights strength and color 
    float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);

    float diffuse = CalculateDiffuse(light, data.normal);
    // float specular = CalculateSpecular(light, data.normal, data.viewDirection, data.smoothness) * diffuse;

    return data.albedo * radiance * (diffuse /*+ specular*/);
}
#endif

void AShortHikeLightingRamp_float(float x, out float y) 
{    
    y = lerp(0.5, 1, ceil(x * 4 + 0.25) / 4);
}

void ReiBraryLightingDiffuse_float(float3 Position, float3 Normal, out float3 Color) 
{
    ReibraryLightingData data;

    data.position = Position;
    data.normal = Normal;    
        
    // data.ambientOcclusion = AmbientOcclusion;    

    #ifdef SHADERGRAPH_PREVIEW
        Color = 0;
        return;
    #else    
        float4 positionCS = TransformWorldToHClip(Position);
        #if SHADOWS_SCREEN
            data.shadowCoord = ComputeScreenPos(positionCS);
        #else
            data.shadowCoord = TransformWorldToShadowCoord(Position);
        #endif

        Light mainLight = GetMainLight(data.shadowCoord, Position, 1);

        float3 radiance = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
        float3 diffuse = 0;
        
        diffuse += radiance * CalculateDiffuse(mainLight, Normal);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++) 
            {
                Light light = GetAdditionalLight(i, Position, 1);
                
                radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                diffuse += radiance * CalculateDiffuse(light, Normal);
            }
        #endif

        Color = diffuse;
    #endif    
}

void ReiBraryLightingSpecular_float(float3 Position, float3 Normal, float3 ViewDirection, float Smoothness, out float3 Color) 
{
    ReibraryLightingData data;

    data.position = Position;
    data.normal = Normal;
    data.viewDirection = ViewDirection;
        
    data.smoothness = Smoothness;
    // data.ambientOcclusion = AmbientOcclusion;    
    
    #ifdef SHADERGRAPH_PREVIEW
        Color = 0;
        return;
    #else    
        float4 positionCS = TransformWorldToHClip(Position);
        #if SHADOWS_SCREEN
            data.shadowCoord = ComputeScreenPos(positionCS);
        #else
            data.shadowCoord = TransformWorldToShadowCoord(Position);
        #endif

        Light mainLight = GetMainLight(data.shadowCoord, Position, 1);

        float3 radiance = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
        float3 specular = 0;
        
        specular += radiance * CalculateSpecular(mainLight, Normal, ViewDirection, Smoothness);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++) 
            {
                Light light = GetAdditionalLight(i, Position, 1);
                
                radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation);
                specular += radiance * CalculateSpecular(light, Normal, ViewDirection, Smoothness);
            }
        #endif

        Color = specular;
    #endif    
}

void ReiBraryLighting_float(float3 Albedo, float3 Position, float3 Normal, float3 ViewDirection, float Smoothness, float AmbientOcclusion, float2 LightmapUV, out float3 Color)
{
    ReibraryLightingData data;

    data.position = Position;
    data.normal = Normal;
    data.viewDirection = ViewDirection;
    
    data.albedo = Albedo;
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

void ReiBraryLightingRaw_float(float3 Position, float3 Normal, float3 ViewDirection, float Smoothness, float AmbientOcclusion, float2 LightmapUV, out float3 Color)
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
