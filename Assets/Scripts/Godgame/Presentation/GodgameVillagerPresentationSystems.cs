using Godgame.Scenario;
using Godgame.Villages;
using LocalNavigation = Godgame.Villagers.Navigation;
using LocalJobPhase = Godgame.Villagers.JobPhase;
using LocalVillagerJobState = Godgame.Villagers.VillagerJobState;
using PureDOTS.Rendering;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_VillagerPresentationSystem : ISystem
    {
        private uint _lastTick;
        private byte _tickInitialized;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerWorkCooldown> _cooldownLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerPresentationTag>();
            state.RequireForUpdate<SettlementVillagerState>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();

            _tickInitialized = 0;
            _lastTick = 0;
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _cooldownLookup = state.GetComponentLookup<VillagerWorkCooldown>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var deltaTime = ResolveDeltaTime(timeState);
            if (deltaTime <= 0f)
            {
                return;
            }

            _transformLookup.Update(ref state);
            _cooldownLookup.Update(ref state);

            const float minDirectionSq = 0.0001f;
            var timeSeconds = timeState.WorldSeconds;

            foreach (var (settlementState, visualState, transform) in SystemAPI
                         .Query<RefRO<SettlementVillagerState>, RefRW<VillagerVisualState>, RefRW<LocalTransform>>()
                         .WithAll<VillagerPresentationTag>())
            {
                var phase = settlementState.ValueRO.Phase;
                bool moving = phase == SettlementVillagerPhase.ToResource || phase == SettlementVillagerPhase.ToDepot;
                bool working = phase == SettlementVillagerPhase.Harvest;

                visualState.ValueRW.TaskState = moving
                    ? (int)VillagerTaskState.Traveling
                    : working
                        ? (int)VillagerTaskState.Working
                        : (int)VillagerTaskState.Idle;
                visualState.ValueRW.AnimationState = moving ? 1 : working ? 2 : 0;
                visualState.ValueRW.EffectIntensity = moving ? 1f : 0f;

                if (!moving)
                {
                    continue;
                }

                var target = phase == SettlementVillagerPhase.ToResource
                    ? settlementState.ValueRO.CurrentResourceNode
                    : settlementState.ValueRO.CurrentDepot;

                if (target == Entity.Null || !_transformLookup.HasComponent(target))
                {
                    continue;
                }

                var current = transform.ValueRO.Position;
                var targetPos = _transformLookup[target].Position;
                targetPos.y = current.y;

                var toTarget = targetPos - current;
                toTarget.y = 0f;
                if (math.lengthsq(toTarget) <= minDirectionSq)
                {
                    continue;
                }

                var rotation = quaternion.LookRotationSafe(math.normalize(toTarget), new float3(0f, 1f, 0f));
                var updated = transform.ValueRO;
                updated.Rotation = rotation;
                transform.ValueRW = updated;
            }

            foreach (var (settlementState, visualState, renderTint, entity) in SystemAPI
                         .Query<RefRO<SettlementVillagerState>, RefRO<VillagerVisualState>, RefRW<RenderTint>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithEntityAccess())
            {
                var baseTint = visualState.ValueRO.AlignmentTint;
                var cooldownMode = _cooldownLookup.HasComponent(entity)
                    ? _cooldownLookup[entity].Mode
                    : VillagerWorkCooldownMode.None;
                var cue = VillagerTaskCueUtility.ResolveCue(settlementState.ValueRO.Phase, cooldownMode);
                renderTint.ValueRW.Value = VillagerTaskCueUtility.ApplyCue(baseTint, cue, timeSeconds, entity.Index);
            }
        }

        private float ResolveDeltaTime(in TimeState timeState)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                return 0f;
            }

            var deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;

            if (deltaTicks == 0u)
            {
                return 0f;
            }

            var fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return fixedDt * deltaTicks;
        }

        public void OnDestroy(ref SystemState state) { }
    }

    /// <summary>
    /// Fallback villager behavior → visuals mapping for non-scenario villagers.
    /// Uses <see cref="VillagerAIState"/> / <see cref="VillagerJob"/> when <see cref="SettlementVillagerState"/> is not present.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillagerPresentationSystem))]
    public partial struct Godgame_VillagerBehaviorVisualFallbackSystem : ISystem
    {
        private uint _lastTick;
        private byte _tickInitialized;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerWorkCooldown> _cooldownLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerPresentationTag>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();

            _tickInitialized = 0;
            _lastTick = 0;
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _cooldownLookup = state.GetComponentLookup<VillagerWorkCooldown>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var deltaTime = ResolveDeltaTime(timeState);
            if (deltaTime <= 0f)
            {
                return;
            }

            _transformLookup.Update(ref state);
            _cooldownLookup.Update(ref state);

            foreach (var (job, visual) in SystemAPI
                         .Query<RefRO<VillagerJob>, RefRW<VillagerVisualState>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithNone<SettlementVillagerState, VillagerAIState>())
            {
                ApplyFromJob(job.ValueRO, ref visual.ValueRW);
            }

            foreach (var (jobState, navigation, visual, transform) in SystemAPI
                         .Query<RefRO<LocalVillagerJobState>, RefRO<LocalNavigation>, RefRW<VillagerVisualState>, RefRW<LocalTransform>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithNone<SettlementVillagerState, VillagerAIState, VillagerJob>())
            {
                ApplyFromJobState(jobState.ValueRO, ref visual.ValueRW);

                if (!IsJobStateTraveling(jobState.ValueRO))
                {
                    continue;
                }

                var current = transform.ValueRO.Position;
                var targetPos = navigation.ValueRO.Destination;
                targetPos.y = current.y;

                var toTarget = targetPos - current;
                toTarget.y = 0f;
                if (math.lengthsq(toTarget) <= 0.0001f)
                {
                    continue;
                }

                var rotation = quaternion.LookRotationSafe(math.normalize(toTarget), new float3(0f, 1f, 0f));
                var updated = transform.ValueRO;
                updated.Rotation = rotation;
                transform.ValueRW = updated;
            }

            foreach (var (ai, visual, transform, entity) in SystemAPI
                         .Query<RefRO<VillagerAIState>, RefRW<VillagerVisualState>, RefRW<LocalTransform>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithNone<SettlementVillagerState>()
                         .WithEntityAccess())
            {
                ApplyFromAI(ai.ValueRO, ref visual.ValueRW);

                if (ai.ValueRO.CurrentState != VillagerAIState.State.Travelling &&
                    ai.ValueRO.CurrentState != VillagerAIState.State.Fleeing)
                {
                    continue;
                }

                float3 targetPos;
                if (ai.ValueRO.TargetEntity != Entity.Null && _transformLookup.HasComponent(ai.ValueRO.TargetEntity))
                {
                    targetPos = _transformLookup[ai.ValueRO.TargetEntity].Position;
                }
                else
                {
                    targetPos = ai.ValueRO.TargetPosition;
                }

                var current = transform.ValueRO.Position;
                targetPos.y = current.y;

                var toTarget = targetPos - current;
                toTarget.y = 0f;
                if (math.lengthsq(toTarget) <= 0.0001f)
                {
                    continue;
                }

                var rotation = quaternion.LookRotationSafe(math.normalize(toTarget), new float3(0f, 1f, 0f));
                var updated = transform.ValueRO;
                updated.Rotation = rotation;
                transform.ValueRW = updated;
            }

            var timeSeconds = timeState.WorldSeconds;
            foreach (var (job, visual, renderTint, entity) in SystemAPI
                         .Query<RefRO<VillagerJob>, RefRO<VillagerVisualState>, RefRW<RenderTint>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithNone<SettlementVillagerState, VillagerAIState, LocalVillagerJobState>()
                         .WithEntityAccess())
            {
                var baseTint = visual.ValueRO.AlignmentTint;
                var cooldownMode = _cooldownLookup.HasComponent(entity)
                    ? _cooldownLookup[entity].Mode
                    : VillagerWorkCooldownMode.None;
                var cue = VillagerTaskCueUtility.ResolveCue(job.ValueRO, cooldownMode);
                renderTint.ValueRW.Value = VillagerTaskCueUtility.ApplyCue(baseTint, cue, timeSeconds, entity.Index);
            }

            foreach (var (jobState, visual, renderTint, entity) in SystemAPI
                         .Query<RefRO<LocalVillagerJobState>, RefRO<VillagerVisualState>, RefRW<RenderTint>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithNone<SettlementVillagerState, VillagerAIState, VillagerJob>()
                         .WithEntityAccess())
            {
                var baseTint = visual.ValueRO.AlignmentTint;
                var cooldownMode = _cooldownLookup.HasComponent(entity)
                    ? _cooldownLookup[entity].Mode
                    : VillagerWorkCooldownMode.None;
                var cue = VillagerTaskCueUtility.ResolveCue(jobState.ValueRO, cooldownMode);
                renderTint.ValueRW.Value = VillagerTaskCueUtility.ApplyCue(baseTint, cue, timeSeconds, entity.Index);
            }

            foreach (var (ai, visual, renderTint, entity) in SystemAPI
                         .Query<RefRO<VillagerAIState>, RefRO<VillagerVisualState>, RefRW<RenderTint>>()
                         .WithAll<VillagerPresentationTag>()
                         .WithNone<SettlementVillagerState>()
                         .WithEntityAccess())
            {
                var baseTint = visual.ValueRO.AlignmentTint;
                var cue = VillagerTaskCueUtility.ResolveCue(ai.ValueRO);
                renderTint.ValueRW.Value = VillagerTaskCueUtility.ApplyCue(baseTint, cue, timeSeconds, entity.Index);
            }
        }

        private static void ApplyFromAI(in VillagerAIState ai, ref VillagerVisualState visualState)
        {
            visualState.TaskIconIndex = ai.CurrentState switch
            {
                VillagerAIState.State.Travelling => 1,
                VillagerAIState.State.Working => 2,
                VillagerAIState.State.Eating => 3,
                VillagerAIState.State.Sleeping => 4,
                VillagerAIState.State.Fleeing => 5,
                VillagerAIState.State.Fighting => 6,
                VillagerAIState.State.Dead => 7,
                _ => 0
            };

            bool traveling = ai.CurrentState == VillagerAIState.State.Travelling || ai.CurrentState == VillagerAIState.State.Fleeing;
            bool working = ai.CurrentState == VillagerAIState.State.Working || ai.CurrentState == VillagerAIState.State.Fighting;

            visualState.TaskState = traveling
                ? (int)VillagerTaskState.Traveling
                : working
                    ? (int)VillagerTaskState.Working
                    : (int)VillagerTaskState.Idle;

            visualState.AnimationState = traveling ? 1 : working ? 2 : 0;
        }

        private static void ApplyFromJobState(in LocalVillagerJobState job, ref VillagerVisualState visualState)
        {
            visualState.TaskIconIndex = (int)job.Type;

            bool traveling = IsJobStateTraveling(job);
            bool working = job.Phase == LocalJobPhase.Gather || job.Phase == LocalJobPhase.Deliver;

            visualState.TaskState = traveling
                ? (int)VillagerTaskState.Traveling
                : working
                    ? (int)VillagerTaskState.Working
                    : (int)VillagerTaskState.Idle;

            visualState.AnimationState = traveling ? 1 : working ? 2 : 0;
        }

        private static bool IsJobStateTraveling(in LocalVillagerJobState job)
        {
            return job.Phase == LocalJobPhase.NavigateToNode || job.Phase == LocalJobPhase.NavigateToStorehouse;
        }

        private static void ApplyFromJob(in VillagerJob job, ref VillagerVisualState visualState)
        {
            visualState.TaskIconIndex = (int)job.Type;

            bool traveling = job.Phase == VillagerJob.JobPhase.Gathering || job.Phase == VillagerJob.JobPhase.Delivering;
            bool working = job.Phase == VillagerJob.JobPhase.Building ||
                           job.Phase == VillagerJob.JobPhase.Crafting ||
                           job.Phase == VillagerJob.JobPhase.Fighting;

            visualState.TaskState = traveling
                ? (int)VillagerTaskState.Traveling
                : working
                    ? (int)VillagerTaskState.Working
                    : (int)VillagerTaskState.Idle;

            visualState.AnimationState = traveling ? 1 : working ? 2 : 0;
        }

        private float ResolveDeltaTime(in TimeState timeState)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                return 0f;
            }

            var deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;

            if (deltaTicks == 0u)
            {
                return 0f;
            }

            var fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return fixedDt * deltaTicks;
        }

        public void OnDestroy(ref SystemState state) { }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_VillageCenterPresentationSystem : ISystem
    {
        private uint _lastTick;
        private byte _tickInitialized;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillageCenterPresentationTag>();
            state.RequireForUpdate<Village>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();

            _tickInitialized = 0;
            _lastTick = 0;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!RuntimeMode.IsRenderingEnabled)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var deltaTime = ResolveDeltaTime(timeState);
            if (deltaTime <= 0f)
            {
                return;
            }

            var timeSeconds = timeState.WorldSeconds;

            foreach (var (village, visual, entity) in SystemAPI
                         .Query<RefRO<Village>, RefRW<VillageCenterVisualState>>()
                         .WithAll<VillageCenterPresentationTag>()
                         .WithEntityAccess())
            {
                var phaseTint = GetPhaseColor(village.ValueRO.Phase);
                visual.ValueRW.PhaseTint = phaseTint;
                visual.ValueRW.InfluenceRadius = village.ValueRO.InfluenceRadius;

                // Keep intensity meaningful even without a dedicated shader hook.
                visual.ValueRW.Intensity = village.ValueRO.Phase == VillagePhase.Crisis ? 1.15f : 1f;
            }

            foreach (var (village, visual, renderTint, entity) in SystemAPI
                         .Query<RefRO<Village>, RefRO<VillageCenterVisualState>, RefRW<RenderTint>>()
                         .WithAll<VillageCenterPresentationTag>()
                         .WithEntityAccess())
            {
                var pulse = 0.92f + 0.12f * math.sin(timeSeconds * 2.4f + entity.Index * 0.07f);
                var color = visual.ValueRO.PhaseTint;
                renderTint.ValueRW.Value = new float4(color.xyz * pulse, 1f);
            }
        }

        private static float4 GetPhaseColor(VillagePhase phase)
        {
            return phase switch
            {
                VillagePhase.Forming => new float4(0.55f, 0.55f, 0.55f, 1f),
                VillagePhase.Growing => new float4(0.25f, 0.85f, 0.25f, 1f),
                VillagePhase.Stable => new float4(0.25f, 0.45f, 0.85f, 1f),
                VillagePhase.Expanding => new float4(0.25f, 0.90f, 0.90f, 1f),
                VillagePhase.Crisis => new float4(0.95f, 0.25f, 0.25f, 1f),
                VillagePhase.Declining => new float4(0.65f, 0.35f, 0.15f, 1f),
                _ => new float4(1f, 1f, 1f, 1f)
            };
        }

        private float ResolveDeltaTime(in TimeState timeState)
        {
            var tick = timeState.Tick;
            if (_tickInitialized == 0)
            {
                _tickInitialized = 1;
                _lastTick = tick;
                return 0f;
            }

            var deltaTicks = tick >= _lastTick ? tick - _lastTick : 0u;
            _lastTick = tick;

            if (deltaTicks == 0u)
            {
                return 0f;
            }

            var fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            return fixedDt * deltaTicks;
        }

        public void OnDestroy(ref SystemState state) { }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_ResourceChunkPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResourceChunkPresentationTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Placeholder for resource chunk presentation bridge.
        }

        public void OnDestroy(ref SystemState state) { }
    }

    internal enum VillagerTaskCue : byte
    {
        Idle = 0,
        Social = 1,
        HeadingToWork = 2,
        Working = 3,
        Delivering = 4
    }

    internal static class VillagerTaskCueUtility
    {
        public static VillagerTaskCue ResolveCue(SettlementVillagerPhase phase, VillagerWorkCooldownMode cooldownMode)
        {
            return phase switch
            {
                SettlementVillagerPhase.ToResource => VillagerTaskCue.HeadingToWork,
                SettlementVillagerPhase.Harvest => VillagerTaskCue.Working,
                SettlementVillagerPhase.ToDepot => VillagerTaskCue.Delivering,
                SettlementVillagerPhase.Resting => ResolveIdleCue(cooldownMode),
                SettlementVillagerPhase.Idle => ResolveIdleCue(cooldownMode),
                _ => VillagerTaskCue.Idle
            };
        }

        public static VillagerTaskCue ResolveCue(in LocalVillagerJobState job, VillagerWorkCooldownMode cooldownMode)
        {
            return job.Phase switch
            {
                LocalJobPhase.NavigateToNode => VillagerTaskCue.HeadingToWork,
                LocalJobPhase.Gather => VillagerTaskCue.Working,
                LocalJobPhase.NavigateToStorehouse => VillagerTaskCue.Delivering,
                LocalJobPhase.Deliver => VillagerTaskCue.Delivering,
                LocalJobPhase.Idle => ResolveIdleCue(cooldownMode),
                _ => VillagerTaskCue.Idle
            };
        }

        public static VillagerTaskCue ResolveCue(in VillagerJob job, VillagerWorkCooldownMode cooldownMode)
        {
            return job.Phase switch
            {
                VillagerJob.JobPhase.Assigned => VillagerTaskCue.HeadingToWork,
                VillagerJob.JobPhase.Gathering => VillagerTaskCue.HeadingToWork,
                VillagerJob.JobPhase.Delivering => VillagerTaskCue.Delivering,
                VillagerJob.JobPhase.Building => VillagerTaskCue.Working,
                VillagerJob.JobPhase.Crafting => VillagerTaskCue.Working,
                VillagerJob.JobPhase.Fighting => VillagerTaskCue.Working,
                VillagerJob.JobPhase.Idle => ResolveIdleCue(cooldownMode),
                _ => VillagerTaskCue.Idle
            };
        }

        public static VillagerTaskCue ResolveCue(in VillagerAIState ai)
        {
            return ai.CurrentState switch
            {
                VillagerAIState.State.Travelling => VillagerTaskCue.HeadingToWork,
                VillagerAIState.State.Working => VillagerTaskCue.Working,
                VillagerAIState.State.Fighting => VillagerTaskCue.Working,
                VillagerAIState.State.Fleeing => VillagerTaskCue.HeadingToWork,
                VillagerAIState.State.Idle => ai.CurrentGoal == VillagerAIState.Goal.Socialize
                    ? VillagerTaskCue.Social
                    : VillagerTaskCue.Idle,
                VillagerAIState.State.Eating => VillagerTaskCue.Idle,
                VillagerAIState.State.Sleeping => VillagerTaskCue.Idle,
                _ => VillagerTaskCue.Idle
            };
        }

        public static float4 ApplyCue(float4 baseTint, VillagerTaskCue cue, float timeSeconds, int entityIndex)
        {
            var cueTint = ResolveCueTint(cue);
            var pulse = ResolveCuePulse(cue, timeSeconds, entityIndex);
            var blended = baseTint.xyz * (0.4f + 0.6f * cueTint.xyz);
            return new float4(blended * pulse, baseTint.w);
        }

        private static VillagerTaskCue ResolveIdleCue(VillagerWorkCooldownMode cooldownMode)
        {
            return cooldownMode == VillagerWorkCooldownMode.Socialize
                ? VillagerTaskCue.Social
                : VillagerTaskCue.Idle;
        }

        private static float4 ResolveCueTint(VillagerTaskCue cue)
        {
            return cue switch
            {
                VillagerTaskCue.HeadingToWork => new float4(0.35f, 0.85f, 1f, 1f),
                VillagerTaskCue.Working => new float4(0.35f, 1f, 0.45f, 1f),
                VillagerTaskCue.Delivering => new float4(1f, 0.85f, 0.25f, 1f),
                VillagerTaskCue.Social => new float4(0.95f, 0.6f, 0.95f, 1f),
                _ => new float4(0.85f, 0.85f, 0.9f, 1f)
            };
        }

        private static float ResolveCuePulse(VillagerTaskCue cue, float timeSeconds, int entityIndex)
        {
            return cue switch
            {
                VillagerTaskCue.HeadingToWork => 0.88f + 0.18f * math.sin(timeSeconds * 5.4f + entityIndex * 0.1f),
                VillagerTaskCue.Delivering => 0.90f + 0.16f * math.sin(timeSeconds * 4.2f + entityIndex * 0.08f),
                VillagerTaskCue.Working => 0.95f + 0.08f * math.sin(timeSeconds * 3.2f + entityIndex * 0.05f),
                VillagerTaskCue.Social => 0.92f + 0.14f * math.sin(timeSeconds * 2.6f + entityIndex * 0.07f),
                _ => 1f
            };
        }
    }
}
