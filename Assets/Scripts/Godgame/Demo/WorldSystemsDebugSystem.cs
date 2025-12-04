using Unity.Entities;
using UnityEngine;

namespace Godgame.Demo
{
    /// <summary>
    /// Debug system that logs all systems in the ECS world at startup.
    /// Used to verify that PureDOTS demo systems (OrbitCubeSystem, VillageDemoBootstrapSystem, etc.) are loaded.
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class WorldSystemsDebugSystem : SystemBase
    {
        bool _logged;

        protected override void OnUpdate()
        {
            if (_logged)
                return;

            _logged = true;

#if GODGAME_DEBUG && UNITY_EDITOR
            Debug.Log("[WorldSystemsDebugSystem] === ECS World Systems ===");
            foreach (var sys in World.Systems)
            {
                Debug.Log($"[World] System: {sys.GetType().FullName}");
            }
            Debug.Log("[WorldSystemsDebugSystem] === End Systems List ===");
#endif
        }
    }
}

