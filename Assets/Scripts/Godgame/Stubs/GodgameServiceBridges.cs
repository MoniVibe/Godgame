// [TRI-STUB] This is an ahead-of-time stub. Safe to compile, does nothing at runtime.
using PureDOTS.Runtime.Behavior;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Diplomacy;
using PureDOTS.Runtime.Economy;
using PureDOTS.Runtime.Communication;
using PureDOTS.Runtime.Trade;
using PureDOTS.Runtime.Navigation;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Narrative;
using PureDOTS.Runtime.Persistence;
using PureDOTS.Runtime.Sensors;
using PureDOTS.Runtime.TimeControl;
using PureDOTS.Runtime.Motivation;
using PureDOTS.Runtime.Decision;
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

    public static class GodgameBehaviorBridgeStub
    {
        public static void ApplyProfile(EntityManager manager, Entity entity, int profileId, float modifier = 0f)
        {
            BehaviorService.ApplyProfile(manager, entity, profileId, modifier);
        }

        public static void RegisterNeed(EntityManager manager, Entity entity, byte needType, float initialSatisfaction = 1f)
        {
            BehaviorService.RegisterNeed(manager, entity, needType, initialSatisfaction);
        }

        public static void ReportNeedDelta(EntityManager manager, Entity entity, float delta)
        {
            BehaviorService.ReportSatisfaction(manager, entity, delta);
        }
    }

    public static class GodgameDecisionBridgeStub
    {
        public static void EnsureTicket(EntityManager manager, Entity entity)
        {
            DecisionServiceStub.EnsureTicket(manager, entity);
        }
    }

    public static class GodgameAmbitionBridgeStub
    {
        public static void RegisterAmbition(EntityManager manager, Entity entity, int ambitionId, byte priority)
        {
            AmbitionServiceStub.RegisterAmbition(manager, entity, ambitionId, priority);
        }

        public static void QueueDesire(EntityManager manager, Entity entity, int desireId, byte priority)
        {
            AmbitionServiceStub.QueueDesire(manager, entity, desireId, priority);
        }
    }

    public static class GodgameCommunicationBridgeStub
    {
        public static void RegisterChannel(EntityManager manager, Entity entity, int channelId, float latencySeconds)
        {
            CommunicationServiceStub.RegisterChannel(manager, entity, channelId, latencySeconds);
        }

        public static void ReportDisruption(EntityManager manager, Entity entity, float severity, float recoveryRate)
        {
            CommunicationServiceStub.ReportDisruption(manager, entity, severity, recoveryRate);
        }
    }

    public static class GodgameTradeBridgeStub
    {
        public static void SetTradeIntent(EntityManager manager, Entity entity, int targetEntityId, byte action)
        {
            TradeServiceStub.SetTradeIntent(manager, entity, targetEntityId, action);
        }
    }
}
