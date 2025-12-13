using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Miracles
{
    /// <summary>
    /// A rain cloud that drifts and emits moisture.
    /// Integrates with wind system for drift behavior.
    /// Used by Rain miracle.
    /// </summary>
    public struct RainCloud : IComponentData
    {
        /// <summary>Total rain capacity (moisture pool).</summary>
        public float MoisturePool;
        
        /// <summary>Moisture emission rate per second.</summary>
        public float EmissionRate;
        
        /// <summary>Current altitude of the cloud.</summary>
        public float Altitude;
        
        /// <summary>Horizontal glide speed override (optional, for throw casts).</summary>
        public float GlideSpeed;
    }
}


















