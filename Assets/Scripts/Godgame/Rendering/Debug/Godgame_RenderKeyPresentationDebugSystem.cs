using Unity.Burst;
using Unity.Entities;
using PureDOTS.Rendering;
using Godgame.Rendering.Debug;

namespace Godgame.Rendering.Debug
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    public partial struct Godgame_RenderKeyPresentationDebugSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var q = SystemAPI.QueryBuilder()
                .WithAll<RenderKey>()
                .Build();

            int count = q.CalculateEntityCount();

            Log.Message($"[Godgame RenderKey PRESENTATION] World '{state.WorldUnmanaged.Name}' has {count} RenderKey entities.");
            state.Enabled = false; // log once; remove if continuous logging is desired
        }

        public void OnCreate(ref SystemState state) { }
        public void OnDestroy(ref SystemState state) { }
    }
}


