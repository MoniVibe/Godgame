using Godgame.Runtime.Interaction;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class HandPickableDefaultsAuthoring : MonoBehaviour
    {
        [Header("Hand Pickable Defaults")]
        public float mass = 10f;
        public float maxHoldDistance = 12f;
        public float throwImpulseMultiplier = 1f;
        [Range(0.01f, 1f)]
        public float followLerp = 0.2f;

        private sealed class Baker : Baker<HandPickableDefaultsAuthoring>
        {
            public override void Bake(HandPickableDefaultsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new HandPickableDefaults
                {
                    Mass = Mathf.Max(0.1f, authoring.mass),
                    MaxHoldDistance = Mathf.Max(0.1f, authoring.maxHoldDistance),
                    ThrowImpulseMultiplier = Mathf.Max(0.1f, authoring.throwImpulseMultiplier),
                    FollowLerp = Mathf.Clamp(authoring.followLerp, 0.01f, 1f)
                });
            }
        }
    }
}
