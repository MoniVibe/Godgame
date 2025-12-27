using Unity.Entities;

namespace Godgame.Runtime.Interaction
{
    /// <summary>
    /// Default hand pickup tuning for runtime-spawned entities.
    /// </summary>
    public struct HandPickableDefaults : IComponentData
    {
        public float Mass;
        public float MaxHoldDistance;
        public float ThrowImpulseMultiplier;
        public float FollowLerp;

        public static HandPickableDefaults Default => new HandPickableDefaults
        {
            Mass = 10f,
            MaxHoldDistance = 12f,
            ThrowImpulseMultiplier = 1f,
            FollowLerp = 0.2f
        };
    }
}
