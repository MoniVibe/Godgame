using Godgame.Villagers;
using PureDOTS.Runtime.Aggregate;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Bands
{
    /// <summary>
    /// Detects band formation candidates and creates bands based on alignment/outlook/initiative rules and patriotism thresholds.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BandFormationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var random = new Unity.Mathematics.Random((uint)(tick + 1));

            // Detect formation candidates
            var detectJob = new DetectFormationCandidatesJob
            {
                Tick = tick,
                Random = random
            };
            state.Dependency = detectJob.ScheduleParallel(state.Dependency);

            // Process formation requests
            var processJob = new ProcessFormationRequestsJob
            {
                Tick = tick,
                Ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                    .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            };
            state.Dependency = processJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct DetectFormationCandidatesJob : IJobEntity
        {
            public uint Tick;
            public Unity.Mathematics.Random Random;

            void Execute(
                Entity entity,
                in VillagerPersonality personality,
                in VillagerAlignment alignment,
                in VillagerOutlook outlook,
                in VillagerInitiativeState initiative,
                in VillagerPatriotism patriotism)
            {
                // Check if villager is eligible for band formation
                if (initiative.CurrentInitiative < 0.4f)
                {
                    return; // Too low initiative
                }

                if (patriotism.Value < 30)
                {
                    return; // Too low patriotism (likely to defect)
                }

                // Check for nearby compatible villagers (simplified - would use spatial queries)
                // For now, this is a placeholder that would be expanded with actual proximity detection
            }
        }

        [BurstCompile]
        public partial struct ProcessFormationRequestsJob : IJobEntity
        {
            public uint Tick;
            public EntityCommandBuffer.ParallelWriter Ecb;

            void Execute(
                [ChunkIndexInQuery] int ciq,
                Entity entity,
                in BandFormationCandidate candidate)
            {
                // Calculate formation probability
                var probability = CalculateFormationProbability(candidate);

                // Create band if probability check passes
                if (probability > 0.5f) // Simplified threshold
                {
                    CreateBand(ciq, entity, candidate, Tick, Ecb);
                }
            }

            private static float CalculateFormationProbability(BandFormationCandidate candidate)
            {
                // Base probability from member count and acceptance status
                var baseProbability = candidate.AllProspectsAccepted ? 0.7f : 0.3f;
                
                // Member count modifier (more members = higher probability)
                var memberModifier = math.clamp(candidate.ProspectiveMemberCount / 10f, 0f, 0.3f);

                return math.clamp(baseProbability + memberModifier, 0f, 1f);
            }

            private static void CreateBand(
                int ciq,
                Entity candidateEntity,
                BandFormationCandidate candidate,
                uint tick,
                EntityCommandBuffer.ParallelWriter ecb)
            {
                // Create band entity
                var bandEntity = ecb.CreateEntity(ciq);
                ecb.AddComponent(ciq, bandEntity, new Band
                {
                    BandName = default,
                    Purpose = candidate.ProposedPurpose,
                    Leader = candidate.InitiatorEntity,
                    FormationTick = candidate.ProposedTick != 0 ? candidate.ProposedTick : tick,
                    Id = 0, // Will be assigned by system
                    FactionId = 0,
                    Morale = 0.5f,
                    Cohesion = 0.5f,
                    Fatigue = 0f,
                    Status = BandStatus.Forming,
                    LastUpdateTick = tick,
                    MinSize = 3,
                    MaxSize = 10,
                    RecruitmentBuilding = Entity.Null,
                    Experience = 0f
                });

                // Add members
                // (Simplified - would iterate through candidate members)
                
                // Remove candidate component
                ecb.RemoveComponent<BandFormationCandidate>(ciq, candidateEntity);
            }
        }
    }
}

