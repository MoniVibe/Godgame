using Godgame.Climate;
using PureDOTS.Environment;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Climate.Systems
{
    /// <summary>
    /// Processes god climate brush commands and creates/updates climate control sources.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameplaySystemGroup))]
    public partial struct GodClimateBrushSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ClimateBrushCommand>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

            foreach (var (brushCommands, brushEntity) in SystemAPI.Query<DynamicBuffer<ClimateBrushCommand>>()
                .WithEntityAccess())
            {
                for (int i = 0; i < brushCommands.Length; i++)
                {
                    var command = brushCommands[i];

                    // Find or create climate control source at this position
                    Entity? existingSource = null;
                    float minDist = float.MaxValue;

                    foreach (var (source, transform, sourceEntity) in SystemAPI.Query<RefRO<ClimateControlSource>, RefRO<LocalTransform>>()
                        .WithAll<ClimateControlSource>()
                        .WithEntityAccess())
                    {
                        var dist = math.distance(transform.ValueRO.Position, command.Position);
                        if (dist < command.Radius * 0.5f && dist < minDist)
                        {
                            existingSource = sourceEntity;
                            minDist = dist;
                        }
                    }

                    if (existingSource.HasValue)
                    {
                        // Update existing source
                        var source = SystemAPI.GetComponent<ClimateControlSource>(existingSource.Value);
                        source.TargetClimate = command.TargetClimate;
                        source.Radius = command.Radius;
                        source.Strength = command.Strength;
                        source.Center = command.Position;
                        ecb.SetComponent(existingSource.Value, source);
                    }
                    else
                    {
                        // Create new source
                        var newEntity = ecb.CreateEntity();
                        ecb.AddComponent(newEntity, new ClimateControlSource
                        {
                            Kind = ClimateControlKind.GodMiracle,
                            Center = command.Position,
                            Radius = command.Radius,
                            TargetClimate = command.TargetClimate,
                            Strength = command.Strength
                        });
                        ecb.AddComponent(newEntity, LocalTransform.FromPositionRotationScale(
                            command.Position, quaternion.identity, 1f));
                    }
                }

                // Clear processed commands
                brushCommands.Clear();
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

