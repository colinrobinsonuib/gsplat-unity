// Copyright (c) 2026 Colin Robinson
// SPDX-License-Identifier: MIT

using UnityEngine;
using UnityEngine.Serialization;

namespace Gsplat
{
    [CreateAssetMenu(menuName = "Gsplat/Effects/Hurricane Profile")]
    public sealed class HurricaneGsplatProfile : ScriptableObject
    {
        [Min(0.1f)] public float Duration = 6.5f;
        [Min(0.05f)] public float InnerRadius = 1.75f;
        [Min(0.1f)] public float OuterRadius = 7.0f;
        [Min(0.0f)] public float RotationsPerSecond = 0.65f;
        [Min(0.0f)] public float VerticalTurbulence = 0.5f;
        [Min(0.1f)] public float StormHalfHeight = 3.0f;
        [Min(0.0f)] public float VerticalCyclesPerSecond = 0.3f;
        [Range(0.0f, 0.9f)] public float Stagger = 0.35f;
        [Range(0.001f, 1.0f)] public float InitialScale = 0.07f;
        [FormerlySerializedAs("SettleStart")]
        [Range(0.05f, 0.95f)] public float AssemblyStart = 0.5f;
        [FormerlySerializedAs("FadeInFraction")]
        [Range(0.05f, 0.95f)] public float FadeInEnd = 0.5f;
        [Range(0.05f, 0.95f)] public float CloudSettleEnd = 0.65f;
        [Range(0.05f, 0.99f)] public float RippleStart = 0.75f;
        [Range(0.001f, 1.0f)] public float CloudDensity = 0.12f;
        [Min(0.0f)] public float WobbleAmplitude = 0.08f;
        [Min(0.1f)] public float RippleRadius = 10.0f;
        [Min(0.01f)] public float RippleWidth = 0.75f;
        [Min(0.0f)] public float RippleGlow = 0.75f;
        public float Seed;

        void OnValidate()
        {
            OuterRadius = Mathf.Max(OuterRadius, InnerRadius);
            FadeInEnd = Mathf.Min(FadeInEnd, AssemblyStart);
            CloudSettleEnd = Mathf.Max(CloudSettleEnd, AssemblyStart);
            RippleStart = Mathf.Max(RippleStart, CloudSettleEnd);
        }
    }
}
