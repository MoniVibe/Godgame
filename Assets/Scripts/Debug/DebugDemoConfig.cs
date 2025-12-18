using Godgame.Scenario;
using Unity.Entities;
using UnityEngine;

public class DebugDemoConfig : MonoBehaviour
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
            Debug.Log($"[DebugDemoConfig] Found {count} SettlementConfig entities.");
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var e in entities)
            {
                var config = em.GetComponentData<SettlementConfig>(e);
                Debug.Log($"[DebugDemoConfig] Entity {e}: VillagerPrefab={config.VillagerPrefab}, StorehousePrefab={config.StorehousePrefab}");
            }
            entities.Dispose();
            enabled = false; // Stop checking
        }
        else
        {
            // Debug.Log("[DebugDemoConfig] No SettlementConfig found yet...");
        }
    }
}
