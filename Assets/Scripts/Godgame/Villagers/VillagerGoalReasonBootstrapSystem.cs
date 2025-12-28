using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Adds goal reason tracking to villagers if missing.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerGoalReasonBootstrapSystem : ISystem
    {
        private EntityQuery _missingReasons;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerId>();
            _missingReasons = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerGoalReason>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_missingReasons.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.AddComponent(_missingReasons, new VillagerGoalReason
            {
                Kind = VillagerGoalReasonKind.None,
                Urgency = 0f,
                SourceEntity = Entity.Null,
                SetTick = 0u
            });
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
