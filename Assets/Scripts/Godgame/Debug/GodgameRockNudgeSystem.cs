using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Godgame.Physics;

namespace Godgame.Debugging
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameRockNudgeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GodgamePhysicsBody>(); // or Rock tag
        }

        public void OnUpdate(ref SystemState state)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!UnityEngine.Input.GetKeyDown(KeyCode.N)) // N for Nudge
                return;

            var rand = new Unity.Mathematics.Random(1234u + (uint)System.Environment.TickCount);

            foreach (var (vel, transform) in
                     SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<LocalTransform>>().WithAll<GodgamePhysicsBody>())
            {
                // tiny lateral kick
                float3 dir = math.normalize(new float3(rand.NextFloat(-1,1), 0, rand.NextFloat(-1,1)));
                vel.ValueRW.Linear += dir * 5f; // tweak strength
            }
#else
            // Disable when the legacy input backend is not active to avoid InputSystem warnings.
            state.Enabled = false;
#endif
        }
    }
}
