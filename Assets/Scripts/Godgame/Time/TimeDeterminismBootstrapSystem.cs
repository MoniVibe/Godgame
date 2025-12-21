using Godgame.Time;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Time
{
    /// <summary>
    /// Ensures a rewindable determinism actor exists when explicitly enabled.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial struct TimeDeterminismBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<TimeDeterminismTag>())
            {
                state.Enabled = false;
                return;
            }

            EnsureDeterminismEntity(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
        }

        private void EnsureDeterminismEntity(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeDeterminismState>());
            if (!query.IsEmptyIgnoreFilter)
            {
                return;
            }

            var entity = entityManager.CreateEntity(typeof(TimeDeterminismConfig), typeof(TimeDeterminismState),
                typeof(LocalTransform), typeof(RewindableTag), typeof(PlaceholderVisual));

            entityManager.SetComponentData(entity, new TimeDeterminismConfig
            {
                VelocityPerSecond = new float3(0.35f, 0.15f, -0.2f),
                PhaseRadiansPerSecond = math.PI * 0.35f
            });

            entityManager.SetComponentData(entity, new TimeDeterminismState
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
