using PureDOTS.Runtime.Core;
using PureDOTS.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// Presentation-only expiry hints for perishable resources (stubbed until data is wired).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_ResourceNodePresentationSystem))]
    [UpdateAfter(typeof(Godgame_AggregatePilePresentationEnsureSystem))]
    public partial struct Godgame_ExpiryHintPresentationSystem : ISystem
    {
        private EntityQuery _baseTintQuery;

        public void OnCreate(ref SystemState state)
        {
            _baseTintQuery = SystemAPI.QueryBuilder()
                .WithAll<PresentationExpiryHint, RenderTint>()
                .WithNone<PresentationExpiryBaseTint>()
                .Build();

            state.RequireForUpdate<PresentationExpiryHint>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            if (!_baseTintQuery.IsEmptyIgnoreFilter)
            {
                foreach (var (tint, hint, entity) in SystemAPI
                             .Query<RefRO<RenderTint>, RefRO<PresentationExpiryHint>>()
                             .WithNone<PresentationExpiryBaseTint>()
                             .WithEntityAccess())
                {
                    if (hint.ValueRO.IsActive == 0 || hint.ValueRO.SecondsTotal <= 0f)
                    {
                        continue;
                    }

                    ecb.AddComponent(entity, new PresentationExpiryBaseTint { Value = tint.ValueRO.Value });
                }
            }

            var time = (float)SystemAPI.Time.ElapsedTime;
            foreach (var (hint, tint, baseTint, entity) in SystemAPI
                         .Query<RefRO<PresentationExpiryHint>, RefRW<RenderTint>, RefRW<PresentationExpiryBaseTint>>()
                         .WithEntityAccess())
            {
                if (hint.ValueRO.IsActive == 0 || hint.ValueRO.SecondsTotal <= 0f)
                {
                    tint.ValueRW.Value = baseTint.ValueRO.Value;
                    ecb.RemoveComponent<PresentationExpiryBaseTint>(entity);
                    continue;
                }

                float remaining = math.max(0f, hint.ValueRO.SecondsRemaining);
                float total = math.max(0.01f, hint.ValueRO.SecondsTotal);
                float urgency = 1f - math.saturate(remaining / total);

                float frequency = math.lerp(2f, 8f, urgency);
                float pulse = 0.85f + 0.15f * math.sin(time * frequency + entity.Index * 0.1f);

                float4 baseColor = baseTint.ValueRO.Value;
                float3 warning = new float3(1f, 0.5f, 0.2f);
                float3 tinted = math.lerp(baseColor.xyz, warning, urgency * 0.6f);
                tint.ValueRW.Value = new float4(tinted * pulse, baseColor.w);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
