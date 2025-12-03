using Godgame.Time;
using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Time
{
    /// <summary>
    /// Ensures time control singletons and a rewindable demo actor exist for the time slice.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial struct TimeDemoBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            EnsureTimeControls(ref state);
            EnsureDemoEntity(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
        }

        private void EnsureTimeControls(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            Entity controlEntity;

            var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeControlSingletonTag>());
            controlEntity = query.IsEmptyIgnoreFilter
                ? entityManager.CreateEntity(typeof(TimeControlSingletonTag))
                : query.GetSingletonEntity();

            if (!entityManager.HasComponent<TimeControlConfig>(controlEntity))
            {
                entityManager.AddComponentData(controlEntity, new TimeControlConfig
                {
                    SlowMotionSpeed = 0.5f,
                    FastForwardSpeed = 3f,
                    MinSpeedMultiplier = 0.01f,
                    MaxSpeedMultiplier = 16.0f
                });
            }

            if (!entityManager.HasComponent<PureDOTS.Runtime.Components.TimeControlInputState>(controlEntity))
            {
                entityManager.AddComponentData(controlEntity, default(PureDOTS.Runtime.Components.TimeControlInputState));
            }

            if (!entityManager.HasBuffer<TimeControlCommand>(controlEntity))
            {
                entityManager.AddBuffer<TimeControlCommand>(controlEntity);
            }
        }

        private void EnsureDemoEntity(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeDemoState>());
            if (!query.IsEmptyIgnoreFilter)
            {
                return;
            }

            var entity = entityManager.CreateEntity(typeof(TimeDemoConfig), typeof(TimeDemoState),
                typeof(LocalTransform), typeof(RewindableTag), typeof(PlaceholderVisual));

            entityManager.SetComponentData(entity, new TimeDemoConfig
            {
                VelocityPerSecond = new float3(0.35f, 0.15f, -0.2f),
                PhaseRadiansPerSecond = math.PI * 0.35f
            });

            entityManager.SetComponentData(entity, new TimeDemoState
            {
                Position = float3.zero,
                Phase = 0f,
                LastAppliedTick = 0
            });

            entityManager.SetComponentData(entity, LocalTransform.FromPositionRotationScale(float3.zero,
                quaternion.identity, 1f));

            entityManager.SetComponentData(entity, new PlaceholderVisual
            {
                Kind = PlaceholderVisualKind.Crate,
                BaseScale = 0.85f,
                LocalOffset = float3.zero
            });
        }
    }
}
