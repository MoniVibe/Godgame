using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures a default work tuning singleton exists for villager roles.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerWorkTuningBootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<VillagerWorkTuning>())
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new VillagerWorkTuning
                {
                    ForesterInputId = new FixedString64Bytes("wood"),
                    ForesterOutputId = new FixedString64Bytes("lumber"),
                    MinerOutputId = new FixedString64Bytes("ore"),
                    FarmerOutputId = new FixedString64Bytes("grain"),
                    ForesterInputIndex = ushort.MaxValue,
                    ForesterOutputIndex = ushort.MaxValue,
                    MinerOutputIndex = ushort.MaxValue,
                    FarmerOutputIndex = ushort.MaxValue,
                    HaulChance = 0.2f,
                    HaulCooldownSeconds = 8f,
                    PileDropMinUnits = 6f,
                    PileDropMaxUnits = 30f,
                    PilePickupMinUnits = 3f,
                    PileSearchRadius = 60f,
                    LastResolvedTick = 0u
                });
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }
    }
}
