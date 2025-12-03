using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Tracks honor ledger, dual rank ladders, and promotion history for military bands.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct CombatHonorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Process honor updates after combat resolution
            var job = new ProcessHonorUpdatesJob();
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ProcessHonorUpdatesJob : IJobEntity
        {
            void Execute(
                Entity entity,
                ref HonorLedger ledger,
                in VillagerPersonality personality,
                in VillagerAlignment alignment)
            {
                // Update honor decay
                ledger.HonorPoints = math.max(0, ledger.HonorPoints - (int)(ledger.DecayRate * 0.1f));
                ledger.Glory = math.max(0f, ledger.Glory - ledger.GloryDecayRate * 0.1f);

                // Check for promotion eligibility
                if (ShouldCheckPromotion(ledger))
                {
                    CheckPromotion(entity, ref ledger, personality, alignment);
                }
            }

            private static bool ShouldCheckPromotion(HonorLedger ledger)
            {
                // Check promotion thresholds based on current rank
                return ledger.HonorPoints >= GetPromotionThreshold(ledger.CurrentRank);
            }

            private static int GetPromotionThreshold(MilitaryRank rank)
            {
                return rank switch
                {
                    MilitaryRank.Recruit => 100,
                    MilitaryRank.Private => 250,
                    MilitaryRank.Legionary => 500,
                    MilitaryRank.VeteranLegionary => 1000,
                    MilitaryRank.Centurion => 2000,
                    MilitaryRank.Champion => 5000,
                    _ => int.MaxValue
                };
            }

            private static void CheckPromotion(
                Entity entity,
                ref HonorLedger ledger,
                VillagerPersonality personality,
                VillagerAlignment alignment)
            {
                // Promotion logic based on rank track and honor
                var nextRank = GetNextRank(ledger.CurrentRank, ledger.IsEliteTrack);
                
                if (nextRank != MilitaryRank.None)
                {
                    ledger.CurrentRank = nextRank;
                    // Add promotion event to history
                }
            }

            private static MilitaryRank GetNextRank(MilitaryRank currentRank, bool isEliteTrack)
            {
                if (isEliteTrack)
                {
                    return currentRank switch
                    {
                        MilitaryRank.Lieutenant => MilitaryRank.Captain,
                        MilitaryRank.Captain => MilitaryRank.Commander,
                        MilitaryRank.Commander => MilitaryRank.Marshal,
                        _ => MilitaryRank.None
                    };
                }
                else
                {
                    return currentRank switch
                    {
                        MilitaryRank.Recruit => MilitaryRank.Private,
                        MilitaryRank.Private => MilitaryRank.Legionary,
                        MilitaryRank.Legionary => MilitaryRank.VeteranLegionary,
                        MilitaryRank.VeteranLegionary => MilitaryRank.Centurion,
                        MilitaryRank.Centurion => MilitaryRank.Champion,
                        _ => MilitaryRank.None
                    };
                }
            }
        }
    }

    /// <summary>
    /// Honor ledger tracking combat performance and promotion eligibility.
    /// </summary>
    public struct HonorLedger : IComponentData
    {
        public int HonorPoints;
        public float Glory;
        public float Reputation;
        public MilitaryRank CurrentRank;
        public bool IsEliteTrack;
        public float DecayRate;
        public float GloryDecayRate;
    }

    /// <summary>
    /// Military rank enumeration (dual ladder: Enlisted and Elite).
    /// </summary>
    public enum MilitaryRank : byte
    {
        None = 0,
        // Enlisted track
        Recruit = 1,
        Private = 2,
        Legionary = 3,
        VeteranLegionary = 4,
        Centurion = 5,
        Champion = 6,
        // Elite track
        Lieutenant = 10,
        Captain = 11,
        Commander = 12,
        Marshal = 13
    }

    /// <summary>
    /// Promotion history buffer for tracking rank progression.
    /// </summary>
    public struct PromotionHistory : IBufferElementData
    {
        public MilitaryRank Rank;
        public uint PromotionTick;
        public int HonorPointsAtPromotion;
        public float GloryAtPromotion;
    }
}

