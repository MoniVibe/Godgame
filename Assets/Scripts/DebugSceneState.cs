using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class DebugSceneState : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- Debugging Scene State ---");
        
        // Check Camera
        var cam = Camera.main;
        if (cam != null)
        {
            Debug.Log($"Main Camera: Pos={cam.transform.position}, Rot={cam.transform.rotation.eulerAngles}");
        }
        else
        {
            Debug.LogError("Main Camera not found! Checking all cameras...");
            foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                Debug.Log($"Found Camera: {c.name} at {c.transform.position}");
            }
        }

        // Check Entities
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
        {
            var em = world.EntityManager;
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<LocalTransform>());
            var count = query.CalculateEntityCount();
            Debug.Log($"Total Entities with LocalTransform: {count}");

            using var entities = query.ToEntityArray(Allocator.Temp);
            using var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var e = entities[i];
                var t = transforms[i];
                Debug.Log($"Entity {e.Index}: Pos={t.Position}, Scale={t.Scale}");
            }
        }
        else
        {
            Debug.LogError("Default World not found!");
        }
    }
}
