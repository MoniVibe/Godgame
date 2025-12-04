using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Handles villager interrupts (hand pickup, path blocked, combat, etc.)
    /// and manages state transitions between normal job behavior and interrupt handling.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerNeedsSystem))]
    public partial struct VillagerInterruptSystem : ISystem
    {
        private ComponentLookup<VillagerJob> _jobLookup;
        private ComponentLookup<VillagerJobTicket> _ticketLookup;
        private ComponentLookup<VillagerFlags> _flagsLookup;
        private ComponentLookup<VillagerAvailability> _availabilityLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            _jobLookup = state.GetComponentLookup<VillagerJob>(false);
            _ticketLookup = state.GetComponentLookup<VillagerJobTicket>(false);
            _flagsLookup = state.GetComponentLookup<VillagerFlags>(false);
            _availabilityLookup = state.GetComponentLookup<VillagerAvailability>(false);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);

            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            _jobLookup.Update(ref state);
            _ticketLookup.Update(ref state);
            _flagsLookup.Update(ref state);
            _availabilityLookup.Update(ref state);
            _transformLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Process hand pickup interrupts
            foreach (var (interrupt, interruptState, entity) in SystemAPI.Query<
                RefRO<VillagerInterrupt>,
                RefRW<VillagerInterruptState>>()
                .WithAll<HandCarriedTag>()
                .WithEntityAccess())
            {
                var interruptValue = interrupt.ValueRO;
                if (interruptValue.Type == VillagerInterruptType.HandPickup)
                {
                    // Save current job state if not already saved
                    if (_jobLookup.HasComponent(entity))
                    {
                        var job = _jobLookup[entity];
                        var savedState = interruptState.ValueRO;
                        if (savedState.SavedPhase == VillagerJob.JobPhase.Idle)
                        {
                            savedState.SavedPhase = job.Phase;
                            savedState.SavedType = job.Type;
                            savedState.SavedTicketId = job.ActiveTicketId;
                            if (_ticketLookup.HasComponent(entity))
                            {
                                savedState.SavedTargetEntity = _ticketLookup[entity].ResourceEntity;
                            }
                            interruptState.ValueRW = savedState;
                        }

                        // Clear job state
                        var jobValue = job;
                        jobValue.Phase = VillagerJob.JobPhase.Idle;
                        jobValue.ActiveTicketId = 0;
                        _jobLookup[entity] = jobValue;

                        if (_flagsLookup.HasComponent(entity))
                        {
                            var flags = _flagsLookup[entity];
                            flags.IsIdle = true;
                            flags.IsWorking = false;
                            _flagsLookup[entity] = flags;
                        }

                        if (_availabilityLookup.HasComponent(entity))
                        {
                            var availability = _availabilityLookup[entity];
                            availability.IsAvailable = 0;
                            availability.LastChangeTick = timeState.Tick;
                            _availabilityLookup[entity] = availability;
                        }
                    }
                }
            }

            // Process path blocked interrupts
            foreach (var (interrupt, blocked, entity) in SystemAPI.Query<
                RefRW<VillagerInterrupt>,
                RefRO<PathBlockedTag>>()
                .WithEntityAccess())
            {
                var interruptValue = interrupt.ValueRO;
                var blockedValue = blocked.ValueRO;

                // Check if blocked for too long (5 seconds)
                if (timeState.Tick - blockedValue.BlockedTick > 300) // ~5 seconds @60fps
                {
                    // Try to resolve by clearing path blocked state
                    interruptValue.Type = VillagerInterruptType.None;
                    interruptValue.SourceEntity = Entity.Null;
                    interrupt.ValueRW = interruptValue;

                    // Remove path blocked tag
                    ecb.RemoveComponent<PathBlockedTag>(entity);

                    // Resume job if we have saved state
                    if (state.EntityManager.HasComponent<VillagerInterruptState>(entity))
                    {
                        var savedState = state.EntityManager.GetComponentData<VillagerInterruptState>(entity);
                        if (_jobLookup.HasComponent(entity))
                        {
                            var job = _jobLookup[entity];
                            job.Phase = savedState.SavedPhase;
                            job.Type = savedState.SavedType;
                            job.ActiveTicketId = savedState.SavedTicketId;
                            _jobLookup[entity] = job;
                        }
                    }
                }
            }

            // Process interrupt timeouts
            foreach (var (interrupt, interruptState, entity) in SystemAPI.Query<
                RefRW<VillagerInterrupt>,
                RefRW<VillagerInterruptState>>()
                .WithEntityAccess())
            {
                var interruptValue = interrupt.ValueRO;
                if (interruptValue.Type != VillagerInterruptType.None && interruptValue.Duration > 0f)
                {
                    var elapsed = (timeState.Tick - interruptValue.StartTick) * timeState.FixedDeltaTime;
                    if (elapsed >= interruptValue.Duration)
                    {
                        // Interrupt expired, resume saved state
                        var savedState = interruptState.ValueRO;
                        if (_jobLookup.HasComponent(entity))
                        {
                            var job = _jobLookup[entity];
                            job.Phase = savedState.SavedPhase;
                            job.Type = savedState.SavedType;
                            job.ActiveTicketId = savedState.SavedTicketId;
                            _jobLookup[entity] = job;
                        }

                        // Clear interrupt
                        interruptValue.Type = VillagerInterruptType.None;
                        interruptValue.SourceEntity = Entity.Null;
                        interrupt.ValueRW = interruptValue;

                        // Clear saved state
                        ecb.RemoveComponent<VillagerInterruptState>(entity);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

