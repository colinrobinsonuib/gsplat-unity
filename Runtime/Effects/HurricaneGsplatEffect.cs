// Copyright (c) 2026 Colin Robinson
// SPDX-License-Identifier: MIT

using UnityEngine;

namespace Gsplat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GsplatRenderer))]
    public sealed class HurricaneGsplatEffect : GsplatEffect
    {
        public HurricaneGsplatProfile Profile;
        [Tooltip("Captured when Play is called. Falls back to Camera.main when left empty.")]
        public Transform Viewer;
        public bool PlayOnEnable;

        [SerializeField, Range(0.0f, 1.0f)] float m_progress = 1.0f;
        [SerializeField, HideInInspector] Vector3 m_effectCenterLocal;

        GsplatRenderer m_renderer;
        float m_elapsedTime;
        bool m_isPlaying;

        const float k_defaultDuration = 6.5f;
        const float k_defaultInnerRadius = 1.75f;
        const float k_defaultOuterRadius = 7.0f;
        const float k_defaultRotationsPerSecond = 0.65f;
        const float k_defaultVerticalTurbulence = 0.5f;
        const float k_defaultStormHalfHeight = 3.0f;
        const float k_defaultVerticalCyclesPerSecond = 0.3f;
        const float k_defaultStagger = 0.35f;
        const float k_defaultInitialScale = 0.07f;
        const float k_defaultAssemblyStart = 0.5f;
        const float k_defaultFadeInEnd = 0.5f;

        float Duration => Profile ? Profile.Duration : k_defaultDuration;
        float InnerRadius => Profile ? Profile.InnerRadius : k_defaultInnerRadius;
        float OuterRadius => Profile ? Mathf.Max(Profile.OuterRadius, Profile.InnerRadius) : k_defaultOuterRadius;
        float RotationsPerSecond => Profile ? Profile.RotationsPerSecond : k_defaultRotationsPerSecond;
        float VerticalTurbulence => Profile ? Profile.VerticalTurbulence : k_defaultVerticalTurbulence;
        float StormHalfHeight => Profile ? Profile.StormHalfHeight : k_defaultStormHalfHeight;
        float VerticalCyclesPerSecond =>
            Profile ? Profile.VerticalCyclesPerSecond : k_defaultVerticalCyclesPerSecond;
        float Stagger => Profile ? Profile.Stagger : k_defaultStagger;
        float InitialScale => Profile ? Profile.InitialScale : k_defaultInitialScale;
        float AssemblyStart => Profile ? Profile.AssemblyStart : k_defaultAssemblyStart;
        float FadeInEnd => Profile ? Mathf.Min(Profile.FadeInEnd, Profile.AssemblyStart) : k_defaultFadeInEnd;
        float Seed => Profile ? Profile.Seed : 0.0f;

        public float Progress => m_progress;
        public bool IsPlaying => m_isPlaying;
        public override bool IsActive => isActiveAndEnabled && m_progress < 1.0f;

        public override GsplatEffectShaderData ShaderData
        {
            get
            {
                if (!IsActive)
                    return default;

                // Progress is the authoritative timeline. This keeps Inspector/Timeline scrubbing
                // deterministic instead of leaving the orbit frozen at the last elapsed time.
                var timelineTime = m_progress * Mathf.Max(Duration, 0.1f);
                return new GsplatEffectShaderData(
                    GsplatEffectType.Hurricane,
                    new Vector4(m_effectCenterLocal.x, m_effectCenterLocal.y, m_effectCenterLocal.z, m_progress),
                    new Vector4(timelineTime, InnerRadius, OuterRadius,
                        RotationsPerSecond * Mathf.PI * 2.0f),
                    new Vector4(VerticalTurbulence, Stagger, InitialScale,
                        VerticalCyclesPerSecond * Mathf.PI * 2.0f),
                    new Vector4(AssemblyStart, FadeInEnd, Seed, 0.0f),
                    new Vector4(StormHalfHeight, 0.0f, 0.0f, 0.0f));
            }
        }

        void Awake()
        {
            m_renderer = GetComponent<GsplatRenderer>();
        }

        void OnEnable()
        {
            if (!m_renderer)
                m_renderer = GetComponent<GsplatRenderer>();

            if (Application.isPlaying && PlayOnEnable)
                Play();
        }

        void OnValidate()
        {
            m_progress = Mathf.Clamp01(m_progress);
            m_elapsedTime = m_progress * Mathf.Max(Duration, 0.1f);

            if (!m_renderer)
                m_renderer = GetComponent<GsplatRenderer>();

            // Make direct Inspector scrubbing use the currently assigned center transform.
            if (Viewer)
                m_effectCenterLocal = transform.InverseTransformPoint(Viewer.position);
            else if (!Application.isPlaying)
                m_effectCenterLocal = Vector3.zero;

            m_renderer?.ForceRefresh();
        }

        void Update()
        {
            if (!Application.isPlaying || !m_isPlaying)
                return;

            m_elapsedTime += Time.deltaTime;
            m_progress = Mathf.Clamp01(m_elapsedTime / Mathf.Max(Duration, 0.1f));
            m_renderer?.ForceRefresh();

            if (m_progress >= 1.0f)
                m_isPlaying = false;
        }

        [ContextMenu("Play")]
        public void Play()
        {
            var viewer = Viewer;
            if (!viewer && Camera.main)
                viewer = Camera.main.transform;
            Play(viewer);
        }

        public void Play(Transform viewer)
        {
            if (!m_renderer)
                m_renderer = GetComponent<GsplatRenderer>();

            CaptureCenter(viewer, true);

            m_elapsedTime = 0.0f;
            m_progress = 0.0f;
            m_isPlaying = true;
            m_renderer?.ForceRefresh();
        }

        public void SetProgress(float progress)
        {
            var wasInactive = m_progress >= 1.0f;
            m_progress = Mathf.Clamp01(progress);
            m_elapsedTime = m_progress * Mathf.Max(Duration, 0.1f);
            m_isPlaying = false;

            if (wasInactive && m_progress < 1.0f)
                CaptureCenter(Viewer, false);

            m_renderer?.ForceRefresh();
        }

        [ContextMenu("Complete Immediately")]
        public void CompleteImmediately()
        {
            SetProgress(1.0f);
        }

        public override Bounds ExpandBounds(Bounds sourceBounds)
        {
            if (!IsActive)
                return sourceBounds;

            var minY = Mathf.Min(sourceBounds.min.y,
                m_effectCenterLocal.y - StormHalfHeight - VerticalTurbulence);
            var maxY = Mathf.Max(sourceBounds.max.y,
                m_effectCenterLocal.y + StormHalfHeight + VerticalTurbulence);
            var stormBounds = new Bounds(
                new Vector3(m_effectCenterLocal.x, (minY + maxY) * 0.5f, m_effectCenterLocal.z),
                new Vector3(OuterRadius * 2.0f, maxY - minY, OuterRadius * 2.0f));
            sourceBounds.Encapsulate(stormBounds);
            return sourceBounds;
        }

        void CaptureCenter(Transform viewer, bool warnIfMissing)
        {
            if (viewer)
            {
                m_effectCenterLocal = transform.InverseTransformPoint(viewer.position);
                return;
            }

            m_effectCenterLocal = Vector3.zero;
            if (warnIfMissing)
                Debug.LogWarning(
                    $"[{nameof(HurricaneGsplatEffect)}] No Viewer or Main Camera was available; using the GSplat origin.",
                    this);
        }
    }
}
