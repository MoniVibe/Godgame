using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Demo
{
    /// <summary>
    /// Drives a lightweight gather/deliver loop so villagers constantly move between resource nodes and the depot.
    /// Keeps PureDOTS villager analytics flowing while we stand up a more complete gameplay loop.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(VillagerSystemGroup), OrderFirst = true)]
    public partial struct DemoVillagerBehaviorSystem : ISystem
    {
        private ComponentLookup<DemoSettlementRuntime> _settlementLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        private BufferLookup<DemoSettlementResource> _resourceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DemoSettlementConfig>();
            state.RequireForUpdate<TimeState>();

            _settlementLookup = state.GetComponentLookup<DemoSettlementRuntime>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _resourceLookup = state.GetBufferLookup<DemoSettlementResource>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            _settlementLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _resourceLookup.Update(ref state);

            var deltaTime = timeState.FixedDeltaTime;

            foreach (var (demoState, ai, job, ticket, flags, availability, transform, entity) in SystemAPI
                         .Query<RefRW<DemoVillagerState>, RefRW<VillagerAIState>, RefRW<VillagerJob>, RefRW<VillagerJobTicket>, RefRW<VillagerFlags>, RefRW<VillagerAvailability>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                var stateData = demoState.ValueRO;
                if (stateData.Settlement == Entity.Null ||
                    !_settlementLookup.HasComponent(stateData.Settlement) ||
                    _settlementLookup[stateData.Settlement].HasSpawned == 0 ||
                    !_resourceLookup.TryGetBuffer(stateData.Settlement, out var resources) ||
                    resources.Length == 0)
                {
                    continue;
                }

                var runtime = _settlementLookup[stateData.Settlement];
                var random = new Unity.Mathematics.Random(math.max(1u, stateData.RandomState));
                var updatedState = stateData;
                var aiState = ai.ValueRW;
                var jobValue = job.ValueRW;
                var ticketValue = ticket.ValueRW;
                var flagsValue = flags.ValueRW;
                var availabilityValue = availability.ValueRW;

                switch (updatedState.Phase)
                {
                    case DemoVillagerPhase.Idle:
                        updatedState.CurrentResourceNode = PickResource(resources, ref random);
                        if (updatedState.CurrentResourceNode == Entity.Null)
                        {
                            break;
                        }

                        BeginWork(ref updatedState, ref aiState, ref jobValue, ref ticketValue, ref flagsValue, ref availabilityValue,
                            updatedState.CurrentResourceNode, timeState.Tick);
                        updatedState.Phase = DemoVillagerPhase.ToResource;
                        break;

                    case DemoVillagerPhase.ToResource:
                        if (!TryGetPosition(in updatedState.CurrentResourceNode, ref _transformLookup, out var gatherPos))
                        {
                            updatedState.Phase = DemoVillagerPhase.Idle;
                            break;
                        }

                        if (HasReached(transform.ValueRO.Position, gatherPos))
                        {
                            updatedState.Phase = DemoVillagerPhase.Harvest;
                            updatedState.PhaseTimer = random.NextFloat(1.2f, 2.8f);
                            aiState.CurrentState = VillagerAIState.State.Working;
                            jobValue.Phase = VillagerJob.JobPhase.Gathering;
                            jobValue.LastStateChangeTick = timeState.Tick;
                            ticketValue.Phase = (byte)VillagerJob.JobPhase.Gathering;
                            ticketValue.LastProgressTick = timeState.Tick;
                        }
                        break;

                    case DemoVillagerPhase.Harvest:
                        updatedState.PhaseTimer -= deltaTime;
                        if (updatedState.PhaseTimer <= 0f)
                        {
                            updatedState.CurrentDepot = ResolveDepot(in runtime);
                            if (updatedState.CurrentDepot == Entity.Null)
                            {
                                updatedState.Phase = DemoVillagerPhase.Resting;
                                updatedState.PhaseTimer = random.NextFloat(1f, 2f);
                                break;
                            }

                            aiState.CurrentState = VillagerAIState.State.Travelling;
                            aiState.TargetEntity = updatedState.CurrentDepot;
                            aiState.StateTimer = 0f;
                            aiState.StateStartTick = timeState.Tick;

                            jobValue.Phase = VillagerJob.JobPhase.Delivering;
                            jobValue.LastStateChangeTick = timeState.Tick;
                            ticketValue.ResourceEntity = updatedState.CurrentDepot;
                            ticketValue.Phase = (byte)VillagerJob.JobPhase.Delivering;
                            ticketValue.LastProgressTick = timeState.Tick;
                            updatedState.Phase = DemoVillagerPhase.ToDepot;
                        }
                        break;

                    case DemoVillagerPhase.ToDepot:
                        if (!TryGetPosition(in updatedState.CurrentDepot, ref _transformLookup, out var depotPos))
                        {
                            updatedState.Phase = DemoVillagerPhase.Resting;
                            updatedState.PhaseTimer = random.NextFloat(1f, 2f);
                            break;
                        }

                        if (HasReached(transform.ValueRO.Position, depotPos))
                        {
                            updatedState.Phase = DemoVillagerPhase.Resting;
                            updatedState.PhaseTimer = random.NextFloat(1f, 2f);
                            aiState.CurrentState = VillagerAIState.State.Travelling;
                            jobValue.Phase = VillagerJob.JobPhase.Completed;
                            jobValue.LastStateChangeTick = timeState.Tick;
                            ticketValue.Phase = (byte)VillagerJob.JobPhase.Completed;
                            ticketValue.LastProgressTick = timeState.Tick;
                        }
                        break;

                    case DemoVillagerPhase.Resting:
                        updatedState.PhaseTimer -= deltaTime;
                        if (updatedState.PhaseTimer <= 0f)
                        {
                            FinishRest(ref updatedState, ref aiState, ref jobValue, ref ticketValue, ref flagsValue, ref availabilityValue, timeState.Tick);
                        }
                        break;
                }

                updatedState.RandomState = random.state;
                demoState.ValueRW = updatedState;
                ai.ValueRW = aiState;
                job.ValueRW = jobValue;
                ticket.ValueRW = ticketValue;
                flags.ValueRW = flagsValue;
                availability.ValueRW = availabilityValue;
            }
        }

        private static Entity PickResource(DynamicBuffer<DemoSettlementResource> resources, ref Unity.Mathematics.Random random)
        {
            if (resources.Length == 0)
            {
                return Entity.Null;
            }

            var index = random.NextInt(0, resources.Length);
            return resources[index].Node;
        }

        private static Entity ResolveDepot(in DemoSettlementRuntime runtime)
        {
            if (runtime.StorehouseInstance != Entity.Null)
            {
                return runtime.StorehouseInstance;
            }

            if (runtime.HousingInstance != Entity.Null)
            {
                return runtime.HousingInstance;
            }

            if (runtime.VillageCenterInstance != Entity.Null)
            {
                return runtime.VillageCenterInstance;
            }

            return runtime.WorshipInstance;
        }

        private static bool TryGetPosition(in Entity entity, ref ComponentLookup<LocalTransform> lookup, out float3 position)
        {
            if (entity != Entity.Null && lookup.HasComponent(entity))
            {
                position = lookup[entity].Position;
                return true;
            }

            position = float3.zero;
            return false;
        }

        private static bool HasReached(float3 current, float3 target)
        {
            return math.distancesq(current, target) <= 0.36f;
        }

        private static void BeginWork(ref DemoVillagerState stateData,
            ref VillagerAIState aiState,
            ref VillagerJob job,
            ref VillagerJobTicket ticket,
            ref VillagerFlags flags,
            ref VillagerAvailability availability,
            Entity targetNode,
            uint tick)
        {
            aiState.CurrentGoal = VillagerAIState.Goal.Work;
            aiState.CurrentState = VillagerAIState.State.Travelling;
            aiState.TargetEntity = targetNode;
            aiState.StateTimer = 0f;
            aiState.StateStartTick = tick;

            job.Type = VillagerJob.JobType.Gatherer;
            job.Phase = VillagerJob.JobPhase.Assigned;
            job.Productivity = 1f;
            job.LastStateChangeTick = tick;

            ticket.JobType = VillagerJob.JobType.Gatherer;
            ticket.ResourceEntity = targetNode;
            ticket.Phase = (byte)VillagerJob.JobPhase.Assigned;
            ticket.LastProgressTick = tick;
            if (ticket.TicketId == 0)
            {
                ticket.TicketId = (uint)(tick + 1u);
            }
            job.ActiveTicketId = ticket.TicketId;

            flags.IsIdle = false;
            flags.IsWorking = true;

            availability.IsAvailable = 0;
            availability.LastChangeTick = tick;
        }

        private static void FinishRest(ref DemoVillagerState stateData,
            ref VillagerAIState aiState,
            ref VillagerJob job,
            ref VillagerJobTicket ticket,
            ref VillagerFlags flags,
            ref VillagerAvailability availability,
            uint tick)
        {
            stateData.Phase = DemoVillagerPhase.Idle;
            stateData.CurrentDepot = Entity.Null;
            stateData.CurrentResourceNode = Entity.Null;

            aiState.CurrentGoal = VillagerAIState.Goal.None;
            aiState.CurrentState = VillagerAIState.State.Idle;
            aiState.TargetEntity = Entity.Null;
            aiState.StateTimer = 0f;
            aiState.StateStartTick = tick;

            job.Phase = VillagerJob.JobPhase.Idle;
            job.LastStateChangeTick = tick;
            job.ActiveTicketId = 0;
            ticket.ResourceEntity = Entity.Null;
            ticket.Phase = (byte)VillagerJob.JobPhase.Idle;
            ticket.LastProgressTick = tick;

            flags.IsIdle = true;
            flags.IsWorking = false;

            availability.IsAvailable = 1;
            availability.LastChangeTick = tick;
        }
    }
}
