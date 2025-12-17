using Unity.Entities;
using UnityEngine;
using Godgame.Rendering;
using Godgame.Rendering.Catalog;
using PureDOTS.Runtime.Core;

namespace Godgame.DebugSystems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial class CheckRenderCatalogSystem : SystemBase
    {
        private EntityQuery _catalogQuery;
        private double _startTime;
        private bool _warnedMissing;
        private bool _loggedCatalog;
        private const double CatalogGraceSeconds = 2.0;

        protected override void OnCreate()
        {
            _catalogQuery = GetEntityQuery(ComponentType.ReadOnly<RenderCatalogSingleton>());
            RequireForUpdate<GameWorldTag>();
            _startTime = double.NaN;
        }

        protected override void OnStartRunning()
        {
            _startTime = UnityEngine.Time.realtimeSinceStartupAsDouble;
            _warnedMissing = false;
            _loggedCatalog = false;
        }

        protected override void OnStopRunning()
        {
            _startTime = double.NaN;
            _warnedMissing = false;
            _loggedCatalog = false;
        }

        protected override void OnUpdate()
        {
            var count = _catalogQuery.CalculateEntityCount();

            if (count == 1 && SystemAPI.HasSingleton<RenderCatalogSingleton>())
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
                _loggedCatalog = true;
            }
            else if (count == 0)
            {
                // Allow streaming/baking to complete before emitting a warning so we avoid startup false positives.
                var now = UnityEngine.Time.realtimeSinceStartupAsDouble;
                var elapsed = now - (double.IsNaN(_startTime) ? now : _startTime);
                if (elapsed < CatalogGraceSeconds)
                    return;

                if (!_warnedMissing)
                {
                    UnityEngine.Debug.LogWarning("[CheckRenderCatalogSystem] RenderCatalogSingleton NOT found after waiting for streaming to finish.");
                    _warnedMissing = true;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[CheckRenderCatalogSystem] Expected exactly one RenderCatalogSingleton but found {count}. Remove duplicate catalog authoring objects so we only bake one singleton.");
            }

            if (_loggedCatalog || _warnedMissing || count > 1)
            {
                Enabled = false;
            }
        }
    }
}
