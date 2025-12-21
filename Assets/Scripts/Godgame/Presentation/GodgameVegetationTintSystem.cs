using Godgame.Environment.Vegetation;
using PureDOTS.Runtime.Core;
using PureDOTS.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// Updates vegetation tint based on health and growth stage.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VegetationPresentationSystem))]
    public partial struct Godgame_VegetationTintSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (RuntimeMode.IsHeadless)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (visual, entity) in SystemAPI
                         .Query<RefRO<VegetationVisualState>>()
                         .WithAll<VegetationPresentationTag>()
                         .WithEntityAccess())
            {
                float health = math.saturate(visual.ValueRO.Health);
                var healthy = new float4(0.2f, 0.7f, 0.2f, 1f);
                var stressed = new float4(0.6f, 0.4f, 0.2f, 1f);
                var dead = new float4(0.25f, 0.25f, 0.25f, 1f);

                var tint = math.lerp(stressed, healthy, health);

                if (visual.ValueRO.GrowthStage == (byte)GrowthStage.Seedling)
                {
                    tint = math.lerp(tint, new float4(0.5f, 0.85f, 0.5f, 1f), 0.35f);
                }
                else if (visual.ValueRO.GrowthStage == (byte)GrowthStage.Dead || health <= 0.01f)
                {
                    tint = dead;
                }

                tint.xyz *= math.max(0.01f, visual.ValueRO.BiomeTint.xyz);
                tint.w = 1f;

                if (SystemAPI.HasComponent<RenderTint>(entity))
                {
                    ecb.SetComponent(entity, new RenderTint { Value = tint });
                }
                else
                {
                    ecb.AddComponent(entity, new RenderTint { Value = tint });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
