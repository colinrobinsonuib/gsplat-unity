// Copyright (c) 2026 Colin Robinson
// SPDX-License-Identifier: MIT

#ifndef GSPLAT_EFFECTS_INCLUDED
#define GSPLAT_EFFECTS_INCLUDED

int _GsplatEffectType;
float4 _GsplatEffectData0;
float4 _GsplatEffectData1;
float4 _GsplatEffectData2;
float4 _GsplatEffectData3;
float4 _GsplatEffectData4;

#define GSPLAT_EFFECT_NONE 0
#define GSPLAT_EFFECT_HURRICANE 1

float GsplatEffectHash(uint value)
{
    value ^= value >> 16u;
    value *= 0x7feb352du;
    value ^= value >> 15u;
    value *= 0x846ca68bu;
    value ^= value >> 16u;
    return (value & 0x00ffffffu) / 16777215.0f;
}

float GsplatHurricaneAssemblyProgress(uint splatId)
{
    float progress = saturate(_GsplatEffectData0.w);
    float assemblyStart = saturate(_GsplatEffectData3.x);
    float stagger = saturate(_GsplatEffectData2.y);
    float seed = _GsplatEffectData3.z;
    uint seedBits = asuint(seed + 1.0f);
    float delay = GsplatEffectHash(splatId ^ seedBits) * stagger;
    float assemblyProgress = saturate((progress - assemblyStart) / max(1.0f - assemblyStart, 0.0001f));
    return saturate((assemblyProgress - delay) / max(1.0f - delay, 0.0001f));
}

float GsplatHurricaneAppearance(uint splatId)
{
    float progress = saturate(_GsplatEffectData0.w);
    float fadeInEnd = max(_GsplatEffectData3.y, 0.0001f);
    float stagger = saturate(_GsplatEffectData2.y);
    uint seedBits = asuint(_GsplatEffectData3.z + 17.0f);
    float fadeDelay = GsplatEffectHash(splatId ^ seedBits) * fadeInEnd * stagger;
    float fadeProgress = saturate((progress - fadeDelay) / max(fadeInEnd - fadeDelay, 0.0001f));
    return smoothstep(0.0f, 1.0f, fadeProgress);
}

float3 GsplatHurricanePosition(uint splatId, float3 originalPosition, float localProgress)
{
    float3 effectCenter = _GsplatEffectData0.xyz;
    float elapsedTime = _GsplatEffectData1.x;
    float innerRadius = _GsplatEffectData1.y;
    float outerRadius = max(_GsplatEffectData1.z, innerRadius);
    float angularSpeed = _GsplatEffectData1.w;
    float verticalTurbulence = _GsplatEffectData2.x;
    float verticalSpeed = _GsplatEffectData2.w;
    float stormHalfHeight = max(_GsplatEffectData4.x, 0.1f);
    uint seedBits = asuint(_GsplatEffectData3.z + 1.0f);

    float randomAngle = GsplatEffectHash(splatId * 3u + 11u + seedBits);
    float randomRadius = GsplatEffectHash(splatId * 3u + 23u + seedBits);
    float randomSpeed = GsplatEffectHash(splatId * 3u + 47u + seedBits);
    float randomHeight = GsplatEffectHash(splatId * 3u + 71u + seedBits);

    const float twoPi = 6.283185307179586f;
    float angle = randomAngle * twoPi +
        elapsedTime * angularSpeed * lerp(0.75f, 1.25f, randomSpeed);
    float radius = lerp(innerRadius, outerRadius, randomRadius);
    float verticalNoise = sin(elapsedTime * verticalSpeed + randomAngle * twoPi * 3.0f) *
        verticalTurbulence;

    float3 stormPosition = float3(
        effectCenter.x + cos(angle) * radius,
        effectCenter.y + lerp(-stormHalfHeight, stormHalfHeight, randomHeight) + verticalNoise,
        effectCenter.z + sin(angle) * radius);

    float settle = localProgress * localProgress * (3.0f - 2.0f * localProgress);
    return lerp(stormPosition, originalPosition, settle);
}

void ApplyGsplatEffectPosition(uint splatId, inout float3 position)
{
    if (_GsplatEffectType == GSPLAT_EFFECT_HURRICANE)
    {
        float localProgress = GsplatHurricaneAssemblyProgress(splatId);
        position = GsplatHurricanePosition(splatId, position, localProgress);
    }
}

void ApplyGsplatEffect(uint splatId, inout float3 position, inout float3 scale, inout float4 color)
{
    if (_GsplatEffectType == GSPLAT_EFFECT_HURRICANE)
    {
        float localProgress = GsplatHurricaneAssemblyProgress(splatId);
        position = GsplatHurricanePosition(splatId, position, localProgress);

        float initialScale = max(_GsplatEffectData2.z, 0.0001f);
        float assembled = smoothstep(0.0f, 1.0f, localProgress);
        float appearance = GsplatHurricaneAppearance(splatId);

        scale *= lerp(initialScale, 1.0f, assembled);
        color.a *= appearance;
    }
}

#endif
