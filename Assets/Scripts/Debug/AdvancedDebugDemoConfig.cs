using Godgame.Scenario;
using Unity.Entities;
using UnityEngine;

public class AdvancedDebugDemoConfig : MonoBehaviour
{
    void Update()
    {
        foreach (var world in World.All)
        {
            var em = world.EntityManager;
            var query = em.CreateEntityQuery(typeof(SettlementConfig));
            int count = query.CalculateEntityCount();
            
            if (count > 0)
            {
                Debug.Log($"[AdvancedDebug] World '{world.Name}': Found {count} SettlementConfig entities.");
                var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                foreach (var e in entities)
                {
                    var config = em.GetComponentData<SettlementConfig>(e);
                    Debug.Log($"[AdvancedDebug] World '{world.Name}' Entity {e}: VillagerPrefab={config.VillagerPrefab}, StorehousePrefab={config.StorehousePrefab}");
                }
                entities.Dispose();
            }
        }
    }
}
