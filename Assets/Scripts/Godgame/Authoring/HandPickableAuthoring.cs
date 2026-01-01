using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Interaction;
using Unity.Entities;
using UnityEngine;
using Pickable = PureDOTS.Runtime.Interaction.Pickable;

namespace Godgame.Authoring
{
    [DisallowMultipleComponent]
    public sealed class HandPickableAuthoring : MonoBehaviour
    {
        [Header("Hand Pickable")]
        public float mass = 1f;
        public float maxHoldDistance = 12f;
        public float throwImpulseMultiplier = 1f;
        [Range(0.01f, 1f)] public float followLerp = 0.25f;

        private sealed class Baker : Baker<HandPickableAuthoring>
        {
            public override void Bake(HandPickableAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HandPickable
                {
                    Mass = Mathf.Max(0.01f, authoring.mass),
                    MaxHoldDistance = Mathf.Max(0.1f, authoring.maxHoldDistance),
                    ThrowImpulseMultiplier = Mathf.Max(0.01f, authoring.throwImpulseMultiplier),
                    FollowLerp = Mathf.Clamp(authoring.followLerp, 0.01f, 1f)
                });
                AddComponent<Pickable>(entity);
                AddComponent<HeldByPlayer>(entity);
                SetComponentEnabled<HeldByPlayer>(entity, false);
                AddComponent<MovementSuppressed>(entity);
                SetComponentEnabled<MovementSuppressed>(entity, false);
                AddComponent<BeingThrown>(entity);
                SetComponentEnabled<BeingThrown>(entity, false);
            }
        }
    }
}
