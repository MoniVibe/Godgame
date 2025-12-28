#if GODGAME_SCENARIO && GODGAME_DEVTOOLS
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Godgame.Scenario;
// using PureDOTS.LegacyScenario.Village; // legacy scenario dependency

namespace Godgame.Villages
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillageDebugSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }
    }
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class VillageDebugSystemClass : SystemBase
    {
        private float _nextLogTime;
        private EntityQuery _villageQuery;
        private EntityQuery _villagerQuery;
        private EntityQuery _puredotsVillageQuery;
        private EntityQuery _puredotsVillagerQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            _villageQuery = GetEntityQuery(ComponentType.ReadOnly<Village>());
            _villagerQuery = GetEntityQuery(ComponentType.ReadOnly<SettlementVillagerState>());
            _puredotsVillageQuery = GetEntityQuery(ComponentType.ReadOnly<VillageTag>());
            _puredotsVillagerQuery = GetEntityQuery(ComponentType.ReadOnly<VillagerTag>());
        }

        protected override void OnUpdate()
        {
#if GODGAME_DEBUG && UNITY_EDITOR
            if (SystemAPI.Time.ElapsedTime < _nextLogTime)
            {
                return;
            }
            _nextLogTime = (float)SystemAPI.Time.ElapsedTime + 2.0f; // Log every 2 seconds

            // Use SettlementVillagerState as a proxy for villagers in the scenario bootstrap
            var puredotsVillageCount = _puredotsVillageQuery.CalculateEntityCount();
            var puredotsVillagerCount = _puredotsVillagerQuery.CalculateEntityCount();

            var villageCount = _villageQuery.CalculateEntityCount();
            var villagerCount = _villagerQuery.CalculateEntityCount();

            Godgame.GodgameDebug.Log($"[VillageDebugSystem] Godgame Villages: {villageCount}, Godgame Villagers: {villagerCount}, PureDOTS Villages: {puredotsVillageCount}, PureDOTS Villagers: {puredotsVillagerCount}");
#endif
        }
    }
}
#endif
