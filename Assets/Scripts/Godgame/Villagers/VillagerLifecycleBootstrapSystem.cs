using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Adds default lifecycle and reproduction state to villagers if missing.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerLifecycleBootstrapSystem : ISystem
    {
        private EntityQuery _missingLifecycle;
        private EntityQuery _missingReproduction;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerId>();
            state.RequireForUpdate<TimeState>();
            _missingLifecycle = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerLifecycleState>()
                .Build();
            _missingReproduction = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerReproductionState>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_missingLifecycle.IsEmptyIgnoreFilter && _missingReproduction.IsEmptyIgnoreFilter)
            {
                state.Enabled = false;
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var tuning = SystemAPI.HasSingleton<VillagerLifecycleTuning>()
                ? SystemAPI.GetSingleton<VillagerLifecycleTuning>()
                : new VillagerLifecycleTuning
                {
                    ChildStageDays = 10f,
                    YouthStageDays = 20f,
                    AdultStageDays = 60f,
                    ElderStageDays = 90f,
                    FertilityStartDays = 18f,
                    FertilityEndDays = 70f
                };

            var defaultAgeDays = math.max(0f, tuning.FertilityStartDays + 2f);
            var defaultStage = ResolveStage(defaultAgeDays, in tuning);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            if (!_missingLifecycle.IsEmptyIgnoreFilter)
            {
                var entities = _missingLifecycle.ToEntityArray(state.WorldUpdateAllocator);
                for (int i = 0; i < entities.Length; i++)
                {
                    ecb.AddComponent(entities[i], new VillagerLifecycleState
                    {
                        AgeDays = defaultAgeDays,
                        Stage = defaultStage,
                        LastUpdateTick = timeState.Tick
                    });
                }
            }

            if (!_missingReproduction.IsEmptyIgnoreFilter)
            {
                var entities = _missingReproduction.ToEntityArray(state.WorldUpdateAllocator);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var seed = math.hash(new uint2((uint)(entity.Index + 1), timeState.Tick + 7u));
                    var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
                    var sex = random.NextFloat() < 0.5f ? VillagerSex.Female : VillagerSex.Male;

                    ecb.AddComponent(entity, new VillagerReproductionState
                    {
                        Sex = sex,
                        Fertility = 0.6f,
                        IsPregnant = 0,
                        PregnancyDays = 0f,
                        ConceptionTick = 0u,
                        NextConceptionTick = 0u,
                        Partner = Entity.Null,
                        BirthCount = 0,
                        PendingBirths = 0
                    });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static VillagerLifeStage ResolveStage(float ageDays, in VillagerLifecycleTuning tuning)
        {
            var childMax = math.max(0f, tuning.ChildStageDays);
            var youthMax = childMax + math.max(0f, tuning.YouthStageDays);
            var adultMax = youthMax + math.max(0f, tuning.AdultStageDays);

            if (ageDays < childMax) return VillagerLifeStage.Child;
            if (ageDays < youthMax) return VillagerLifeStage.Youth;
            if (ageDays < adultMax) return VillagerLifeStage.Adult;
            return VillagerLifeStage.Elder;
        }
    }
}
