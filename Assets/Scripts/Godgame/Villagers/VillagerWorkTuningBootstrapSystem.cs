using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures a default work tuning singleton exists for villager roles.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerWorkTuningBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<VillagerWorkTuning>())
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new VillagerWorkTuning
                {
                    ForesterInputId = BuildWoodId(),
                    ForesterOutputId = BuildLumberId(),
                    MinerOutputId = BuildOreId(),
                    FarmerOutputId = BuildGrainId(),
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

        private static FixedString64Bytes BuildWoodId()
        {
            FixedString64Bytes id = default;
            id.Append('w');
            id.Append('o');
            id.Append('o');
            id.Append('d');
            return id;
        }

        private static FixedString64Bytes BuildLumberId()
        {
            FixedString64Bytes id = default;
            id.Append('l');
            id.Append('u');
            id.Append('m');
            id.Append('b');
            id.Append('e');
            id.Append('r');
            return id;
        }

        private static FixedString64Bytes BuildOreId()
        {
            FixedString64Bytes id = default;
            id.Append('o');
            id.Append('r');
            id.Append('e');
            return id;
        }

        private static FixedString64Bytes BuildGrainId()
        {
            FixedString64Bytes id = default;
            id.Append('g');
            id.Append('r');
            id.Append('a');
            id.Append('i');
            id.Append('n');
            return id;
        }
    }
}
