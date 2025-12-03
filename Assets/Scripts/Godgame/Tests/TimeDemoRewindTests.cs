using Godgame.Time;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using PureDOTS.Systems.Input;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests
{
    public class TimeDemoRewindTests
    {
        [Test]
        public void TimeDemoActor_RewindPlayback_RestoresStateDeterministically()
        {
            var world = new World("TimeDemoRewindTests");
            try
            {
                var entityManager = world.EntityManager;
                CoreSingletonBootstrapSystem.EnsureSingletons(entityManager);

                var timeEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
                var timeState = entityManager.GetComponentData<TimeState>(timeEntity);
                timeState.FixedDeltaTime = 0.1f;
                timeState.CurrentSpeedMultiplier = 1f;
                timeState.IsPaused = false;
                entityManager.SetComponentData(timeEntity, timeState);

                var tickState = entityManager.GetComponentData<TickTimeState>(timeEntity);
                tickState.FixedDeltaTime = timeState.FixedDeltaTime;
                tickState.CurrentSpeedMultiplier = 1f;
                tickState.IsPaused = false;
                tickState.IsPlaying = true;
                tickState.TargetTick = tickState.Tick;
                entityManager.SetComponentData(timeEntity, tickState);

                var rewindEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>()).GetSingletonEntity();
                var rewindState = entityManager.GetComponentData<RewindState>(rewindEntity);
                rewindState.Mode = RewindMode.Record;
                entityManager.SetComponentData(rewindEntity, rewindState);

                var bootstrap = world.GetOrCreateSystem<TimeDemoBootstrapSystem>();
                bootstrap.Update(world.Unmanaged);

                var motion = world.GetOrCreateSystem<TimeDemoMotionSystem>();
                var history = world.GetOrCreateSystem<TimeDemoHistorySystem>();
                var timeInput = world.GetOrCreateSystem<TimeControlInputSystem>();
                var rewindCoordinator = world.GetOrCreateSystem<RewindCoordinatorSystem>();
                var timeTick = world.GetOrCreateSystem<TimeTickSystem>();

                var demoEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeDemoState>()).GetSingletonEntity();
                var config = entityManager.GetComponentData<TimeDemoConfig>(demoEntity);
                var timeControlEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeControlInputState>()).GetSingletonEntity();

                float elapsed = 0f;
                void StepFrame(float deltaTime)
                {
                    elapsed += deltaTime;
                    world.Unmanaged.Time = new TimeData(elapsed, deltaTime);
                    rewindCoordinator.Update(world.Unmanaged);
                    timeTick.Update(world.Unmanaged);
                    motion.Update(world.Unmanaged);
                    history.Update(world.Unmanaged);
                }

                // Advance a few ticks to populate history.
                for (int i = 0; i < 12; i++)
                {
                    StepFrame(timeState.FixedDeltaTime);
                }

                var preRewindState = entityManager.GetComponentData<TimeDemoState>(demoEntity);
                var startTick = entityManager.GetComponentData<TimeState>(timeEntity).Tick;

                // Simulate holding R via the time control input state.
                entityManager.SetComponentData(timeControlEntity, new TimeControlInputState
                {
                    SampleTick = startTick,
                    RewindHeld = 1,
                    RewindPressedThisFrame = 1,
                    RewindSpeedLevel = 1,
                    EnterGhostPreview = 0,
                    StepDownTriggered = 0,
                    StepUpTriggered = 0,
                    PauseToggleTriggered = 0
                });
                timeInput.Update(world.Unmanaged);

                // Run until rewind playback and catch-up complete.
                for (int i = 0; i < 64; i++)
                {
                    StepFrame(timeState.FixedDeltaTime);
                    rewindState = entityManager.GetComponentData<RewindState>(rewindEntity);
                    if (rewindState.Mode == RewindMode.Record)
                    {
                        break;
                    }
                }

                Assert.AreEqual(RewindMode.Record, entityManager.GetComponentData<RewindState>(rewindEntity).Mode);
                var postRewindState = entityManager.GetComponentData<TimeDemoState>(demoEntity);
                Assert.AreEqual(preRewindState.Position, postRewindState.Position);
                Assert.AreEqual(preRewindState.Phase, postRewindState.Phase);
                Assert.AreEqual(preRewindState.LastAppliedTick, postRewindState.LastAppliedTick);

                // Command log should contain the rewind request.
                var commandLogState = entityManager.GetComponentData<InputCommandLogState>(timeEntity);
                var commandBuffer = entityManager.GetBuffer<InputCommandLogEntry>(timeEntity);
                Assert.Greater(commandLogState.Count, 0);
                var lastIndex = (commandLogState.StartIndex + commandLogState.Count - 1) % commandBuffer.Length;
                Assert.AreEqual((byte)TimeControlCommandType.StartRewind, commandBuffer[lastIndex].Type);

                // Resume and advance a few deterministic ticks; state should keep the same trajectory.
                const int ticksAfterResume = 4;
                for (int i = 0; i < ticksAfterResume; i++)
                {
                    StepFrame(timeState.FixedDeltaTime);
                }

                var resumedState = entityManager.GetComponentData<TimeDemoState>(demoEntity);
                var expectedPosition = preRewindState.Position + config.VelocityPerSecond * timeState.FixedDeltaTime * ticksAfterResume;
                var expectedPhase = Repeat(preRewindState.Phase + config.PhaseRadiansPerSecond * timeState.FixedDeltaTime * ticksAfterResume, math.PI * 2f);
                Assert.IsTrue(math.all(expectedPosition == resumedState.Position));
                Assert.AreEqual(expectedPhase, resumedState.Phase);
            }
            finally
            {
                if (world.IsCreated)
                {
                    world.Dispose();
                }
            }
        }

        private static float Repeat(float t, float length)
        {
            if (length == 0f)
            {
                return 0f;
            }

            return t - math.floor(t / length) * length;
        }
    }
}
