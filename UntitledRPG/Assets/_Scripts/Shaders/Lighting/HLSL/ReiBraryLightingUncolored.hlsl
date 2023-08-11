#ifndef REIBRARY4_LIGHTING_UNCOLORED_INLCUDED
#define REIBRARY4_LIGHTING_UNCOLORED_INLCUDED

#include "ReiBraryLighting.hlsl"

#ifndef SHADERGRAPH_PREVIEW

float3 CalculateLightingForLightUncolored(Light light, ReibraryLightingData data)
{
    float attenuation = light.distanceAttenuation * light.shadowAttenuation;    

    float diffuse = CalculateDiffuse(light, data.normal);    
    float specular = diffuse * CalculateSpecular(light, data.normal, data.viewDirection, data.smoothness);

    return data.albedo * attenuation * (diffuse + specular);
}

float3 CalculateDiffuseForLightUncolored(Light light, ReibraryLightingData data)
{
    float attenuation = light.distanceAttenuation * light.shadowAttenuation;    

    float diffuse = CalculateDiffuse(light, data.normal);    

    return data.albedo * attenuation * diffuse;
}

float3 CalculateSpecularForLightUncolored(Light light, ReibraryLightingData data)
{
    float attenuation = light.distanceAttenuation * light.shadowAttenuation;
    
    float specular = CalculateSpecular(light, data.normal, data.viewDirection, data.smoothness);

    return data.albedo * attenuation * specular;
}

#endif

void CalculateReiBrarySpecularUncolored_float(
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
        
        color += CalculateSpecularForLightUncolored(mainLight, data);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++) 
            {
                Light light = GetAdditionalLight(i, data.position, 1);
                
                color += CalculateSpecularForLightUncolored(light, data);
            }
        #endif        

        color = MixFog(color, data.fogFactor);
        
        Color = color;
    #endif
}

void CalculateReiBraryLightingUncolored_float(
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

        // Calculate shadow mask if baked shadows are on
        data.shadowMask = SAMPLE_SHADOWMASK(lightmapUV);

        // Calculate environment fog 
        data.fogFactor = ComputeFogFactor(positionCS);

        Light mainLight = GetMainLight(data.shadowCoord, data.position, 1);
        
        // Subtract main light from baked lighting
        MixRealtimeAndBakedGI(mainLight, data.normal, data.bakedGI);

        float3 color = CalculateGlobalIllumination(data);
        
        color += CalculateDiffuseForLightUncolored(mainLight, data);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++) 
            {
                Light light = GetAdditionalLight(i, data.position, 1);
                
                color += CalculateDiffuseForLightUncolored(light, data);
            }
        #endif        

        color = MixFog(color, data.fogFactor);
        
        Color = color;
    #endif
}

#endif
