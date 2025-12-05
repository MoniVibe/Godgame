using Godgame.Miracles;
using PureDOTS.Environment;
using PureDOTS.Runtime.Components;
using MiracleType = PureDOTS.Runtime.Components.MiracleType;
using MiracleLifecycleState = PureDOTS.Runtime.Components.MiracleLifecycleState;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Integrates miracles with climate control system.
    /// Rain/Fire miracles create climate control sources that affect local climate.
    /// </summary>
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    [BurstCompile]
    public partial struct MiracleBiomeIntegrationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleToken>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

            foreach (var (miracleEntity, miracle, transform) in SystemAPI.Query<Entity, RefRO<MiracleToken>, RefRO<LocalTransform>>())
            {
                // Only process active miracles
                if (miracle.ValueRO.Lifecycle != MiracleLifecycleState.Active)
                {
                    continue;
                }

                var position = transform.ValueRO.Position;
                var radius = miracle.ValueRO.CurrentRadius;
                var intensity = miracle.ValueRO.CurrentIntensity;

                ClimateVector targetClimate = default;
                bool shouldCreate = false;

                // Map miracle types to climate effects
                switch (miracle.ValueRO.Type)
                {
                    case MiracleType.Rain:
                        targetClimate = new ClimateVector
                        {
                            Temperature = 0f,
                            Moisture = math.clamp(intensity, 0f, 1f),
                            Fertility = intensity * 0.3f,
                            WaterLevel = 0f,
                            Ruggedness = 0f
                        };
                        shouldCreate = true;
                        break;

                    case MiracleType.Fire:
                        targetClimate = new ClimateVector
                        {
                            Temperature = math.clamp(intensity * 0.5f, 0f, 1f),
                            Moisture = math.clamp(1f - intensity * 0.7f, 0f, 1f),
                            Fertility = math.clamp(1f - intensity * 0.4f, 0f, 1f),
                            WaterLevel = 0f,
                            Ruggedness = 0f
                        };
                        shouldCreate = true;
                        break;

                    case MiracleType.Freeze:
                        targetClimate = new ClimateVector
                        {
                            Temperature = math.clamp(-intensity, -1f, 0f),
                            Moisture = math.clamp(intensity * 0.5f, 0f, 1f),
                            Fertility = 0f,
                            WaterLevel = 0f,
                            Ruggedness = 0f
                        };
                        shouldCreate = true;
                        break;
                }

                if (shouldCreate && radius > 0f)
                {
                    // Find or create climate control source for this miracle
                    Entity? existingSource = null;
                    foreach (var (sourceEntity, source) in SystemAPI.Query<Entity, RefRO<ClimateControlSource>>())
                    {
                        if (math.distance(source.ValueRO.Center, position) < radius * 0.1f)
                        {
                            existingSource = sourceEntity;
                            break;
                        }
                    }

                    if (existingSource.HasValue)
                    {
                        // Update existing source
                        var source = SystemAPI.GetComponent<ClimateControlSource>(existingSource.Value);
                        source.TargetClimate = targetClimate;
                        source.Radius = radius;
                        source.Strength = intensity * 0.1f; // Convert intensity to strength
                        source.Center = position;
                        ecb.SetComponent(existingSource.Value, source);
                    }
                    else
                    {
                        // Create new source
                        var newEntity = ecb.CreateEntity();
                        ecb.AddComponent(newEntity, new ClimateControlSource
                        {
                            Kind = ClimateControlKind.GodMiracle,
                            Center = position,
                            Radius = radius,
                            TargetClimate = targetClimate,
                            Strength = intensity * 0.1f
                        });
                        ecb.AddComponent(newEntity, LocalTransform.FromPositionRotationScale(
                            position, quaternion.identity, 1f));
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

