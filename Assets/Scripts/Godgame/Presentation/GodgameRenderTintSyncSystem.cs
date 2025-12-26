using PureDOTS.Runtime.Core;
using PureDOTS.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Godgame.Presentation
{
    /// <summary>
    /// Drives URP base/emission colors from RenderTint so presentation palettes are visible.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_RenderTintSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (tint, entity) in SystemAPI
                         .Query<RefRO<RenderTint>>()
                         .WithNone<URPMaterialPropertyBaseColor>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new URPMaterialPropertyBaseColor
                {
                    Value = tint.ValueRO.Value
                });
            }

            foreach (var (tint, entity) in SystemAPI
                         .Query<RefRO<RenderTint>>()
                         .WithNone<URPMaterialPropertyEmissionColor>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new URPMaterialPropertyEmissionColor
                {
                    Value = new float4(tint.ValueRO.Value.xyz * 0.75f, 1f)
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            foreach (var (tint, baseColor, emission) in SystemAPI
                         .Query<RefRO<RenderTint>, RefRW<URPMaterialPropertyBaseColor>, RefRW<URPMaterialPropertyEmissionColor>>()
                         .WithChangeFilter<RenderTint>())
            {
                var color = tint.ValueRO.Value;
                baseColor.ValueRW.Value = color;
                emission.ValueRW.Value = new float4(color.xyz * 0.75f, 1f);
            }
        }
    }
}
