using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villages
{
    /// <summary>
    /// Village-level AI decision-making system.
    /// Makes autonomous decisions about resource allocation, building expansion, and crisis response.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillageAIDecisionSystem : ISystem
    {
        private ComponentLookup<VillagerNeeds> _needsLookup;
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            _needsLookup = state.GetComponentLookup<VillagerNeeds>(true);
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

            _needsLookup.Update(ref state);
            _transformLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (village, members, resources, decision, expansionRequests, entity) in SystemAPI.Query<
                RefRO<Village>,
                DynamicBuffer<VillageMember>,
                DynamicBuffer<VillageResource>,
                RefRW<VillageAIDecision>,
                DynamicBuffer<VillageExpansionRequest>>()
                .WithEntityAccess())
            {
                var villageValue = village.ValueRO;
                var decisionValue = decision.ValueRW;

                // Update decision if expired or none exists
                if (decisionValue.DecisionType == 0 || 
                    (timeState.Tick - decisionValue.DecisionTick) * timeState.FixedDeltaTime > decisionValue.DecisionDuration)
                {
                    // Calculate village needs
                    var totalResources = 0;
                    foreach (var resource in resources)
                    {
                        totalResources += resource.Quantity;
                    }

                    var averageEnergy = 0f;
                    var averageMorale = 0f;
                    var memberCount = 0;
                    foreach (var member in members)
                    {
                        if (_needsLookup.HasComponent(member.VillagerEntity))
                        {
                            var needs = _needsLookup[member.VillagerEntity];
                            averageEnergy += needs.Energy;
                            averageMorale += needs.Morale;
                            memberCount++;
                        }
                    }

                    if (memberCount > 0)
                    {
                        averageEnergy /= memberCount;
                        averageMorale /= memberCount;
                    }

                    // Decision logic based on village state
                    byte priority = 0;
                    byte decisionType = 0;
                    Entity targetEntity = Entity.Null;
                    float3 targetPosition = villageValue.CenterPosition;

                    // Low resources -> prioritize gathering
                    if (totalResources < 100 && memberCount > 0)
                    {
                        priority = 80;
                        decisionType = 4; // Gather
                    }
                    // Low morale -> prioritize expansion/improvement
                    else if (averageMorale < 500f && villageValue.Phase == VillagePhase.Growing)
                    {
                        priority = 60;
                        decisionType = 2; // Expand
                        // Add expansion request
                        expansionRequests.Add(new VillageExpansionRequest
                        {
                            BuildingType = 1, // Housing
                            Position = villageValue.CenterPosition + new float3(5f, 0f, 0f),
                            Priority = 60,
                            RequestTick = timeState.Tick
                        });
                    }
                    // Growing phase -> prioritize expansion
                    else if (villageValue.Phase == VillagePhase.Growing && totalResources > 200)
                    {
                        priority = 50;
                        decisionType = 2; // Expand
                        expansionRequests.Add(new VillageExpansionRequest
                        {
                            BuildingType = 2, // Storehouse
                            Position = villageValue.CenterPosition + new float3(-5f, 0f, 0f),
                            Priority = 50,
                            RequestTick = timeState.Tick
                        });
                    }
                    // Stable phase -> maintain status quo
                    else if (villageValue.Phase == VillagePhase.Stable)
                    {
                        priority = 30;
                        decisionType = 0; // None (maintain)
                    }

                    // Update decision
                    decisionValue.CurrentPriority = priority;
                    decisionValue.DecisionType = decisionType;
                    decisionValue.TargetEntity = targetEntity;
                    decisionValue.TargetPosition = targetPosition;
                    decisionValue.DecisionTick = timeState.Tick;
                    decisionValue.DecisionDuration = 10f; // 10 seconds
                    decision.ValueRW = decisionValue;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

