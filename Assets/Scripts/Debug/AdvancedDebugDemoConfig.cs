using Unity.Entities;
using UnityEngine;
using Godgame.Demo;

public class AdvancedDebugDemoConfig : MonoBehaviour
{
    void Update()
    {
        foreach (var world in World.All)
        {
            var em = world.EntityManager;
            var query = em.CreateEntityQuery(typeof(DemoSettlementConfig));
            int count = query.CalculateEntityCount();
            
            if (count > 0)
            {
                Debug.Log($"[AdvancedDebug] World '{world.Name}': Found {count} DemoSettlementConfig entities.");
                var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                foreach (var e in entities)
                {
                    var config = em.GetComponentData<DemoSettlementConfig>(e);
                    Debug.Log($"[AdvancedDebug] World '{world.Name}' Entity {e}: VillagerPrefab={config.VillagerPrefab}, StorehousePrefab={config.StorehousePrefab}");
                }
                entities.Dispose();
            }
        }
    }
}
