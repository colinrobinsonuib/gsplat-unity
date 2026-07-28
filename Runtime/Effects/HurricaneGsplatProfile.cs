// Copyright (c) 2026 Colin Robinson
// SPDX-License-Identifier: MIT

using UnityEngine;

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
        [Min(0.0f)] public float VerticalCyclesPerSecond = 0.3f;
        [Range(0.0f, 0.9f)] public float Stagger = 0.35f;
        [Range(0.001f, 1.0f)] public float InitialScale = 0.07f;
        [Range(0.0f, 0.95f)] public float SettleStart = 0.4f;
        [Range(0.01f, 0.5f)] public float FadeInFraction = 0.18f;
        public float Seed;

        void OnValidate()
        {
            OuterRadius = Mathf.Max(OuterRadius, InnerRadius);
        }
    }
}
