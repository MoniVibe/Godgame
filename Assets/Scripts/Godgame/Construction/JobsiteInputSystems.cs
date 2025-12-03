using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Entities;
#if !UNITY_DOTSRUNTIME
using UnityEngine;
#endif

namespace Godgame.Construction
{
    /// <summary>
    /// Translates hotkey edges (or external input routers) into jobsite placement requests.
    /// </summary>
    [UpdateInGroup(typeof(ConstructionSystemGroup), OrderFirst = true)]
    public partial struct JobsitePlacementHotkeySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JobsitePlacementHotkeyState>();
            state.RequireForUpdate<JobsitePlacementState>();
            state.RequireForUpdate<JobsitePlacementConfig>();
            state.RequireForUpdate<JobsitePlacementRequest>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var placementEntity = SystemAPI.GetSingletonEntity<JobsitePlacementState>();
            var hotkey = SystemAPI.GetComponent<JobsitePlacementHotkeyState>(placementEntity);

            bool placeRequested = hotkey.PlaceRequested != 0;
#if !UNITY_DOTSRUNTIME && ENABLE_LEGACY_INPUT_MANAGER
            if (!placeRequested && Input.GetKeyDown(KeyCode.J))
            {
                placeRequested = true;
            }
#endif

            if (!placeRequested)
            {
                return;
            }

            var config = SystemAPI.GetComponent<JobsitePlacementConfig>(placementEntity);
            var placementState = SystemAPI.GetComponent<JobsitePlacementState>(placementEntity);
            var requests = SystemAPI.GetBuffer<JobsitePlacementRequest>(placementEntity);

            requests.Add(new JobsitePlacementRequest { Position = placementState.NextPosition });
            placementState.NextPosition += config.PositionStep;

            hotkey.PlaceRequested = 0;
            SystemAPI.SetComponent(placementEntity, placementState);
            SystemAPI.SetComponent(placementEntity, hotkey);
        }
    }
}
