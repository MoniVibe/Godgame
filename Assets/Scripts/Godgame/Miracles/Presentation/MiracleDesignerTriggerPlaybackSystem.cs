using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Miracles.Presentation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MiracleDesignerTriggerSystem))]
    public partial struct MiracleDesignerTriggerPlaybackSystem : ISystem
    {
        private EntityQuery _releaseQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleDesignerTrigger>();
            _releaseQuery = state.GetEntityQuery(ComponentType.ReadWrite<MiracleReleaseEvent>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_releaseQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var releaseEntity = _releaseQuery.GetSingletonEntity();
            var releaseBuffer = state.EntityManager.GetBuffer<MiracleReleaseEvent>(releaseEntity);

            foreach (var triggerBuffer in SystemAPI.Query<DynamicBuffer<MiracleDesignerTrigger>>())
            {
                if (triggerBuffer.Length == 0)
                {
                    continue;
                }

                foreach (var trigger in triggerBuffer)
                {
                    releaseBuffer.Add(new MiracleReleaseEvent
                    {
                        Type = trigger.Type,
                        Position = trigger.Position,
                        Direction = new float3(0f, 1f, 0f),
                        Impulse = 1f,
                        ConfigEntity = Entity.Null
                    });
                }

                triggerBuffer.Clear();
            }
        }
    }
}
