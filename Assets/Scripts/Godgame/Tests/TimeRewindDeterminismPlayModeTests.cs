// Only compile in test-enabled builds
#if UNITY_INCLUDE_TESTS && GODGAME_TESTS
using System.Collections;
using Godgame.Logistics;
using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Transport;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.TestTools;
using EntitiesPresentationSystemGroup = Unity.Entities.PresentationSystemGroup;

namespace Godgame.Tests
{
    public class TimeRewindDeterminismPlayModeTests
    {
        private struct TestContext : System.IDisposable
        {
            public World World;
            public InitializationSystemGroup InitGroup;
            public SimulationSystemGroup SimulationGroup;
            public EntitiesPresentationSystemGroup PresentationGroup;
            public FixedStepSimulationSystemGroup FixedStepGroup;

            public TestContext(
                World world,
                InitializationSystemGroup initGroup,
                SimulationSystemGroup simulationGroup,
                EntitiesPresentationSystemGroup presentationGroup,
                FixedStepSimulationSystemGroup fixedStepGroup)
            {
                World = world;
                InitGroup = initGroup;
                SimulationGroup = simulationGroup;
                PresentationGroup = presentationGroup;
                FixedStepGroup = fixedStepGroup;
            }

            public void Dispose()
            {
                if (World == World.DefaultGameObjectInjectionWorld)
                {
                    World.DefaultGameObjectInjectionWorld = null;
                }

                World.Dispose();
            }
        }

        private struct LogisticsSnapshot : System.IDisposable
        {
            public LogisticsRequestRegistry Registry;
            public RegistryMetadata Metadata;
            public NativeArray<LogisticsRequestRegistryEntry> Entries;

            public void Dispose()
            {
                if (Entries.IsCreated)
                {
                    Entries.Dispose();
                }
            }
        }

        [UnityTest]
        public IEnumerator RewindDisablesLogisticsWritesDuringPlayback()
        {
            using var ctx = CreateContext();
            CreateTransportOrder(ctx.World, new float3(1f, 0f, 2f), new float3(4f, 0f, -3f), requestedUnits: 12f);

            AdvanceSeveralFrames(ctx, 4);
            var beforeRewind = CaptureLogisticsSnapshot(ctx.World, Allocator.Temp);

            var currentTick = GetTick(ctx.World);
            var targetTick = currentTick > 2 ? currentTick - 2u : 0u;
            AddCommand(ctx.World, TimeControlCommand.CommandType.StartRewind, targetTick);

            AdvanceFrame(ctx, 1d / 30d);
            var rewindState = GetRewindState(ctx.World);
            Assert.AreEqual(RewindMode.Playback, rewindState.Mode, "Rewind should enter playback mode.");
            Assert.IsFalse(ctx.SimulationGroup.Enabled, "Simulation should be disabled during playback.");

            var duringPlayback = CaptureLogisticsSnapshot(ctx.World, Allocator.Temp);
            Assert.AreEqual(beforeRewind.Metadata.LastUpdateTick, duringPlayback.Metadata.LastUpdateTick, "Registry writes should pause during playback.");
            Assert.AreEqual(beforeRewind.Metadata.EntryCount, duringPlayback.Metadata.EntryCount, "Registry entry count should remain stable while rewinding.");

            beforeRewind.Dispose();
            duringPlayback.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator LogisticsRegistryRemainsDeterministicAfterRewindAndCatchUp()
        {
            using var ctx = CreateContext();
            CreateTransportOrder(ctx.World, new float3(2f, 0f, 2f), new float3(-2f, 0f, 6f), requestedUnits: 10f, fulfilledUnits: 3f, priority: TransportPriority.Critical, flags: TransportFlags.Urgent, assignedUnits: 4f);
            CreateTransportOrder(ctx.World, new float3(-3f, 0f, -1f), new float3(5f, 0f, 5f), requestedUnits: 6f, fulfilledUnits: 0f, priority: TransportPriority.High, flags: TransportFlags.PlayerPinned);

            AdvanceSeveralFrames(ctx, 6);
            var initialSnapshot = CaptureLogisticsSnapshot(ctx.World, Allocator.Temp);
            var initialTick = GetTick(ctx.World);

            var rewindTarget = initialTick > 3 ? initialTick - 3u : 0u;
            AddCommand(ctx.World, TimeControlCommand.CommandType.StartRewind, rewindTarget);

            var safety = 0;
            while (GetRewindState(ctx.World).Mode == RewindMode.Playback && safety++ < 240)
            {
                AdvanceFrame(ctx, 1d / 120d);
            }

            while (GetRewindState(ctx.World).Mode == RewindMode.CatchUp && safety++ < 240)
            {
                AdvanceFrame(ctx);
            }

            Assert.Less(safety, 240, "Rewind should complete within safety window.");

            AdvanceFrame(ctx); // Allow registry systems to run after returning to Record

            var finalSnapshot = CaptureLogisticsSnapshot(ctx.World, Allocator.Temp);
            var finalTick = GetTick(ctx.World);

            Assert.AreEqual(initialSnapshot.Metadata.EntryCount, finalSnapshot.Metadata.EntryCount, "Entry counts should remain deterministic after rewind/catch-up.");
            Assert.AreEqual(initialSnapshot.Registry.TotalRequests, finalSnapshot.Registry.TotalRequests);
            Assert.AreEqual(initialSnapshot.Registry.CriticalRequests, finalSnapshot.Registry.CriticalRequests);
            Assert.AreEqual(initialSnapshot.Registry.TotalRequestedUnits, finalSnapshot.Registry.TotalRequestedUnits, 0.0001f);
            Assert.AreEqual(initialSnapshot.Registry.TotalAssignedUnits, finalSnapshot.Registry.TotalAssignedUnits, 0.0001f);
            Assert.AreEqual(initialSnapshot.Registry.TotalRemainingUnits, finalSnapshot.Registry.TotalRemainingUnits, 0.0001f);

            Assert.AreEqual(initialSnapshot.Entries.Length, finalSnapshot.Entries.Length, "Entry buffers should remain ordered and stable.");
            for (var i = 0; i < initialSnapshot.Entries.Length; i++)
            {
                var before = initialSnapshot.Entries[i];
                var after = finalSnapshot.Entries[i];

                Assert.AreEqual(before.RequestEntity, after.RequestEntity);
                Assert.AreEqual(before.SourceEntity, after.SourceEntity);
                Assert.AreEqual(before.DestinationEntity, after.DestinationEntity);
                Assert.AreEqual(before.ResourceTypeIndex, after.ResourceTypeIndex);
                Assert.AreEqual(before.Priority, after.Priority);
                Assert.AreEqual(before.Flags, after.Flags);
                Assert.AreEqual(before.SourceCellId, after.SourceCellId);
                Assert.AreEqual(before.DestinationCellId, after.DestinationCellId);
                Assert.AreEqual(before.SpatialVersion, after.SpatialVersion);
                Assert.AreEqual(before.RequestedUnits, after.RequestedUnits, 0.0001f);
                Assert.AreEqual(before.AssignedUnits, after.AssignedUnits, 0.0001f);
                Assert.AreEqual(before.RemainingUnits, after.RemainingUnits, 0.0001f);
            }

            Assert.AreEqual(finalTick, finalSnapshot.Metadata.LastUpdateTick, "Registry metadata should reflect the latest tick after catch-up.");
            Assert.AreEqual(initialSnapshot.Metadata.Continuity.SpatialResolvedCount, finalSnapshot.Metadata.Continuity.SpatialResolvedCount);
            Assert.AreEqual(initialSnapshot.Metadata.Continuity.SpatialFallbackCount, finalSnapshot.Metadata.Continuity.SpatialFallbackCount);
            Assert.AreEqual(initialSnapshot.Metadata.Continuity.SpatialUnmappedCount, finalSnapshot.Metadata.Continuity.SpatialUnmappedCount);

            initialSnapshot.Dispose();
            finalSnapshot.Dispose();
            yield return null;
        }

        private static TestContext CreateContext()
        {
            var world = new World("RewindPlaymodeWorld", WorldFlags.Game);
            World.DefaultGameObjectInjectionWorld = world;

            var initGroup = world.GetOrCreateSystemManaged<InitializationSystemGroup>();
            var simulationGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
            var presentationGroup = world.GetExistingSystemManaged<EntitiesPresentationSystemGroup>();
            var fixedStepGroup = world.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
            if (fixedStepGroup != null)
            {
                fixedStepGroup.Timestep = 1f / 60f;
            }

            world.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
            world.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
            world.GetOrCreateSystemManaged<TransportPhaseGroup>();

            world.CreateSystem<CoreSingletonBootstrapSystem>();
            world.CreateSystem<TimeSettingsConfigSystem>();
            world.CreateSystem<HistorySettingsConfigSystem>();
            world.CreateSystem<TimeLogConfigSystem>();
            world.CreateSystem<RewindCoordinatorSystem>();
            world.CreateSystem<SimulationTickGateSystem>();
            world.CreateSystem<TimeTickSystem>();
            world.CreateSystem<GameplayFixedStepSyncSystem>();
            world.CreateSystem<TickSnapshotLogSystem>();

            world.CreateSystem<RegistryDirectorySystem>();
            world.CreateSystem<GodgameLogisticsSyncSystem>();
            world.CreateSystem<LogisticsRequestRegistrySystem>();

            initGroup.SortSystems();
            world.GetExistingSystemManaged<TimeSystemGroup>()?.SortSystems();
            world.GetExistingSystemManaged<TransportPhaseGroup>()?.SortSystems();
            fixedStepGroup?.SortSystems();
            simulationGroup.SortSystems();
            presentationGroup?.SortSystems();

            return new TestContext(world, initGroup, simulationGroup, presentationGroup, fixedStepGroup);
        }

        private static void AdvanceFrame(TestContext ctx, double deltaTime = 1d / 60d)
        {
            var timeData = ctx.World.Time;
            var newElapsed = (float)(timeData.ElapsedTime + deltaTime);
            ctx.World.SetTime(new TimeData((float)deltaTime, newElapsed));

            ctx.InitGroup.Update();
            if (ctx.FixedStepGroup != null && ctx.FixedStepGroup.Enabled)
            {
                ctx.FixedStepGroup.Update();
            }

            if (ctx.SimulationGroup.Enabled)
            {
                ctx.SimulationGroup.Update();
            }

            if (ctx.PresentationGroup != null && ctx.PresentationGroup.Enabled)
            {
                ctx.PresentationGroup.Update();
            }
        }

        private static void AdvanceSeveralFrames(TestContext ctx, int frameCount, double deltaTime = 1d / 60d)
        {
            for (var i = 0; i < frameCount; i++)
            {
                AdvanceFrame(ctx, deltaTime);
            }
        }

        private static void AddCommand(World world, TimeControlCommand.CommandType type, uint uintParam = 0, float floatParam = 0f)
        {
            var rewindEntity = world.EntityManager.CreateEntityQuery(typeof(RewindState)).GetSingletonEntity();
            var buffer = world.EntityManager.GetBuffer<TimeControlCommand>(rewindEntity);
            buffer.Add(new TimeControlCommand
            {
                Type = type,
                UintParam = uintParam,
                FloatParam = floatParam
            });
        }

        private static Entity CreateTransportOrder(
            World world,
            float3 source,
            float3 destination,
            ushort resourceType = 0,
            float requestedUnits = 5f,
            float fulfilledUnits = 0f,
            TransportPriority priority = TransportPriority.Normal,
            TransportFlags flags = TransportFlags.None,
            float assignedUnits = 0f)
        {
            var entityManager = world.EntityManager;
            var entity = entityManager.CreateEntity(typeof(TransportOrder));
            entityManager.SetComponentData(entity, new TransportOrder
            {
                SourceEntity = Entity.Null,
                DestinationEntity = Entity.Null,
                SourcePosition = source,
                DestinationPosition = destination,
                ResourceTypeIndex = resourceType,
                RequestedUnits = requestedUnits,
                FulfilledUnits = fulfilledUnits,
                Priority = priority,
                Flags = flags,
                CreatedTick = 0,
                LastUpdateTick = 0,
                PreferredVehicle = TransportVehicleType.Wagon,
                MaxDistance = 256f,
                OrderLabel = new Unity.Collections.FixedString64Bytes("TestOrder")
            });
            entityManager.AddComponent<RewindableTag>(entity);

            if (assignedUnits > 0f)
            {
                entityManager.AddComponentData(entity, new LogisticsRequestProgress
                {
                    AssignedUnits = assignedUnits,
                    AssignedTransportCount = 1,
                    LastAssignmentTick = 0
                });
            }

            return entity;
        }

        private static LogisticsSnapshot CaptureLogisticsSnapshot(World world, Allocator allocator)
        {
            var registryEntity = world.EntityManager.CreateEntityQuery(typeof(LogisticsRequestRegistry)).GetSingletonEntity();
            var registry = world.EntityManager.GetComponentData<LogisticsRequestRegistry>(registryEntity);
            var metadata = world.EntityManager.GetComponentData<RegistryMetadata>(registryEntity);
            var entries = world.EntityManager.GetBuffer<LogisticsRequestRegistryEntry>(registryEntity).ToNativeArray(allocator);

            return new LogisticsSnapshot
            {
                Registry = registry,
                Metadata = metadata,
                Entries = entries
            };
        }

        private static uint GetTick(World world)
        {
            return world.EntityManager.CreateEntityQuery(typeof(TickTimeState)).GetSingleton<TickTimeState>().Tick;
        }

        private static RewindState GetRewindState(World world)
        {
            return world.EntityManager.CreateEntityQuery(typeof(RewindState)).GetSingleton<RewindState>();
        }
    }
}
#endif
