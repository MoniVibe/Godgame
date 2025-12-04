using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Godgame.Demo;

public class CheckVillagers
{
    public static void Check()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null)
        {
            Debug.LogError("No Default World found.");
            return;
        }

        var entityManager = world.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(DemoVillagerState), typeof(LocalTransform));
        var entityCount = query.CalculateEntityCount();
        
        Debug.Log($"Found {entityCount} Villager entities.");

        if (entityCount > 0)
        {
            var entities = query.ToEntityArray(Allocator.Temp);
            var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            
            for (int i = 0; i < Mathf.Min(5, entityCount); i++)
            {
                Debug.Log($"Villager {entities[i].Index}: Position {transforms[i].Position}, Scale {transforms[i].Scale}");
            }
            
            entities.Dispose();
            transforms.Dispose();
        }
    }
}
