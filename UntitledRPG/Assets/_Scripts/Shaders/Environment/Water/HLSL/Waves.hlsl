#ifndef WATERWAVES_INLCUDED
#define WATERWAVES_INLCUDED

float3 GerstnerWave(float3 position, float steepness, float wavelength, float speed, float direction, inout float3 tangent, inout float3 binormal) 
{
    direction = direction * 2 - 1;
    float2 dir = normalize(float2(cos(3.14 * direction), sin(3.14 * direction)));
    
    float k = 2 * 3.14 / wavelength;
    float f = k * (dot(dir, position.xz) - speed * _Time.y);
    float a = steepness / k; 

    tangent += float3(
        - dir.x * dir.x * (steepness * sin(f)),
          dir.x *         (steepness * cos(f)),
        - dir.x * dir.y * (steepness * sin(f)));

    binormal += float3(
        - dir.x * dir.y * (steepness * sin(f)),
          dir.y *         (steepness * cos(f)),
        - dir.y * dir.y * (steepness * sin(f)));

    // offset
    return float3(
        dir.x * (a * cos(f)),
                (a * sin(f)),
        dir.y * (a * cos(f)));
}

void GerstnerWaves_float(float3 position, float steepness, float wavelength, float speed, float direction, out float3 offset, out float3 normal)
{    
    float3 tangent  = float3(1, 0, 0); 
    float3 binormal = float3(0, 0, 1);

    offset = 0;
    offset += GerstnerWave(position, steepness, wavelength, speed, 0, tangent, binormal);
    offset += GerstnerWave(position, steepness, wavelength, speed, 0.065, tangent, binormal);
    offset += GerstnerWave(position, steepness, wavelength, speed, 0.860, tangent, binormal);
    // offset += GerstnerWave(position, steepness, wavelength, speed, 0.75, tangent, binormal);

    normal = normalize(cross(binormal, tangent));
}

#endif
