using PureDOTS.Runtime.Hand;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Headless
{
    /// <summary>
    /// Headless runs do not have live input or cameras; disable hand input to avoid physics raycasts.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(PureDOTS.Systems.Hand.HandInputCollectorSystem))]
    public partial struct GodgameHeadlessDisableHandInputSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless || !Application.isBatchMode)
            {
                state.Enabled = false;
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingletonEntity<HandInputFrame>(out var inputEntity))
            {
                state.EntityManager.DestroyEntity(inputEntity);
            }

            if (SystemAPI.TryGetSingletonEntity<HandHover>(out var hoverEntity))
            {
                state.EntityManager.DestroyEntity(hoverEntity);
            }

            state.Enabled = false;
        }
    }
}
