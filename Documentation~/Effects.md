# Modular GSplat Effects

Effects deform decoded splat position, scale, and color before projection. The same position
deformation is applied during depth sorting so transparent ordering follows the animated splats.

## Hurricane assembly

1. Add `HurricaneGsplatEffect` beside a `GsplatRenderer`.
2. Assign it to the renderer's `Effect` field. The renderer also discovers a colocated effect
   automatically when it is enabled or validated.
3. Optionally create a reusable profile with
   `Assets > Create > Gsplat > Effects > Hurricane Profile`.
4. Assign the XR head/camera transform to `Viewer`, or leave it empty to capture `Camera.main`.
5. Call `Play()` when the world transition starts.

The viewer position is converted to GSplat local space and captured once. It does not follow the
head after playback begins. Call `SetProgress` for externally driven transitions, or
`CompleteImmediately` to return to the exact undeformed splat.

`Progress` is a deterministic timeline and can be scrubbed in the Inspector. From zero to
`Fade In End`, splats fade into a wide, full-height rotating volume around the captured center.
They remain in that storm until `Assembly Start`, then stagger and fly into their final positions.

Global merged sorting automatically falls back to per-renderer sorting while any effect is active.
It resumes after every active effect completes.

## Adding an effect plugin

1. Add an ID to `GsplatEffectType`.
2. Create a component derived from `GsplatEffect`.
3. Pack the plugin's runtime state into `GsplatEffectShaderData`.
4. Add the matching HLSL branch to `Runtime/Shaders/Effects/GsplatEffects.hlsl`.
5. Use `ApplyGsplatEffectPosition` for positional deformation and `ApplyGsplatEffect` for position,
   scale, and color deformation. The package invokes both hooks automatically.
6. Override `ExpandBounds` when the effect moves splats outside the imported asset bounds.

The fixed shader payload deliberately keeps the renderer independent of individual plugin classes.
Each plugin owns the interpretation of its five `float4` data vectors.
