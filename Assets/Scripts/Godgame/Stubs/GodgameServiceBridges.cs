// [TRI-STUB] This is an ahead-of-time stub. Safe to compile, does nothing at runtime.
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Diplomacy;
using PureDOTS.Runtime.Economy;
using PureDOTS.Runtime.Navigation;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Narrative;
using PureDOTS.Runtime.Persistence;
using PureDOTS.Runtime.Sensors;
using PureDOTS.Runtime.TimeControl;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Stubs
{
    public static class GodgameNavigationBridgeStub
    {
        public static Entity RequestVillagerPath(ref SystemState state, in Entity villager, in float3 start, in float3 destination)
        {
            return NavigationServiceStub.RequestPath(ref state, in villager, in start, in destination);
        }
    }

    public static class GodgameCombatBridgeStub
    {
        public static void ScheduleBandEngagement(in Entity attacker, in Entity defender)
        {
            CombatServiceStub.ScheduleEngagement(in attacker, in defender);
        }
    }

    public static class GodgameEconomyBridgeStub
    {
        public static void QueueWorkshopRecipe(in Entity workshop, int recipeId)
        {
            EconomyServiceStub.EnqueueProduction(in workshop, recipeId);
        }
    }

    public static class GodgameDiplomacyBridgeStub
    {
        public static void ApplyVillageRelationDelta(in Entity villageA, in Entity villageB, float delta)
        {
            DiplomacyServiceStub.ApplyRelationDelta(in villageA, in villageB, delta);
        }
    }

    public static class GodgameTelemetryBridgeStub
    {
        public static void LogMiracleMetric(FixedString64Bytes metric, float value)
        {
            TelemetryBridgeStub.RecordMetric(metric, value);
        }
    }

    public static class GodgameSensorBridgeStub
    {
        public static void RegisterRig(in Entity entity, byte channelsMask)
        {
            SensorServiceStub.RegisterRig(in entity, channelsMask);
        }

        public static void SubmitInterrupt(in Entity entity, byte category)
        {
            SensorServiceStub.SubmitInterrupt(in entity, category);
        }
    }

    public static class GodgameTimeBridgeStub
    {
        public static void RequestPause() => TimeControlServiceStub.RequestPause();
        public static void RequestResume() => TimeControlServiceStub.RequestResume();
    }

    public static class GodgameNarrativeBridgeStub
    {
        public static void RaiseEvent(int situationId, int eventId)
        {
            NarrativeServiceStub.RaiseEvent(situationId, eventId);
        }
    }

    public static class GodgameSaveLoadBridgeStub
    {
        public static SnapshotHandle RequestSave(ref SystemState state)
        {
            return SaveLoadServiceStub.RequestSave(ref state);
        }
    }
}
