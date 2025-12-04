using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Miracles
{
    /// <summary>
    /// Local time distortion bubble.
    /// Used by Temporal Veil miracle.
    /// </summary>
    public struct TimeDistortion : IComponentData
    {
        /// <summary>Center position of the distortion bubble.</summary>
        public float3 Center;
        
        /// <summary>Radius of the bubble.</summary>
        public float Radius;
        
        /// <summary>Time scale multiplier (0.25-2.0, where 1.0 = normal, <1.0 = slow, >1.0 = fast).</summary>
        public float TimeScale;
        
        /// <summary>Distortion mode (0=Slow, 1=Haste, 2=Stasis, 3=Rewind future).</summary>
        public byte Mode;
    }
    
    /// <summary>
    /// Per-entity local time scale multiplier.
    /// Applied by TimeDistortionApplySystem to entities inside time bubbles.
    /// </summary>
    public struct LocalTimeScale : IComponentData
    {
        /// <summary>Time scale value (1.0 = normal, <1.0 = slow, >1.0 = fast).</summary>
        public float Value;
    }
    
    /// <summary>
    /// Temporal lashback risk tracker for god/player.
    /// Tracks risk accumulation and active penalties from Temporal Veil usage.
    /// </summary>
    public struct TemporalLashback : IComponentData
    {
        /// <summary>Risk level (0-1, accumulates with aggressive use).</summary>
        public float Risk;
        
        /// <summary>Whether lashback is currently active (0/1, stunned/penalized).</summary>
        public byte Active;
    }
}

