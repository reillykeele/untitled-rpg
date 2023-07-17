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

    // bruh
    float attenuationThreshold;
    float shadowDepth;
    float alphaCutoff;
    float darkness;
};

#ifndef SHADERGRAPH_PREVIEW

float CalculateDiffuse(Light light, float3 normal)
{
    return saturate(dot(normal, light.direction));
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

float3 CalculateLightingForLight2(Light light, ReibraryLightingData data) 
{
    // radiance it the lights strength and color 
    float attenuation = light.distanceAttenuation * light.shadowAttenuation;

    float3 radiance = light.color * (attenuation);

    float diffuse = CalculateDiffuse(light, data.normal);    

    return data.albedo * radiance * (diffuse /*+ specular*/);
}

float AShortHikeLightingRamp(float x) 
{
    return lerp(0.5, 1, ceil(x * 4 + 0.25) / 4);
} 

float AShortHikeLightingRamp(float x, float step, float darkness) 
{
    return lerp(darkness, 1, ceil(x * step + 0.25) / step);    
} 

float AShortHikeLightingRamp(float x, float step, float shift, float darkness) 
{
    return lerp(darkness, 1, ceil(x * step + shift) / step);    
} 

#endif

void AShortHikeLightingRamp_float(float x, out float y) 
{    
    #ifdef SHADERGRAPH_PREVIEW
    y = x;
    #else
    y = AShortHikeLightingRamp(x);
    #endif
}

void AShortHikeLightingRamp_float(float x, float step, out float y) 
{    
    #ifdef SHADERGRAPH_PREVIEW
    y = x;
    #else
    y = AShortHikeLightingRamp(x, step, 0, 0);
    #endif
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

        // Color = AShortHikeLightingRamp(color);
        Color = color;
    #endif
}

void ReiBraryLightingRaw2_float(float3 Position, float3 Normal, float3 ViewDirection, float Smoothness, float AmbientOcclusion, float2 LightmapUV, 
    float AttenuationThreshold, float ShadowDepth, float AlphaCutoff, float Darkness,
    out float3 Color)
{
    ReibraryLightingData data;

    data.position = Position;
    data.normal = Normal;
    data.viewDirection = ViewDirection;
    
    data.albedo = 1;
    data.smoothness = Smoothness;
    data.ambientOcclusion = AmbientOcclusion;    

    // bruh
    data.attenuationThreshold = AttenuationThreshold;
    data.shadowDepth = ShadowDepth;
    data.alphaCutoff = AlphaCutoff;
    data.darkness = Darkness;

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
        
        color += CalculateLightingForLight2(mainLight, data);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++) 
            {
                Light light = GetAdditionalLight(i, data.position, 1);
                
                color += CalculateLightingForLight2(light, data);
            }
        #endif        

        color = MixFog(color, data.fogFactor);

        Color = AShortHikeLightingRamp(color, data.shadowDepth, data.darkness);
        // Color = color;
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

#endif
