using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using PureDOTS.Rendering;
using Godgame.Rendering.Debug;

namespace Godgame.Rendering.Debug
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct Godgame_RenderKeySimDebugSystem : ISystem
    {
        private EntityQuery _renderSemanticKeyQuery;

        public void OnCreate(ref SystemState state)
        {
            _renderSemanticKeyQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderSemanticKey>());
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            int count = _renderSemanticKeyQuery.CalculateEntityCount();
            
            // Track expected entities from headless scenario vision via semantic keys.
            var villagerCount = 0;
            var villageCount = 0;
            var bandCount = 0;
            if (!_renderSemanticKeyQuery.IsEmptyIgnoreFilter)
            {
                using var keys = _renderSemanticKeyQuery.ToComponentDataArray<RenderSemanticKey>(Unity.Collections.Allocator.Temp);
                foreach (var key in keys)
                {
                    var value = key.Value;
                    if (value >= Godgame.Rendering.GodgameSemanticKeys.VillagerMiner
                        && value <= Godgame.Rendering.GodgameSemanticKeys.VillagerCombatant)
                    {
                        villagerCount++;
                    }
                    else if (value == Godgame.Rendering.GodgameSemanticKeys.VillageCenter)
                    {
                        villageCount++;
                    }
                    else if (value == Godgame.Rendering.GodgameSemanticKeys.Band)
                    {
                        bandCount++;
                    }
                }
            }
            
            state.Enabled = false;
            LogRenderKeyCount(state.WorldUnmanaged.Name, count);
            LogExpectedEntities(state.WorldUnmanaged.Name, villagerCount, villageCount, bandCount);
#else
            state.Enabled = false;
#endif
        }

        public void OnDestroy(ref SystemState state) { }

#if UNITY_EDITOR
        [BurstDiscard]
        private static void LogRenderKeyCount(FixedString128Bytes worldName, int count)
        {
            Log.Message($"[Godgame Render SIM] World '{worldName}' has {count} RenderSemanticKey entities.");
        }
        
        [BurstDiscard]
        private static void LogExpectedEntities(FixedString128Bytes worldName, int villagers, int villages, int bands)
        {
            var hasExpected = villagers > 0 || villages > 0;
            var hasVisionEntities = bands > 0;
            
            if (!hasExpected)
            {
                Log.Message($"[Godgame Render SIM] WARNING: No villagers ({villagers}) or villages ({villages}) found. Expected from godgame_smoke.json scenario.");
            }
            
            if (!hasVisionEntities)
            {
                Log.Message($"[Godgame Render SIM] NOTE: No bands ({bands}) found. Armies and adventurer bands are part of the target vision (villages in vast landscape, armies patrolling, adventurers roaming) but may not be implemented yet in headless.");
            }
            else
            {
                Log.Message($"[Godgame Render SIM] Vision entities: Villagers={villagers} Villages={villages} Bands={bands}");
            }
        }
#endif
    }
}
