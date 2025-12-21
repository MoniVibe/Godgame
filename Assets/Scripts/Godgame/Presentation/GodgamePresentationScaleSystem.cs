using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_PresentationLayerBootstrapSystem))]
    [UpdateBefore(typeof(Godgame_PresentationScaleSystem))]
    public partial struct Godgame_PresentationScaleRulesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationContentResolved>();
            state.RequireForUpdate<PresentationScaleMultiplier>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless)
            {
                return;
            }

            var config = SystemAPI.TryGetSingleton<PresentationScaleConfig>(out var overrideConfig)
                ? overrideConfig
                : PresentationScaleConfig.Default;

            foreach (var (resolved, layer, scale) in SystemAPI
                         .Query<RefRO<PresentationContentResolved>, RefRO<PresentationLayer>, RefRW<PresentationScaleMultiplier>>())
            {
                if ((resolved.ValueRO.Flags & PresentationContentFlags.HasBaseScale) == 0)
                {
                    continue;
                }

                var target = math.max(0.001f,
                    resolved.ValueRO.BaseScale * ResolveLayerMultiplier(layer.ValueRO.Value, config));
                if (math.abs(scale.ValueRO.Value - target) > 0.0001f)
                {
                    scale.ValueRW.Value = target;
                }
            }
        }

        private static float ResolveLayerMultiplier(PresentationLayerId layer, in PresentationScaleConfig config)
        {
            return layer switch
            {
                PresentationLayerId.Colony => config.ColonyMultiplier,
                PresentationLayerId.Island => config.IslandMultiplier,
                PresentationLayerId.Continent => config.ContinentMultiplier,
                PresentationLayerId.Planet => config.PlanetMultiplier,
                PresentationLayerId.Orbital => config.OrbitalMultiplier,
                PresentationLayerId.System => config.SystemMultiplier,
                PresentationLayerId.Galactic => config.GalacticMultiplier,
                _ => 1f
            };
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_PresentationLODSystem))]
    public partial struct Godgame_PresentationScaleSystem : ISystem
    {
        private EntityQuery _missingPostTransformQuery;

        public void OnCreate(ref SystemState state)
        {
            _missingPostTransformQuery = SystemAPI.QueryBuilder()
                .WithAll<PresentationScaleMultiplier, LocalTransform>()
                .WithNone<PostTransformMatrix>()
                .Build();

            state.RequireForUpdate<PresentationScaleMultiplier>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless)
            {
                return;
            }

            if (!_missingPostTransformQuery.IsEmptyIgnoreFilter)
            {
                var ecb = new EntityCommandBuffer(Allocator.Temp);
                foreach (var (scale, entity) in SystemAPI
                             .Query<RefRO<PresentationScaleMultiplier>>()
                             .WithAll<LocalTransform>()
                             .WithNone<PostTransformMatrix>()
                             .WithEntityAccess())
                {
                    var value = math.max(0.001f, scale.ValueRO.Value);
                    ecb.AddComponent(entity, new PostTransformMatrix { Value = float4x4.Scale(value) });
                }
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }

            foreach (var (scale, postTransform) in SystemAPI
                         .Query<RefRO<PresentationScaleMultiplier>, RefRW<PostTransformMatrix>>()
                         .WithAll<LocalTransform>())
            {
                var value = math.max(0.001f, scale.ValueRO.Value);
                postTransform.ValueRW.Value = float4x4.Scale(value);
            }
        }
    }
}
