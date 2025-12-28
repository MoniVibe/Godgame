using Godgame.Villages;
using PureDOTS.Runtime.Aggregate;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Motivation;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Aggregate
{
    /// <summary>
    /// Bridges existing Village entities to the generic aggregate system.
    /// Creates aggregate entities for villages and links them via VillageAggregateAdapter.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameVillageAggregateAdapterSystem : ISystem
    {
        // Type ID constant for Village aggregate type
        private const ushort VillageTypeId = 100; // Game-specific type ID

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            // Find villages without adapters
            foreach (var (village, entity) in SystemAPI.Query<RefRO<Village>>()
                .WithAbsent<VillageAggregateAdapter>()
                .WithEntityAccess())
            {
                // Create aggregate entity
                var aggregateEntity = ecb.CreateEntity();

                // Add aggregate components
                ecb.AddComponent(aggregateEntity, new AggregateIdentity
                {
                    TypeId = VillageTypeId,
                    Seed = (uint)village.ValueRO.VillageId.GetHashCode() // Use village ID as seed
                });

                ecb.AddComponent(aggregateEntity, new AggregateStats
                {
                    AvgInitiative = 0f,
                    AvgVengefulForgiving = 0f,
                    AvgBoldCraven = 0f,
                    AvgCorruptPure = 0f,
                    AvgChaoticLawful = 0f,
                    AvgEvilGood = 0f,
                    AvgMightMagic = 0f,
                    AvgAmbition = 0f,
                    StatusCoverage = 0f,
                    WealthCoverage = 0f,
                    PowerCoverage = 0f,
                    KnowledgeCoverage = 0f,
                    MemberCount = 0,
                    LastRecalcTick = 0
                });

                ecb.AddComponent(aggregateEntity, new AmbientGroupConditions
                {
                    AmbientCourage = 0f,
                    AmbientCaution = 0f,
                    AmbientAnger = 0f,
                    AmbientCompassion = 0f,
                    AmbientDrive = 0f,
                    ExpectationLoyalty = 0f,
                    ExpectationConformity = 0f,
                    ToleranceForOutliers = 0f,
                    LastUpdateTick = 0
                });

                // Add motivation components for group ambitions
                ecb.AddComponent(aggregateEntity, new MotivationDrive
                {
                    InitiativeCurrent = 100,
                    InitiativeMax = 200,
                    LoyaltyCurrent = 100,
                    LoyaltyMax = 200,
                    PrimaryLoyaltyTarget = Entity.Null,
                    LastInitiativeTick = 0
                });

                // Add adapter to village entity
                ecb.AddComponent(entity, new VillageAggregateAdapter
                {
                    AggregateEntity = aggregateEntity
                });

                // Mark aggregate as dirty for initial stats calculation
                ecb.AddComponent<AggregateStatsDirtyTag>(aggregateEntity);
            }
        }
    }
}
























