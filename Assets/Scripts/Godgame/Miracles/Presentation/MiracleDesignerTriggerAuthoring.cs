using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using MiracleType = PureDOTS.Runtime.Components.MiracleType;

namespace Godgame.Miracles.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MiracleDesignerTriggerAuthoring : MonoBehaviour
    {
        [SerializeField] private MiracleVisualProfile profile;
        [SerializeField] private MiracleType miracleType = MiracleType.None; // TODO: Replace Rain with valid PureDOTS miracle type
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;

        private sealed class Baker : Baker<MiracleDesignerTriggerAuthoring>
        {
            public override void Bake(MiracleDesignerTriggerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MiracleDesignerTriggerSource
                {
                    Profile = new UnityObjectRef<MiracleVisualProfile> { Value = authoring.profile },
                    Type = authoring.miracleType,
                    Offset = authoring.spawnOffset
                });

                AddBuffer<MiracleDesignerTrigger>(entity);
            }
        }
    }
}
