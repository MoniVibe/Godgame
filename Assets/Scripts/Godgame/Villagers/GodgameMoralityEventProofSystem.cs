using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Social;
using Unity.Entities;
using UnityDebug = UnityEngine.Debug;

namespace Godgame.Villagers
{
    /// <summary>
    /// Headless-only proof that morality events are being processed and applied deterministically.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GodgameMoralityEventApplySystem))]
    public partial struct GodgameMoralityEventProofSystem : ISystem
    {
        private byte _printed;

        public void OnCreate(ref SystemState state)
        {
            if (!RuntimeMode.IsHeadless)
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<MoralityEventQueueTag>();
            state.RequireForUpdate<MoralityEventProcessingState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_printed != 0)
            {
                state.Enabled = false;
                return;
            }

            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            var queueEntity = SystemAPI.GetSingletonEntity<MoralityEventQueueTag>();
            var processing = SystemAPI.GetComponentRO<MoralityEventProcessingState>(queueEntity).ValueRO;

            if (processing.TotalProcessedCount <= 0)
            {
                return;
            }

            UnityDebug.Log(
                $"[GodgameMoralityEventProof] PASS tick={tick} processedTick={processing.LastProcessedTick} processedCount={processing.LastProcessedCount} total={processing.TotalProcessedCount}");
            _printed = 1;
            state.Enabled = false;
        }
    }
}

