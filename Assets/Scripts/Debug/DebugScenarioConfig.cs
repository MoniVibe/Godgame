using Godgame.Scenario;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, null, null, "DebugDemoConfig")]
public class DebugScenarioConfig : MonoBehaviour
{
    void Update()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        var em = world.EntityManager;
        var query = em.CreateEntityQuery(typeof(SettlementConfig));
        int count = query.CalculateEntityCount();
        
        if (count > 0)
        {
            Debug.Log($"[DebugScenarioConfig] Found {count} SettlementConfig entities.");
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var e in entities)
            {
                var config = em.GetComponentData<SettlementConfig>(e);
                Debug.Log($"[DebugScenarioConfig] Entity {e}: VillagerPrefab={config.VillagerPrefab}, StorehousePrefab={config.StorehousePrefab}");
            }
            entities.Dispose();
            enabled = false; // Stop checking
        }
        else
        {
            // Debug.Log("[DebugScenarioConfig] No SettlementConfig found yet...");
        }
    }
}
