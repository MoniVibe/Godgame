using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Intent;
using PureDOTS.Runtime.Interrupts;
using PureDOTS.Systems;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Systems
{
    /// <summary>
    /// Bridges PureDOTS EntityIntent to Godgame VillagerAIState.
    /// Consumes interrupt-driven intents and maps them to villager goals.
    /// Runs after InterruptSystemGroup.
    /// 
    /// Note: QueuedIntent buffer support is optional. Entities can opt-in by adding
    /// DynamicBuffer&lt;QueuedIntent&gt; to enable queued intent promotion via IntentProcessingSystem.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(VillagerSystemGroup))]
    [UpdateAfter(typeof(InterruptSystemGroup))]
    public partial struct GodgameVillagerIntentBridgeSystem : ISystem
    {
        private ComponentLookup<VillagerAIState> _aiStateLookup;
        private EntityStorageInfoLookup _entityStorageInfoLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            _aiStateLookup = state.GetComponentLookup<VillagerAIState>(false);
            _entityStorageInfoLookup = state.GetEntityStorageInfoLookup();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton<RewindState>(out var rewindState) || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            state.Dependency.Complete();
            _aiStateLookup.Update(ref state);
            _entityStorageInfoLookup.Update(ref state);

            // Process all entities with EntityIntent and VillagerAIState
            foreach (var (intent, entity) in
                SystemAPI.Query<RefRW<EntityIntent>>()
                .WithAll<VillagerAIState>()
                .WithEntityAccess())
            {
                // Skip if intent is invalid or idle
                if (intent.ValueRO.IsValid == 0 || intent.ValueRO.Mode == IntentMode.Idle)
                {
                    // If intent is invalid and AI state is idle, clear intent
                    if (intent.ValueRO.IsValid == 0)
                    {
                        var aiStateCheck = _aiStateLookup[entity];
                        if (aiStateCheck.CurrentGoal == VillagerAIState.Goal.None && aiStateCheck.CurrentState == VillagerAIState.State.Idle)
                        {
                            IntentService.ClearIntent(ref intent.ValueRW);
                        }
                    }
                    continue;
                }

                if (!_aiStateLookup.HasComponent(entity))
                {
                    continue;
                }

                var aiState = _aiStateLookup[entity];
                var newGoal = MapIntentToGoal(intent.ValueRO);

                // Only update if goal changed or if we need to update target
                bool shouldUpdate = false;

                if (newGoal != VillagerAIState.Goal.None && newGoal != aiState.CurrentGoal)
                {
                    aiState.CurrentGoal = newGoal;
                    aiState.CurrentState = GoalToState(newGoal);
                    aiState.StateTimer = 0f;
                    aiState.StateStartTick = timeState.Tick;
                    shouldUpdate = true;
                }

                // Update target if provided
                if (intent.ValueRO.TargetEntity != Entity.Null && intent.ValueRO.TargetEntity != aiState.TargetEntity)
                {
                    aiState.TargetEntity = intent.ValueRO.TargetEntity;
                    shouldUpdate = true;
                }

                if (math.any(intent.ValueRO.TargetPosition != float3.zero) && math.distance(intent.ValueRO.TargetPosition, aiState.TargetPosition) > 0.1f)
                {
                    aiState.TargetPosition = intent.ValueRO.TargetPosition;
                    shouldUpdate = true;
                }

                if (shouldUpdate)
                {
                    _aiStateLookup[entity] = aiState;
                }

                // Check if intent should be cleared (goal completed or invalid)
                if (ShouldClearIntent(intent.ValueRO, aiState, ref _entityStorageInfoLookup))
                {
                    IntentService.ClearIntent(ref intent.ValueRW);
                }
            }
        }

        /// <summary>
        /// Maps IntentMode to VillagerAIState.Goal.
        /// </summary>
        [BurstCompile]
        private static VillagerAIState.Goal MapIntentToGoal(in EntityIntent intent)
        {
            return intent.Mode switch
            {
                IntentMode.Idle => VillagerAIState.Goal.None,
                IntentMode.MoveTo => intent.TargetEntity != Entity.Null || math.any(intent.TargetPosition != float3.zero) 
                    ? VillagerAIState.Goal.Work 
                    : VillagerAIState.Goal.None,
                IntentMode.Attack => VillagerAIState.Goal.Fight,
                IntentMode.Flee => VillagerAIState.Goal.Flee,
                IntentMode.Gather => VillagerAIState.Goal.Work,
                IntentMode.ExecuteOrder => VillagerAIState.Goal.Work,
                IntentMode.Defend => VillagerAIState.Goal.Fight,
                IntentMode.Patrol => VillagerAIState.Goal.Work, // Patrol as work variant
                IntentMode.Follow => VillagerAIState.Goal.Work, // Follow as work variant
                IntentMode.UseAbility => VillagerAIState.Goal.Fight, // Assume combat ability (most common use case)
                IntentMode.Build => VillagerAIState.Goal.Work, // Construction is work
                // Custom modes are intentionally mapped to None - game-specific systems should handle these
                IntentMode.Custom0 => VillagerAIState.Goal.None, // Game-specific handling required
                IntentMode.Custom1 => VillagerAIState.Goal.None, // Game-specific handling required
                IntentMode.Custom2 => VillagerAIState.Goal.None, // Game-specific handling required
                IntentMode.Custom3 => VillagerAIState.Goal.None, // Game-specific handling required
                _ => VillagerAIState.Goal.None
            };
        }

        /// <summary>
        /// Maps VillagerAIState.Goal to VillagerAIState.State.
        /// </summary>
        [BurstCompile]
        private static VillagerAIState.State GoalToState(VillagerAIState.Goal goal)
        {
            return goal switch
            {
                VillagerAIState.Goal.SurviveHunger => VillagerAIState.State.Eating,
                VillagerAIState.Goal.Work => VillagerAIState.State.Working,
                VillagerAIState.Goal.Rest => VillagerAIState.State.Sleeping,
                VillagerAIState.Goal.Flee => VillagerAIState.State.Fleeing,
                VillagerAIState.Goal.Fight => VillagerAIState.State.Fighting,
                VillagerAIState.Goal.Socialize => VillagerAIState.State.Idle,
                VillagerAIState.Goal.Reproduce => VillagerAIState.State.Idle,
                _ => VillagerAIState.State.Idle
            };
        }

        /// <summary>
        /// Determines if intent should be cleared (goal completed or invalid).
        /// </summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldClearIntent(in EntityIntent intent, in VillagerAIState aiState, ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            // Clear if goal is None and state is Idle (goal completed)
            if (aiState.CurrentGoal == VillagerAIState.Goal.None && aiState.CurrentState == VillagerAIState.State.Idle)
            {
                return true;
            }

            // Clear if target entity was destroyed
            if (intent.TargetEntity != Entity.Null && !entityStorageInfoLookup.Exists(intent.TargetEntity))
            {
                return true; // Target destroyed
            }

            // Don't clear if intent is still valid and goal matches
            return false;
        }
    }
}

