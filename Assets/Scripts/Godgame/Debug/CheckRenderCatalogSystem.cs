using Unity.Entities;
using UnityEngine;
using Godgame.Rendering;
using Godgame.Rendering.Catalog;

namespace Godgame.DebugSystems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class CheckRenderCatalogSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<RenderCatalogSingleton>())
            {
                var singleton = SystemAPI.GetSingleton<RenderCatalogSingleton>();
                if (singleton.Blob.IsCreated)
                {
                    ref var blob = ref singleton.Blob.Value;
                    UnityEngine.Debug.Log($"[CheckRenderCatalogSystem] Catalog found! Entries: {blob.Entries.Length}");
                    for (int i = 0; i < blob.Entries.Length; i++)
                    {
                        UnityEngine.Debug.Log($"[CheckRenderCatalogSystem] Entry {i}: ArchetypeId={blob.Entries[i].ArchetypeId}, MaterialMeshIndex={blob.Entries[i].MaterialMeshIndex}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("[CheckRenderCatalogSystem] Catalog singleton exists but Blob is not created!");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[CheckRenderCatalogSystem] RenderCatalogSingleton NOT found!");
            }
            
            Enabled = false;
        }
    }
}
