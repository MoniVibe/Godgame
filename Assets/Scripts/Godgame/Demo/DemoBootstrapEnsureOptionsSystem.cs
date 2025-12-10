using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Demo
{
    /// <summary>
    /// Ensures DemoOptions, TimeState, and RewindState singletons exist with sensible defaults so the demo always boots and runs.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct DemoBootstrapEnsureOptionsSystem : ISystem
    {
        [BurstDiscard]
        private static FixedString64Bytes FS(string s)
        {
            var f = default(FixedString64Bytes);
            for (int i = 0; i < s.Length; i++)
            {
                f.Append(s[i]);
            }
            return f;
        }

        public void OnCreate(ref SystemState state)
        {
            var em = state.EntityManager;

            if (!SystemAPI.TryGetSingleton<DemoOptions>(out _))
            {
                var e = em.CreateEntity(typeof(DemoOptions));
                em.SetComponentData(e, new DemoOptions
                {
                    ScenarioPath = FS("Scenarios/godgame/villager_loop_small.json"),
                    BindingsSet = 0,
                    Veteran = 0
                });
            }

            if (!SystemAPI.TryGetSingleton<TimeState>(out _))
            {
                var t = em.CreateEntity(typeof(TimeState));
                em.SetComponentData(t, new TimeState
                {
                    IsPaused = false,
                    FixedDeltaTime = 1f / 60f,
                    DeltaTime = 0f,
                    Tick = 0
                });
            }

            // RewindState is now provided by RewindConfigAuthoring in the GodgameRegistrySubScene.
            // Creating it here causes duplication when the SubScene loads.
            /*
            if (!SystemAPI.TryGetSingleton<RewindState>(out _))
            {
                var r = em.CreateEntity(typeof(RewindState));
                em.SetComponentData(r, new RewindState
                {
                    Mode = RewindMode.Record,
                    PlaybackTick = 0
                });
            }
            */
        }
    }
}
