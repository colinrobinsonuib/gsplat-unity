// Copyright (c) 2026 Colin Robinson
// SPDX-License-Identifier: MIT

using UnityEngine;
using UnityEngine.Rendering;

namespace Gsplat
{
    public enum GsplatEffectType
    {
        None = 0,
        Hurricane = 1,
    }

    /// <summary>
    /// Fixed-size payload shared by the draw and depth-sort shaders. New effect plugins can
    /// assign a new type and interpret the six data vectors in their own HLSL implementation.
    /// </summary>
    public readonly struct GsplatEffectShaderData
    {
        public readonly int Type;
        public readonly Vector4 Data0;
        public readonly Vector4 Data1;
        public readonly Vector4 Data2;
        public readonly Vector4 Data3;
        public readonly Vector4 Data4;
        public readonly Vector4 Data5;

        static readonly int k_effectType = Shader.PropertyToID("_GsplatEffectType");
        static readonly int k_effectData0 = Shader.PropertyToID("_GsplatEffectData0");
        static readonly int k_effectData1 = Shader.PropertyToID("_GsplatEffectData1");
        static readonly int k_effectData2 = Shader.PropertyToID("_GsplatEffectData2");
        static readonly int k_effectData3 = Shader.PropertyToID("_GsplatEffectData3");
        static readonly int k_effectData4 = Shader.PropertyToID("_GsplatEffectData4");
        static readonly int k_effectData5 = Shader.PropertyToID("_GsplatEffectData5");

        public GsplatEffectShaderData(GsplatEffectType type, Vector4 data0, Vector4 data1,
            Vector4 data2, Vector4 data3, Vector4 data4 = default, Vector4 data5 = default)
        {
            Type = (int)type;
            Data0 = data0;
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            Data4 = data4;
            Data5 = data5;
        }

        public bool Active => Type != (int)GsplatEffectType.None;

        public void Apply(MaterialPropertyBlock propertyBlock)
        {
            propertyBlock.SetInteger(k_effectType, Type);
            propertyBlock.SetVector(k_effectData0, Data0);
            propertyBlock.SetVector(k_effectData1, Data1);
            propertyBlock.SetVector(k_effectData2, Data2);
            propertyBlock.SetVector(k_effectData3, Data3);
            propertyBlock.SetVector(k_effectData4, Data4);
            propertyBlock.SetVector(k_effectData5, Data5);
        }

        public void Apply(CommandBuffer cmd, ComputeShader computeShader)
        {
            cmd.SetComputeIntParam(computeShader, k_effectType, Type);
            cmd.SetComputeVectorParam(computeShader, k_effectData0, Data0);
            cmd.SetComputeVectorParam(computeShader, k_effectData1, Data1);
            cmd.SetComputeVectorParam(computeShader, k_effectData2, Data2);
            cmd.SetComputeVectorParam(computeShader, k_effectData3, Data3);
            cmd.SetComputeVectorParam(computeShader, k_effectData4, Data4);
            cmd.SetComputeVectorParam(computeShader, k_effectData5, Data5);
        }
    }

    /// <summary>
    /// Base class for modular per-splat effects. Place an effect beside a GsplatRenderer and
    /// assign it to the renderer's Effect field.
    /// </summary>
    public abstract class GsplatEffect : MonoBehaviour
    {
        public abstract bool IsActive { get; }
        public abstract GsplatEffectShaderData ShaderData { get; }

        /// <summary>
        /// Returns local-space bounds large enough for all deformed splats.
        /// </summary>
        public virtual Bounds ExpandBounds(Bounds sourceBounds) => sourceBounds;
    }
}
