#if UNITY_INCLUDE_TESTS
using System;
using Godgame.Registry;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Tests
{
    public class GodgameRegistryDependencyTests
    {
        [Test]
        public void SystemGroups_ExistUnderSimulation()
        {
            Assert.IsTrue(HasUpdateInGroup<SpatialSystemGroup, SimulationSystemGroup>(), "SpatialSystemGroup should live under SimulationSystemGroup.");
            Assert.IsTrue(HasUpdateInGroup<GameplaySystemGroup, SimulationSystemGroup>(), "GameplaySystemGroup should live under SimulationSystemGroup.");
            Assert.IsTrue(HasUpdateInGroup<TransportPhaseGroup, SimulationSystemGroup>(), "TransportPhaseGroup should live under SimulationSystemGroup.");
        }

        [Test]
        public void SyncSystems_DeclareOrderingDependencies()
        {
            Assert.IsTrue(HasUpdateBefore<GodgameBandSyncSystem, SpatialSystemGroup>(), "Band sync should precede spatial resolution.");
            Assert.IsTrue(HasUpdateBefore<GodgameLogisticsSyncSystem, TransportPhaseGroup>(), "Logistics sync should precede transport phases.");
            Assert.IsTrue(HasUpdateBefore<GodgameMiracleSyncSystem, GameplaySystemGroup>(), "Miracle sync should precede gameplay execution.");
        }

        [Test]
        public void VillagerLessonTelemetry_AddsAggregates()
        {
            using var world = new World("LessonTelemetryTest");
            var telemetryEntity = world.EntityManager.CreateEntity();
            world.EntityManager.AddBuffer<TelemetryMetric>(telemetryEntity);
            var telemetryBuffer = world.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);

            var lessonsEntity = world.EntityManager.CreateEntity();
            var lessons = world.EntityManager.AddBuffer<VillagerLessonRegistryEntry>(lessonsEntity);
            lessons.Add(new VillagerLessonRegistryEntry
            {
                VillagerEntity = Entity.Null,
                VillagerId = 1,
                AxisId = new FixedString64Bytes("discipline"),
                Progress = 1f
            });
            lessons.Add(new VillagerLessonRegistryEntry
            {
                VillagerEntity = Entity.Null,
                VillagerId = 2,
                AxisId = new FixedString64Bytes("discipline"),
                Progress = 0.5f
            });
            lessons.Add(new VillagerLessonRegistryEntry
            {
                VillagerEntity = Entity.Null,
                VillagerId = 3,
                AxisId = new FixedString64Bytes("craft"),
                Progress = 0.25f
            });
            lessons.Add(new VillagerLessonRegistryEntry
            {
                VillagerEntity = Entity.Null,
                VillagerId = 4,
                AxisId = default,
                Progress = 0.8f
            });

            VillagerLessonTelemetry.AddMetrics(ref telemetryBuffer, lessons, new FixedString64Bytes("gg"));

            AssertMetricEquals(telemetryBuffer, "gg.villagers.lessons.total", 4d, TelemetryMetricUnit.Count);
            AssertMetricEquals(telemetryBuffer, "gg.villagers.lessons.completed", 1d, TelemetryMetricUnit.Count);
            AssertMetricEquals(telemetryBuffer, "gg.villagers.lessons.axis.discipline", 0.75d, TelemetryMetricUnit.Ratio);
            AssertMetricEquals(telemetryBuffer, "gg.villagers.lessons.axis.craft", 0.25d, TelemetryMetricUnit.Ratio);
        }

        private static bool HasUpdateInGroup<TSystem, TGroup>()
        {
            var attributes = Attribute.GetCustomAttributes(typeof(TSystem), typeof(UpdateInGroupAttribute));
            foreach (var attribute in attributes)
            {
                if (attribute is UpdateInGroupAttribute updateInGroup && updateInGroup.GroupType == typeof(TGroup))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasUpdateBefore<TSystem, TTarget>()
        {
            var attributes = Attribute.GetCustomAttributes(typeof(TSystem), typeof(UpdateBeforeAttribute));
            foreach (var attribute in attributes)
            {
                if (attribute is UpdateBeforeAttribute updateBefore && updateBefore.SystemType == typeof(TTarget))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertMetricEquals(
            DynamicBuffer<TelemetryMetric> buffer,
            string key,
            double expectedValue,
            TelemetryMetricUnit expectedUnit)
        {
            var fixedKey = new FixedString64Bytes(key);
            for (var i = 0; i < buffer.Length; i++)
            {
                var metric = buffer[i];
                if (metric.Key.Equals(fixedKey))
                {
                    Assert.AreEqual(expectedValue, metric.Value);
                    Assert.AreEqual(expectedUnit, metric.Unit);
                    return;
                }
            }

            Assert.Fail($"Metric '{key}' not found in telemetry buffer.");
        }
    }
}
#endif
