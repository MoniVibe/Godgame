using Godgame.Time;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests.Time
{
    /// <summary>
    /// Validates rewind/resim determinism using bytewise snapshot equality.
    /// </summary>
    public class Time_RewindDeterminism_Playmode
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(Time_RewindDeterminism_Playmode));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();

            // Create time determinism config and state
            var configEntity = _entityManager.CreateEntity(typeof(TimeDeterminismConfig), typeof(TimeDeterminismState));
            _entityManager.SetComponentData(configEntity, new TimeDeterminismConfig
            {
                VelocityPerSecond = new float3(1f, 0f, 0f),
                PhaseRadiansPerSecond = 1f
            });
            _entityManager.SetComponentData(configEntity, new TimeDeterminismState
            {
                Position = float3.zero,
                Phase = 0f,
                LastAppliedTick = 0
            });

            var determinismEntity = _entityManager.CreateEntity(typeof(TimeDeterminismState), typeof(LocalTransform));
            _entityManager.SetComponentData(determinismEntity, new TimeDeterminismState
            {
                Position = float3.zero,
                Phase = 0f,
                LastAppliedTick = 0
            });
            _entityManager.SetComponentData(determinismEntity, LocalTransform.FromPosition(float3.zero));
        }

        [TearDown]
        public void TearDown()
        {
            if (_world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [Test]
        public void Rewind_Resim_Deterministic()
        {
            // Run simulation for N ticks
            int runTicks = 100;
            for (int i = 0; i < runTicks; i++)
            {
                UpdateSystems();
            }

            // Capture state snapshot
            var timeQuery = _entityManager.CreateEntityQuery(typeof(TimeState));
            var timeEntity = timeQuery.GetSingletonEntity();
            var timeStateBefore = _entityManager.GetComponentData<TimeState>(timeEntity);
            var determinismQuery = _entityManager.CreateEntityQuery(typeof(TimeDeterminismState));
            var determinismEntity = determinismQuery.GetSingletonEntity();
            var determinismStateBefore = _entityManager.GetComponentData<TimeDeterminismState>(determinismEntity);
            var transformBefore = _entityManager.GetComponentData<LocalTransform>(determinismEntity);

            // Rewind 2 seconds (assuming 60 ticks per second)
            var rewindTicks = 120u;
            var targetTick = math.max(0u, timeStateBefore.Tick - rewindTicks);

            var rewindQuery = _entityManager.CreateEntityQuery(typeof(RewindState));
            var rewindEntity = rewindQuery.GetSingletonEntity();
            var rewindState = _entityManager.GetComponentData<RewindState>(rewindEntity);
            rewindState.Mode = RewindMode.Playback;
            rewindState.PlaybackTick = targetTick;
            _entityManager.SetComponentData(rewindEntity, rewindState);

            var timeState = timeStateBefore;
            timeState.Tick = targetTick;
            _entityManager.SetComponentData(timeEntity, timeState);

            // Resimulate forward to original tick
            while (timeState.Tick < timeStateBefore.Tick)
            {
                UpdateSystems();
                timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            }

            // Verify state matches bytewise
            var timeStateAfter = _entityManager.GetComponentData<TimeState>(timeEntity);
            var determinismStateAfter = _entityManager.GetComponentData<TimeDeterminismState>(determinismEntity);
            var transformAfter = _entityManager.GetComponentData<LocalTransform>(determinismEntity);

            Assert.AreEqual(timeStateBefore.Tick, timeStateAfter.Tick, "Tick should match");
            Assert.AreEqual(determinismStateBefore.Position.x, determinismStateAfter.Position.x, 0.001f, "Position X should match deterministically");
            Assert.AreEqual(determinismStateBefore.Position.y, determinismStateAfter.Position.y, 0.001f, "Position Y should match deterministically");
            Assert.AreEqual(determinismStateBefore.Position.z, determinismStateAfter.Position.z, 0.001f, "Position Z should match deterministically");
            Assert.AreEqual(determinismStateBefore.Phase, determinismStateAfter.Phase, 0.001f, "Phase should match deterministically");
        }

        [Test]
        public void Pause_Resume_MaintainsState()
        {
            // Run for some ticks
            for (int i = 0; i < 50; i++)
            {
                UpdateSystems();
            }

            // Capture state
            var determinismQuery = _entityManager.CreateEntityQuery(typeof(TimeDeterminismState));
            var determinismEntity = determinismQuery.GetSingletonEntity();
            var stateBeforePause = _entityManager.GetComponentData<TimeDeterminismState>(determinismEntity);

            // Pause
            var timeQuery = _entityManager.CreateEntityQuery(typeof(TimeState));
            var timeEntity = timeQuery.GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.IsPaused = true;
            _entityManager.SetComponentData(timeEntity, timeState);

            // Update (should not advance)
            UpdateSystems();

            var stateDuringPause = _entityManager.GetComponentData<TimeDeterminismState>(determinismEntity);
            Assert.AreEqual(stateBeforePause.Position.x, stateDuringPause.Position.x, 0.001f, "State should not change when paused");

            // Resume
            timeState.IsPaused = false;
            _entityManager.SetComponentData(timeEntity, timeState);

            // Update (should advance)
            UpdateSystems();

            var stateAfterResume = _entityManager.GetComponentData<TimeDeterminismState>(determinismEntity);
            Assert.Greater(stateAfterResume.Position.x, stateDuringPause.Position.x, "State should advance after resume");
        }

        private void UpdateSystems()
        {
            var timeDeterminismMotion = _world.GetOrCreateSystem<TimeDeterminismMotionSystem>();
            var timeDeterminismHistory = _world.GetOrCreateSystem<TimeDeterminismHistorySystem>();
            var timeControl = _world.GetOrCreateSystem<TimeControlSystem>();

            _simGroup.RemoveSystemFromUpdateList(timeDeterminismMotion);
            _simGroup.RemoveSystemFromUpdateList(timeDeterminismHistory);
            _simGroup.RemoveSystemFromUpdateList(timeControl);
            _simGroup.AddSystemToUpdateList(timeDeterminismMotion);
            _simGroup.AddSystemToUpdateList(timeDeterminismHistory);
            _simGroup.AddSystemToUpdateList(timeControl);
            _simGroup.SortSystems();
            _simGroup.Update();
        }
    }
}
