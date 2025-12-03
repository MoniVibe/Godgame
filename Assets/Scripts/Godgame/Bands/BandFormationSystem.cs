using Godgame.Villagers;
using PureDOTS.Runtime.Aggregates;
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
                // Base probability from alignment compatibility
                var alignmentCompat = CalculateAlignmentCompatibility(candidate);
                
                // Outlook compatibility
                var outlookCompat = CalculateOutlookCompatibility(candidate);
                
                // Initiative modifier
                var initiativeModifier = candidate.AverageInitiative * 0.3f;

                return math.clamp(alignmentCompat + outlookCompat + initiativeModifier, 0f, 1f);
            }

            private static float CalculateAlignmentCompatibility(BandFormationCandidate candidate)
            {
                // Matching alignments increase compatibility
                // Simplified - would compare actual alignment values
                return 0.5f; // Default
            }

            private static float CalculateOutlookCompatibility(BandFormationCandidate candidate)
            {
                // Matching outlooks increase compatibility
                // Warlike + Warlike = high compatibility
                // Warlike + Peaceful = low compatibility
                return 0.5f; // Default
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
                    Purpose = candidate.Purpose,
                    LeaderEntity = Entity.Null,
                    FormationTick = tick,
                    MemberCount = 0
                });

                ecb.AddComponent(ciq, bandEntity, new BandId
                {
                    Value = candidate.BandId,
                    FactionId = 0,
                    Leader = Entity.Null
                });

                // Add members
                // (Simplified - would iterate through candidate members)
                
                // Remove candidate component
                ecb.RemoveComponent<BandFormationCandidate>(ciq, candidateEntity);
            }
        }
    }

    /// <summary>
    /// Band formation candidate component (temporary, removed after processing).
    /// </summary>
    public struct BandFormationCandidate : IComponentData
    {
        public int BandId;
        public BandPurpose Purpose;
        public float AverageInitiative;
        public float AlignmentCompatibility;
        public float OutlookCompatibility;
    }
}

