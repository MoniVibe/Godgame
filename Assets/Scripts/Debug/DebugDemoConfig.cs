using Unity.Entities;
using UnityEngine;
using Godgame.Demo;

public class DebugDemoConfig : MonoBehaviour
{
    void Update()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        var em = world.EntityManager;
        var query = em.CreateEntityQuery(typeof(DemoSettlementConfig));
        int count = query.CalculateEntityCount();
        
        if (count > 0)
        {
            Debug.Log($"[DebugDemoConfig] Found {count} DemoSettlementConfig entities.");
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var e in entities)
            {
                var config = em.GetComponentData<DemoSettlementConfig>(e);
                Debug.Log($"[DebugDemoConfig] Entity {e}: VillagerPrefab={config.VillagerPrefab}, StorehousePrefab={config.StorehousePrefab}");
            }
            entities.Dispose();
            enabled = false; // Stop checking
        }
        else
        {
            // Debug.Log("[DebugDemoConfig] No DemoSettlementConfig found yet...");
        }
    }
}
