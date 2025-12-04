using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Godgame.Demo;
using PureDOTS.Demo.Village;

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

        protected override void OnUpdate()
        {
#if GODGAME_DEBUG && UNITY_EDITOR
            if (SystemAPI.Time.ElapsedTime < _nextLogTime)
            {
                return;
            }
            _nextLogTime = (float)SystemAPI.Time.ElapsedTime + 2.0f; // Log every 2 seconds

            var villageQuery = GetEntityQuery(ComponentType.ReadOnly<Village>());
            // Use DemoVillagerState as a proxy for villagers in the demo
            var villagerQuery = GetEntityQuery(ComponentType.ReadOnly<DemoVillagerState>());

            // PureDOTS demo components
            var puredotsVillageCount = SystemAPI.QueryBuilder().WithAll<VillageTag>().Build().CalculateEntityCount();
            var puredotsVillagerCount = SystemAPI.QueryBuilder().WithAll<VillagerTag>().Build().CalculateEntityCount();

            var villageCount = villageQuery.CalculateEntityCount();
            var villagerCount = villagerQuery.CalculateEntityCount();

            Godgame.GodgameDebug.Log($"[VillageDebugSystem] Godgame Villages: {villageCount}, Godgame Villagers: {villagerCount}, PureDOTS Villages: {puredotsVillageCount}, PureDOTS Villagers: {puredotsVillagerCount}");
#endif
        }
    }
}
