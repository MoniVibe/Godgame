using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Transport;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Registry
{
    /// <summary>
    /// Verifies logistics requests flow into the shared registry with deterministic metadata.
    /// </summary>
    public class GodgameLogisticsRegistryTests
    {
        [Test]
        public void LogisticsRequests_AppearInRegistry_WithTimestamps()
        {
            using var world = new World("GodgameLogisticsRegistryTests");
            var entityManager = world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(entityManager);
            world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            // Configure time/rewind to record mode.
            var timeEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var time = entityManager.GetComponentData<TimeState>(timeEntity);
            time.Tick = 10;
            time.IsPaused = false;
            entityManager.SetComponentData(timeEntity, time);

            var rewindEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>()).GetSingletonEntity();
            var rewind = entityManager.GetComponentData<RewindState>(rewindEntity);
            rewind.Mode = RewindMode.Record;
            entityManager.SetComponentData(rewindEntity, rewind);

            var requestEntity = entityManager.CreateEntity(typeof(LogisticsRequest), typeof(LogisticsRequestProgress));
            entityManager.SetComponentData(requestEntity, new LogisticsRequest
            {
                SourceEntity = Entity.Null,
                DestinationEntity = Entity.Null,
                SourcePosition = new float3(1f, 0f, 1f),
                DestinationPosition = new float3(4f, 0f, 4f),
                ResourceTypeIndex = 2,
                RequestedUnits = 20f,
                FulfilledUnits = 5f,
                Priority = LogisticsRequestPriority.High,
                Flags = LogisticsRequestFlags.Urgent,
                CreatedTick = 1,
                LastUpdateTick = 1
            });

            entityManager.SetComponentData(requestEntity, new LogisticsRequestProgress
            {
                AssignedUnits = 3f,
                AssignedTransportCount = 1,
                LastAssignmentTick = 9
            });

            var logisticsSync = world.GetOrCreateSystem<GodgameLogisticsSyncSystem>();
            var logisticsRegistry = world.GetOrCreateSystem<LogisticsRequestRegistrySystem>();

            var simGroup = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
            simGroup.RemoveSystemFromUpdateList(logisticsSync);
            simGroup.RemoveSystemFromUpdateList(logisticsRegistry);
            simGroup.AddSystemToUpdateList(logisticsSync);
            simGroup.AddSystemToUpdateList(logisticsRegistry);
            simGroup.SortSystems();
            simGroup.Update();

            var registryEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<LogisticsRequestRegistry>())
                .GetSingletonEntity();
            var registry = entityManager.GetComponentData<LogisticsRequestRegistry>(registryEntity);
            Assert.AreEqual(1, registry.TotalRequests);
            Assert.AreEqual(1, registry.InProgressRequests);
            Assert.AreEqual(time.Tick, registry.LastUpdateTick);

            var metadata = entityManager.GetComponentData<RegistryMetadata>(registryEntity);
            Assert.AreEqual(GodgameRegistryIds.LogisticsRequestArchetype, metadata.ArchetypeId);
            Assert.AreEqual(1, metadata.EntryCount);

            var entries = entityManager.GetBuffer<LogisticsRequestRegistryEntry>(registryEntity);
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(requestEntity, entries[0].RequestEntity);
            Assert.AreEqual(2, entries[0].ResourceTypeIndex);
            Assert.AreEqual(LogisticsRequestPriority.High, entries[0].Priority);
            Assert.IsTrue((entries[0].Flags & LogisticsRequestFlags.Urgent) != 0);
            Assert.AreEqual(time.Tick, entries[0].LastUpdateTick);
        }
    }
}
